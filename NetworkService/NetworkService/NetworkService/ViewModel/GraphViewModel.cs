using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NetworkService.Views;
using System.Threading;
using System.Windows.Media;

namespace NetworkService.ViewModel
{
    public class GraphViewModel : BindableBase
    {
        public ObservableCollection<ReactorTemperature> ReactorTemperatures
        {
            get;set;
        }
        private ReactorTemperature selectedTemperature;
        public ReactorTemperature SelectedTemperature
        {
            get => selectedTemperature;
            set
            {
                selectedTemperature = value;
                UpdatePositions(null);
            }
        }

        private readonly double graphCoefficient = 0.30;

        public MyICommand SelectCommand;
        public MyICommand SelectionChangedCommand { get; }

        private readonly Timer _timer;
        
        private Point startPoint;
        public Point StartPoint { get => startPoint; set => SetProperty(ref startPoint, value); }
        private Point linePoint_1;
        public Point LinePoint_1 { get => linePoint_1; set => SetProperty(ref linePoint_1, value); }
        private Point linePoint_2;
        public Point LinePoint_2 { get => linePoint_2; set => SetProperty(ref linePoint_2, value); }
        private Point linePoint_3;
        public Point LinePoint_3 { get => linePoint_3; set => SetProperty(ref linePoint_3, value); }
        private Point linePoint_4;
        public Point LinePoint_4 { get => linePoint_4; set => SetProperty(ref linePoint_4, value); }
        private Point linePoint_5;
        public Point LinePoint_5 { get => linePoint_5; set => SetProperty(ref linePoint_5, value); }
        private Thickness marginPoint_1;
        public Thickness MarginPoint_1 { get => marginPoint_1; set => SetProperty(ref marginPoint_1, value); }
        private Thickness marginPoint_2;
        public Thickness MarginPoint_2 { get => marginPoint_2; set => SetProperty(ref marginPoint_2, value); }
        private Thickness marginPoint_3;
        public Thickness MarginPoint_3 { get => marginPoint_3; set => SetProperty(ref marginPoint_3, value); }
        private Thickness marginPoint_4;
        public Thickness MarginPoint_4 { get => marginPoint_4; set => SetProperty(ref marginPoint_4, value); }
        private Thickness marginPoint_5;
        public Thickness MarginPoint_5 { get => marginPoint_5; set => SetProperty(ref marginPoint_5, value); }


        private SolidColorBrush nodeColor_1;
        public SolidColorBrush NodeColor_1 { get => nodeColor_1; set => SetProperty(ref nodeColor_1, value); }
        private SolidColorBrush nodeColor_2;
        public SolidColorBrush NodeColor_2 { get => nodeColor_2; set => SetProperty(ref nodeColor_2, value); }
        private SolidColorBrush nodeColor_3;
        public SolidColorBrush NodeColor_3 { get => nodeColor_3; set => SetProperty(ref nodeColor_3, value); }
        private SolidColorBrush nodeColor_4;
        public SolidColorBrush NodeColor_4 { get => nodeColor_4; set => SetProperty(ref nodeColor_4, value); }
        private SolidColorBrush nodeColor_5;
        public SolidColorBrush NodeColor_5 { get => nodeColor_5; set => SetProperty(ref nodeColor_5, value); }

        private string nodeText_1;
        public string NodeText_1 { get => nodeText_1; set => SetProperty(ref nodeText_1, value); }
        private string nodeText_2;
        public string NodeText_2 { get => nodeText_2; set => SetProperty(ref nodeText_2, value); }
        private string nodeText_3;
        public string NodeText_3 { get => nodeText_3; set => SetProperty(ref nodeText_3, value); }
        private string nodeText_4;
        public string NodeText_4 { get => nodeText_4; set => SetProperty(ref nodeText_4, value); }
        private string nodeText_5;
        public string NodeText_5 { get => nodeText_5; set => SetProperty(ref nodeText_5, value); }

        private string time_1;
        public string Time_1 { get => time_1; set => SetProperty(ref time_1, value); }
        private string time_2;
        public string Time_2 { get => time_2; set => SetProperty(ref time_2, value); }
        private string time_3;
        public string Time_3 { get => time_3; set => SetProperty(ref time_3, value); }
        private string time_4;
        public string Time_4 { get => time_4; set => SetProperty(ref time_4, value); }
        private string time_5;
        public string Time_5 { get => time_5; set => SetProperty(ref time_5, value); }

