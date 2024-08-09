using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NetworkService.Helpers;
using System.Threading;
using System.Windows.Shapes;
using System.Windows.Navigation;
using NetworkService.Views;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;

namespace NetworkService.ViewModel
{
    public class DisplayViewModel : BindableBase
    {

        private ReactorTemperature _selectedEntity;
        public ReactorTemperature SelectedEntity
        {
            get => _selectedEntity;
            set
            {
                SetProperty(ref _selectedEntity, value);
            }
        }

        private ReactorTemperature _draggedItem = null;
        private bool _dragging = false;
        public int _draggingSourceIndex = -1;

        public static ObservableCollection<ReactorTemperature> ReactorTemperatures { get; set; }
        public static ObservableCollection<Category> Categories { get; set; }
        public static Dictionary<int, ReactorTemperature> AddedToGrid { get; set; } = new Dictionary<int, ReactorTemperature>();

        public static Dictionary<string, Line> Lines { get; set; } = new Dictionary<string, Line>();
        public ObservableCollection<Line> LinesToDisplay { get; set; }

        public static ObservableCollection<Canvas> CanvasCollection { get; set; } = new ObservableCollection<Canvas>();
        public static ObservableCollection<ReactorTemperature> EntityInfo { get; set; } = new ObservableCollection<ReactorTemperature>();
        public static ObservableCollection<Brush> BorderBrushCollection { get; set; } = new ObservableCollection<Brush>();


        public MyICommand<string> DropEntityOnCanvasCommand { get; set; }
        public MyICommand<string> MouseLeftButtonDownCommand { get; set; }
        public MyICommand MouseLeftButtonUpCommand { get; set; }
        public MyICommand<string> FreeCanvas { get; set; }
        public MyICommand<object> SelectionChangedCommand { get; set; }
        public MyICommand<string> ClearCanvasCommand { get; set; }

        public DisplayViewModel()
        {
            LinesToDisplay = new ObservableCollection<Line>();
            InitializeCollections();
            InitializeCategories();

            DrawExistingLines();

            DropEntityOnCanvasCommand = new MyICommand<string>(OnDrop);
            MouseLeftButtonDownCommand = new MyICommand<string>(OnLeftMouseButtonDown);
            MouseLeftButtonUpCommand = new MyICommand(OnLeftMouseButtonUp);
            FreeCanvas = new MyICommand<string>(ResetCanvas);
            SelectionChangedCommand = new MyICommand<object>(OnSelectionChanged);
            ClearCanvasCommand = new MyICommand<string>(ResetCanvas);


        }
        
        public static void InitializeCollections()
        { 
            if (CanvasCollection.Count == 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    Canvas canvas = new Canvas();
                    if (AddedToGrid.ContainsKey(i))
                    {
                        var logo = new BitmapImage(new Uri(AddedToGrid[i].EntityType.ImagePath, UriKind.Relative));
                        canvas.Background = new ImageBrush(logo);
                        EntityInfo.Add(AddedToGrid[i]);
                        canvas.Resources["taken"] = true;
                        canvas.Resources["data"] = AddedToGrid[i];
                    }
                    else
                    {
                        canvas.Background = Brushes.Transparent;
                        EntityInfo.Add(null);
                    }

                    canvas.AllowDrop = true;
                    CanvasCollection.Add(canvas);
                    BorderBrushCollection.Add(Brushes.Transparent);
                }
            }
            else
            {
                for (int i = 0; i < 12; i++)
                {
                    if (AddedToGrid.ContainsKey(i))
                    {
                        var logo = new BitmapImage(new Uri(AddedToGrid[i].EntityType.ImagePath, UriKind.Relative));
                        CanvasCollection[i].Background = new ImageBrush(logo);
                        CanvasCollection[i].Resources["taken"] = true;
                        CanvasCollection[i].Resources["data"] = AddedToGrid[i];
                        EntityInfo[i] = AddedToGrid[i];
                    }
                    else
                    {
                        // Clearing canvas
                        CanvasCollection[i].Background = Brushes.Transparent;
                        if (CanvasCollection[i].Resources.Contains("taken"))
                        {
                            CanvasCollection[i].Resources.Remove("taken");
                        }
                        if (CanvasCollection[i].Resources.Contains("data"))
                        {
                            CanvasCollection[i].Resources.Remove("data");
                        }
                        BorderBrushCollection[i] = Brushes.Transparent;
                        EntityInfo[i] = null;

                        // Deleting lines
                        List<int> connections = FindAllConnections(i);
                        if (connections.Count > 0)
                        {
                            foreach (int connectedTo in connections)
                            {
                                int source = Math.Min(i, connectedTo);
                                int destination = Math.Max(i, connectedTo);
                                DeleteLine(source, destination);
                            }
                        }

                    }
                }
            }
            foreach (ReactorTemperature f in AddedToGrid.Values)
            {
                RemoveFromCategory(f);
            }
        }
        public static void InitializeCategories()
        {

            ReactorTemperatures = MainWindowViewModel.ReactorTemperatures;

            // If null then ObservableCollection<Category>
            Categories = Categories ?? new ObservableCollection<Category>
            {
                new Category("RTD"),
                new Category("TermoSprega")
            };
            Categories[0].ReactorTemperatures.Clear();
            Categories[1].ReactorTemperatures.Clear();

            foreach (var reactorTemperature in ReactorTemperatures)
            {
                foreach (var category in Categories)
                {
                    if (category.Name.Equals(reactorTemperature.EntityType.Name) && !AddedToGrid.ContainsValue(reactorTemperature))
                    {
                        category.ReactorTemperatures.Add(reactorTemperature);
                    }
                }
            }
        }

