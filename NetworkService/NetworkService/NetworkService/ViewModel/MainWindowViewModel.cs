using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Views;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {
        #region SetUp
        public static ObservableCollection<ReactorTemperature> ReactorTemperatures { get; set; }

        public static Stack<SaveState<CommandType, object>> UndoStack { get; set; }

        private object _selectedContent;
        public object SelectedContent
        {
            get => _selectedContent;
            set
            {
                SetProperty(ref _selectedContent, value);
                UndoCommand.RaiseCanExecuteChanged();
            }
        }

        public static Mutex Mutex { get; set; } = new Mutex();

        public MyICommand<string> ChangeViewCommand { get; set; }
        public MyICommand UndoCommand { get; set; }
        public MyICommand QuitCommand { get; set; }
        public MyICommand CycleTabsCommand { get; set; }
        #endregion

        public MainWindowViewModel()
        {
            CreateListener();

            UndoStack = new Stack<SaveState<CommandType, object>>();

            ReactorTemperatures = XmlHelper.LoadData("entityData.xml");

            ChangeViewCommand = new MyICommand<string>(ChangeView);
            UndoCommand = new MyICommand(OnUndo, CanUndo);
            QuitCommand = new MyICommand(OnQuit);
            CycleTabsCommand = new MyICommand(OnCycleTabs);

            SelectedContent = new EntitiesView();

        }

        private void OnCycleTabs()
        {
            Type viewType = SelectedContent.GetType();
            UndoStack.Push(new SaveState<CommandType, object>(CommandType.SwitchView, viewType));
            if (viewType == typeof(EntitiesView))
            {
                SelectedContent = new DisplayView();
            }
            else if (viewType == typeof(DisplayView))
            {
                SelectedContent = new GraphView();
            }
            else if (viewType == typeof(GraphView))
            {
                SelectedContent = new EntitiesView();
            }
        }
        private void ChangeView(string viewName)
        {
            if (viewName == "Table" && SelectedContent.GetType() != typeof(EntitiesView))
            {
                UndoStack.Push(new SaveState<CommandType, object>(CommandType.SwitchView, SelectedContent.GetType()));
                SelectedContent = new EntitiesView();


            }
            else if (viewName == "Network" && SelectedContent.GetType() != typeof(DisplayView))
            {
                UndoStack.Push(new SaveState<CommandType, object>(CommandType.SwitchView, SelectedContent.GetType()));
                SelectedContent = new DisplayView();

            }
            else if (viewName == "Graph" && SelectedContent.GetType() != typeof(GraphView))
            {
                UndoStack.Push(new SaveState<CommandType, object>(CommandType.SwitchView, SelectedContent.GetType()));
                SelectedContent = new GraphView();

            }
        }
        private void CreateListener()
        {
            var tcp = new TcpListener(IPAddress.Loopback, 25657);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        //Prijem poruke
                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        //Primljena poruka je sacuvana u incomming stringu
                        incomming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        //Ukoliko je primljena poruka pitanje koliko objekata ima u sistemu -> odgovor
                        if (incomming.Equals("Need object count"))
                        {
                            Byte[] data = System.Text.Encoding.ASCII.GetBytes(ReactorTemperatures.Count.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            //Console.WriteLine(incomming); //"Entitet_1:272"

                            Logging.AppendToFile(@"..\..\log.txt", incomming);

                            string[] parts = incomming.Split(':');
                            int id = int.Parse(parts[0].Split('_')[1]);
                            int value = int.Parse(parts[1]);

                            if (id <= ReactorTemperatures.Count - 1)
                            {
                                ReactorTemperatures[id].Value = value;
                                AddValueToList(ReactorTemperatures[id]);
                                DisplayViewModel.UpdateEntitiesOnCanvas();
                            }

                        }
                    }, null);
                }
            });

            listeningThread.IsBackground = true;
            listeningThread.Start();
        }
        private void AddValueToList(ReactorTemperature reactorTemperature)
        {
            if (reactorTemperature.Last_5.Count == 5)
            {
                reactorTemperature.Last_5.RemoveAt(0);
                reactorTemperature.Last_5.Add(new Pair<DateTime, int>(DateTime.Now, reactorTemperature.Value));
            }
            else
            {
                reactorTemperature.Last_5.Add(new Pair<DateTime, int>(DateTime.Now, reactorTemperature.Value));
            }
        }

        #region Undo
        public bool CanUndo()
        {
            return UndoStack.Count != 0;
        }
        public void OnUndo()
        {
            SaveState<CommandType, object> saveState = UndoStack.Pop();
            if (saveState.CommandType == CommandType.SwitchView)
            {
                Type viewType = saveState.SavedState as Type;

                if (viewType == typeof(DisplayView))
                {
                    SelectedContent = new DisplayView();
                }
                else if (viewType == typeof(GraphView))
                {
                    SelectedContent = new GraphView();
                }
                else
                {
                    SelectedContent = new EntitiesView();
                }
            }
            else if (saveState.CommandType == CommandType.EntityChange)
            {
                ReactorTemperatures = saveState.SavedState as ObservableCollection<ReactorTemperature>;

                SelectedContent = new EntitiesView();
            }
            else if (saveState.CommandType == CommandType.CanvasChange)
            {
                Mutex.WaitOne();

                DisplayViewModel.AddedToGrid.Clear();
                DisplayViewModel.Lines.Clear();

                List<object> state = saveState.SavedState as List<object>;

                foreach (var entry in state[1] as Dictionary<string, Line>)
                {
                    DisplayViewModel.Lines.Add(entry.Key, entry.Value);
                }

                Mutex.ReleaseMutex();

                DisplayViewModel.InitializeCollections();
                DisplayViewModel.InitializeCategories();

                SelectedContent = new DisplayView();

            }

            GC.Collect();
            UndoCommand.RaiseCanExecuteChanged();
        }
        #endregion

        private void OnQuit()
        {
            Application.Current.MainWindow.Close();
        }
    }
}