        public GraphViewModel()
        {
            this.ReactorTemperatures = MainWindowViewModel.ReactorTemperatures;
            if(ReactorTemperatures.Count!=0)
            {
                SelectedTemperature = ReactorTemperatures[0];
            }

            StartPoint = new Point(45, 210);

            LinePoint_1 = new Point(45,210); 
            LinePoint_2 = new Point(95, 210);
            LinePoint_3 = new Point(145, 210);
            LinePoint_4 = new Point(195,210);
            LinePoint_5 = new Point(245,210);

            MarginPoint_1 = new Thickness(30, 195, 0, 0);
            MarginPoint_2 = new Thickness(80, 195, 0, 0);
            MarginPoint_3 = new Thickness(130, 195, 0, 0);
            MarginPoint_4 = new Thickness(180, 195, 0, 0);
            MarginPoint_5 = new Thickness(230, 195, 0, 0);

            NodeColor_1 = new SolidColorBrush(Colors.Teal);
            NodeColor_2 = new SolidColorBrush(Colors.Teal);
            NodeColor_3 = new SolidColorBrush(Colors.Teal);
            NodeColor_4 = new SolidColorBrush(Colors.Teal);
            NodeColor_5 = new SolidColorBrush(Colors.Teal);

            NodeText_1 = "-";
            NodeText_2 = "-";
            NodeText_3 = "-";
            NodeText_4 = "-";
            NodeText_5 = "-";

            Time_1 = "00:00";
            Time_2 = "00:00";
            Time_3 = "00:00";
            Time_4 = "00:00";
            Time_5 = "00:00";

            _timer = new Timer(UpdatePositions, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(200));
        }