        public void DrawExistingLines()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LinesToDisplay.Clear();

                var linesArray = Lines.Values.ToArray();

                for (int i = 0; i < linesArray.Length; i++)
                {
                    try
                    {
                        LinesToDisplay.Add(linesArray[i]);
                    }
                    catch(ArgumentOutOfRangeException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            });
        }

        private Line CreateNewLine(int sourceIndex, int destinationIndex)
        {
            Line newLine = new Line
            {
                X1 = ConvertToAbsoluteX(sourceIndex),
                Y1 = ConvertToAbsoluteY(sourceIndex),
                X2 = ConvertToAbsoluteX(destinationIndex),
                Y2 = ConvertToAbsoluteY(destinationIndex),
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                StrokeStartLineCap = PenLineCap.Triangle,
                StrokeEndLineCap = PenLineCap.Triangle
            };
            return newLine;
        }
        private double ConvertToAbsoluteY(int index)
        {
            index /= 3;

            return Math.Round(index * 121.375 + 60.937);
        }
        private double ConvertToAbsoluteX(int index)
        {
            index %= 3;

            return Math.Round(index * 148.333 + 74.167);
        }
        public static List<int> FindAllConnections(int index)
        {
            // "1,2", "2,4", ...
            return Lines.Keys.Select(c =>
            {
                var parts = c.Split(',');
                int index1 = int.Parse(parts[0]);
                int index2 = int.Parse(parts[1]);
                return index == index1 ? index2 : index == index2 ? index1 : (int?)null;
            })
            .Where(connectedIndex => connectedIndex.HasValue)
            .Select(connectedIndex => connectedIndex.Value)
            .ToList();
        }
        public static void DeleteLine(int index1, int index2)
        {
            string key = $"{index1},{index2}";
            if (!Lines.Remove(key))
            {
                key = $"{index2},{index1}";
                Lines.Remove(key);
            }
        }
        private int IsLineAlreadyDrawn(int sourceIndex, int destinationIndex)
        {
            return Lines.Keys.Cast<string>().Any(c =>
            {
                var parts = c.Split(',');
                int index1 = int.Parse(parts[0]);
                int index2 = int.Parse(parts[1]);
                return (sourceIndex == index1 && destinationIndex == index2) || (sourceIndex == index2 && destinationIndex == index1);
            }) ? 1 : 0;
        }

        private void OnLeftMouseButtonUp()
        {
            _dragging = false;
            _draggedItem = null;
            _draggingSourceIndex = -1;
        }
        private void OnSelectionChanged(object parameter)
        {
            if (!_dragging && parameter != null)
            {
                _dragging = true;
                _draggedItem = SelectedEntity;
                if (_draggedItem != null)
                {
                    DragDrop.DoDragDrop((ListView)parameter, _draggedItem, DragDropEffects.Move);
                }
            }
        }
        private void OnLeftMouseButtonDown(string indexString)
        {
            int index = int.Parse(indexString);
            if (!_dragging && CanvasCollection[index].Resources.Contains("taken"))
            {
                _dragging = true;
                _draggedItem = CanvasCollection[index].Resources["data"] as ReactorTemperature;
                _draggingSourceIndex = index;

                if (_draggedItem != null)
                {
                    DragDrop.DoDragDrop(CanvasCollection[index], _draggedItem, DragDropEffects.Move);
                }
            }
        }
        private void OnDrop(string indexString)
        {

            int index = int.Parse(indexString);

            if (CanvasCollection[index].Resources.Contains("data"))
            {
                // Dragged on itself
                if (_draggedItem != null && (CanvasCollection[index].Resources["data"] as ReactorTemperature).ID == _draggedItem.ID)
                {
                    _draggedItem = null;
                    _draggingSourceIndex = -1;
                    _dragging = false;
                    return;
                }
            }

            if (_draggedItem != null && !CanvasCollection[index].Resources.Contains("taken"))
            {
                SaveState();

                // Filling the canvas
                var logo = new BitmapImage(new Uri(_draggedItem.EntityType.ImagePath, UriKind.Relative));
                CanvasCollection[index].Background = new ImageBrush(logo);
                CanvasCollection[index].Resources.Add("taken", true);
                CanvasCollection[index].Resources.Add("data", _draggedItem);
                AddedToGrid.Add(index, _draggedItem);
                BorderBrushCollection[index] = _draggedItem.ValueMeasure == ValueMeasure.Normal ? Brushes.Transparent : Brushes.Red;
                EntityInfo[index] = _draggedItem;

                // If the dragged item is from a different canvas control, clear the previous one and redraw lines
                if (_draggingSourceIndex != -1)
                {
                    ResetCanvas(_draggingSourceIndex.ToString());

                    List<int> connections = FindAllConnections(_draggingSourceIndex);

                    if (connections.Count != 0)
                    {
                        foreach (int connectedTo in connections)
                        {
                            int source = Math.Min(_draggingSourceIndex, connectedTo);
                            int destination = Math.Max(_draggingSourceIndex, connectedTo);
                            DeleteLine(source, destination);

                            source = Math.Min(index, connectedTo);
                            destination = Math.Max(index, connectedTo);
                            Lines.Add($"{source},{destination}", CreateNewLine(source, destination));
                        }
                        DrawExistingLines();
                    }
                }

                // End operation
                RemoveFromCategory(_draggedItem);
                _draggingSourceIndex = -1;
                _draggedItem = null;
                _dragging = false;

            }
            // Connection with line
            else if (_draggedItem != null && CanvasCollection[index].Resources.Contains("taken"))
            {
                bool value = (IsLineAlreadyDrawn(_draggingSourceIndex, index) == 0);

                if (value)
                {
                    SaveState();

                    int source = Math.Min(_draggingSourceIndex, index);
                    int destination = Math.Max(_draggingSourceIndex, index);
                    Lines.Add($"{source},{destination}", CreateNewLine(source, destination));
                    DrawExistingLines();
                }

                // End operation
                _draggingSourceIndex = -1;
                _draggedItem = null;
                _dragging = false;
            }
        }

        // Removing content and lines from canvas
        private void ResetCanvas(string indexString)
        {

            int index = int.Parse(indexString);

            if (!CanvasCollection[index].Resources.Contains("taken"))
            {
                ToastNotify.RaiseToast("Error", "There is nothing to remove!", Notification.Wpf.NotificationType.Warning);
                return;

            }
            if (_draggingSourceIndex == -1)
            {
                SaveState();

                List<int> connections = FindAllConnections(index);

                foreach (int connectedTo in connections)
                {
                    if (connectedTo > index)
                    {
                        DeleteLine(index, connectedTo);
                    }
                    else
                    {
                        DeleteLine(connectedTo, index);
                    }
                }
                DrawExistingLines();
            }

            ReactorTemperature removedTemperature = CanvasCollection[index].Resources["data"] as ReactorTemperature;
            AddedToGrid.Remove(index);
            AddToCategory(removedTemperature);

            CanvasCollection[index].Background = Brushes.Transparent;
            CanvasCollection[index].Resources.Remove("taken");
            CanvasCollection[index].Resources.Remove("data");

            BorderBrushCollection[index] = Brushes.Transparent;
            EntityInfo[index] = null;

        }

        // Add entity back to the list
        private void AddToCategory(ReactorTemperature reactorTemperature)
        {
            foreach (Category c in Categories)
            {
                if (c.Name.Equals(reactorTemperature.EntityType.Name))
                {
                    c.ReactorTemperatures.Add(reactorTemperature);
                    break;
                }
            }
        }

        // Deeleting from list
        private static void RemoveFromCategory(ReactorTemperature reactorTemperature)
        {
            foreach (var category in Categories)
            {
                if (category.ReactorTemperatures.Contains(reactorTemperature))
                {
                    category.ReactorTemperatures.Remove(reactorTemperature);
                    break;
                }
            }
        }


        //method for updating border color based on the value
        public static void UpdateEntitiesOnCanvas()
        {
            if (CanvasCollection != null )
            {
                if(CanvasCollection.Count != 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (CanvasCollection[i].Resources.Contains("taken"))
                        {
                            if (AddedToGrid.TryGetValue(i, out ReactorTemperature reactorTemperature))
                            {
                                BorderBrushCollection[i] = reactorTemperature.ValueMeasure == ValueMeasure.Normal ? Brushes.GreenYellow : Brushes.Crimson;
                            }
                        }
                    }
                }
            }
        }

        //method for saving state before an action
        public static void SaveState()
        {

            Dictionary<int, ReactorTemperature> entityState = new Dictionary<int, ReactorTemperature>();
            foreach (var entry in AddedToGrid)
            {
                entityState.Add(entry.Key, entry.Value);
            }
            Dictionary<string, Line> lineState = new Dictionary<string, Line>();
            foreach (var entry in Lines)
            {
                lineState.Add(entry.Key, entry.Value);
            }

            List<object> state = new List<object>() { entityState, lineState };
            //pushing state onto an undo stack
            MainWindowViewModel.UndoStack.Push(
                new SaveState<CommandType, object>(CommandType.CanvasChange, state));

        }
    }
}