        private void UpdatePositions(object state)
        {
            if (SelectedTemperature != null && Application.Current!=null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SelectedTemperature.Last_5.Count > 0)
                    {
                        LinePoint_1 = new Point(LinePoint_1.X, (int)Math.Round(210 - SelectedTemperature.Last_5[0].Item2 * graphCoefficient));
                        StartPoint = LinePoint_1;
                        MarginPoint_1 = new Thickness(MarginPoint_1.Left, (int)Math.Round(195 - SelectedTemperature.Last_5[0].Item2 * graphCoefficient), 0, 0);
                        NodeText_1 = SelectedTemperature.Last_5[0].Item2.ToString();

                        if (SelectedTemperature.Last_5[0].Item2 < 250)
                        {
                            NodeColor_1 = new SolidColorBrush(Colors.Red);
                        }
                        else if (SelectedTemperature.Last_5[0].Item2 > 350)
                        {
                            NodeColor_1 = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            NodeColor_1 = new SolidColorBrush(Colors.Green);
                        }
                        DateTime dateTime = SelectedTemperature.Last_5[0].Item1;
                        Time_1 = dateTime.Minute.ToString() + ":" + dateTime.Second.ToString();
                    }
                    else
                    {
                        LinePoint_1 = new Point(45, 210);
                        MarginPoint_1 = new Thickness(30, 195, 0, 0);
                        NodeColor_1 = new SolidColorBrush(Colors.Teal);
                        NodeText_1 = "-";
                        Time_1 = "00:00";
                    }


                    if (SelectedTemperature.Last_5.Count > 1)
                    {
                        LinePoint_2 = new Point(LinePoint_2.X, (int)Math.Round(210 - SelectedTemperature.Last_5[1].Item2 * graphCoefficient));
                        MarginPoint_2 = new Thickness(MarginPoint_2.Left, (int)Math.Round(195 - SelectedTemperature.Last_5[1].Item2 * graphCoefficient), 0, 0);
                        NodeText_2 = SelectedTemperature.Last_5[1].Item2.ToString();

                        if (SelectedTemperature.Last_5[1].Item2 < 250)
                        {
                            NodeColor_2 = new SolidColorBrush(Colors.Red);
                        }
                        else if (SelectedTemperature.Last_5[1].Item2 > 350)
                        {
                            NodeColor_2 = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            NodeColor_2 = new SolidColorBrush(Colors.Green);
                        }
                        DateTime dateTime = SelectedTemperature.Last_5[1].Item1;
                        Time_2 = dateTime.Minute.ToString() + ":" + dateTime.Second.ToString();
                    }
                    else
                    {
                        LinePoint_2 = new Point(95, 210);
                        MarginPoint_2 = new Thickness(80, 195, 0, 0);
                        NodeColor_2 = new SolidColorBrush(Colors.Teal);
                        NodeText_2 = "-";
                        Time_2 = "00:00";
                    }

                    if (SelectedTemperature.Last_5.Count > 2)
                    {
                        LinePoint_3 = new Point(LinePoint_3.X, (int)Math.Round(210 - SelectedTemperature.Last_5[2].Item2 * graphCoefficient));
                        MarginPoint_3 = new Thickness(MarginPoint_3.Left, (int)Math.Round(195 - SelectedTemperature.Last_5[2].Item2 * graphCoefficient), 0, 0);
                        NodeText_3 = SelectedTemperature.Last_5[2].Item2.ToString();

                        if (SelectedTemperature.Last_5[2].Item2 < 250)
                        {
                            NodeColor_3 = new SolidColorBrush(Colors.Red);
                        }
                        else if (SelectedTemperature.Last_5[2].Item2 > 350)
                        {
                            NodeColor_3 = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            NodeColor_3 = new SolidColorBrush(Colors.Green);
                        }
                        DateTime dateTime = SelectedTemperature.Last_5[2].Item1;
                        Time_3 = dateTime.Minute.ToString() + ":" + dateTime.Second.ToString();
                    }
                    else
                    {
                        LinePoint_3 = new Point(145, 210);
                        MarginPoint_3 = new Thickness(130, 195, 0, 0);
                        NodeColor_3 = new SolidColorBrush(Colors.Teal);
                        NodeText_3 = "-";
                        Time_3 = "00:00";
                    }

                    if (SelectedTemperature.Last_5.Count > 3)
                    {
                        LinePoint_4 = new Point(LinePoint_4.X, (int)Math.Round(210 - SelectedTemperature.Last_5[3].Item2 * graphCoefficient));
                        MarginPoint_4 = new Thickness(MarginPoint_4.Left, (int)Math.Round(195 - SelectedTemperature.Last_5[3].Item2 * graphCoefficient), 0, 0);
                        NodeText_4 = SelectedTemperature.Last_5[3].Item2.ToString();

                        if (SelectedTemperature.Last_5[3].Item2 < 250)
                        {
                            NodeColor_4 = new SolidColorBrush(Colors.Red);
                        }
                        else if (SelectedTemperature.Last_5[3].Item2 > 350)
                        {
                            NodeColor_4 = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            NodeColor_4 = new SolidColorBrush(Colors.Green);
                        }
                        DateTime dateTime = SelectedTemperature.Last_5[3].Item1;
                        Time_4 = dateTime.Minute.ToString() + ":" + dateTime.Second.ToString();
                    }
                    else
                    {
                        LinePoint_4 = new Point(195, 210);
                        MarginPoint_4 = new Thickness(180, 195, 0, 0);
                        NodeColor_4 = new SolidColorBrush(Colors.Teal);
                        NodeText_4 = "-";
                        Time_4 = "00:00";
                    }

                    if (SelectedTemperature.Last_5.Count > 4)
                    {
                        LinePoint_5 = new Point(LinePoint_5.X, (int)Math.Round(210 - SelectedTemperature.Last_5[4].Item2 * graphCoefficient));
                        MarginPoint_5 = new Thickness(MarginPoint_5.Left, (int)Math.Round(195 - SelectedTemperature.Last_5[4].Item2 * graphCoefficient), 0, 0);
                        NodeText_5 = SelectedTemperature.Last_5[4].Item2.ToString();

                        if (SelectedTemperature.Last_5[4].Item2 < 250)
                        {
                            NodeColor_5 = new SolidColorBrush(Colors.Red);
                        }
                        else if (SelectedTemperature.Last_5[4].Item2 > 350)
                        {
                            NodeColor_5 = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            NodeColor_5 = new SolidColorBrush(Colors.Green);
                        }
                        DateTime dateTime = SelectedTemperature.Last_5[4].Item1;
                        Time_5 = dateTime.Minute.ToString() + ":" + dateTime.Second.ToString();
                    }
                    else
                    {
                        LinePoint_5 = new Point(245, 210);
                        MarginPoint_5 = new Thickness(230, 195, 0, 0);
                        NodeColor_5 = new SolidColorBrush(Colors.Teal);
                        NodeText_5 = "-";
                        Time_5 = "00:00";
                    }

                });

            }
            // Application closed - stop the timer
            else if (Application.Current == null)
            {
                _timer.Dispose();
            }
            else
            {
                 
            }
        }
    }
}
