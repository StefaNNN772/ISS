using NetworkService;
using NetworkService.Model;
using NetworkService.ViewModel;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using NetworkService.Helpers;
using System.Windows.Media.Animation;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Windows.Shapes;

namespace NetworkService.Views
{
    public class EntitiesViewModel : BindableBase
    {
        private SolidColorBrush _idBorderBrush;
        public SolidColorBrush IDBorderBrush { get => _idBorderBrush; set => SetProperty(ref _idBorderBrush, value); }
        public List<string> Types { get; set; }

        private object _selectedType;
        public object SelectedType
        {

            get => _selectedType;
            set
            {
                SetProperty(ref _selectedType, value);
                AddEntityCommand.RaiseCanExecuteChanged();
            }
        }

        private Visibility _keyboardVisibility;
        public Visibility KeyboardVisibility { get => _keyboardVisibility; set => SetProperty(ref _keyboardVisibility, value); }

        private bool _isKeyboardEnabled;
        public bool IsKeyboardEnabled { get => _isKeyboardEnabled; set => SetProperty(ref _isKeyboardEnabled, value); }

        private TextBox _selectedTextBox;
        public TextBox SelectedTextBox
        {
            get => _selectedTextBox;
            set
            {
                SetProperty(ref _selectedTextBox, value);
            }
        }

        private ReactorTemperature _selectedEntity;
        public ReactorTemperature SelectedEntity
        {
            get => _selectedEntity;
            set
            {
                SetProperty(ref _selectedEntity, value);
                RemoveEntityCommand.RaiseCanExecuteChanged();
            }
        }
        public ObservableCollection<ReactorTemperature> ReactorTemperatures { get; set; }
        public ObservableCollection<ReactorTemperature> FilteredReactors { get; set; }

        private string _idText;
        public string IDText
        {
            get { return _idText; }
            set
            {
                SetProperty(ref _idText, value);
                AddEntityCommand.RaiseCanExecuteChanged();
            }
        }
        private string _nameText;
        public string NameText
        {
            get { return _nameText; }
            set
            {
                SetProperty(ref _nameText, value);
                AddEntityCommand.RaiseCanExecuteChanged();
            }
        }
        private string _filterText;
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                SetProperty(ref _filterText, value);
                FilterCommand.RaiseCanExecuteChanged();
            }
        }
        private string _filterType;
        public string FilterType
        {
            get => _filterType;
            set
            {
                SetProperty(ref _filterType, value);
                FilterCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isTypeChecked;
        public bool IsTypeChecked
        {
            get => _isTypeChecked;
            set
            {
                SetProperty(ref _isTypeChecked, value);
                if (_isTypeChecked)
                {
                    IsNameChecked = false;
                }
                FilterCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isNameChecked;
        public bool IsNameChecked
        {
            get => _isNameChecked;
            set
            {
                SetProperty(ref _isNameChecked, value);
                if (_isNameChecked)
                {
                    IsTypeChecked = false;
                }
                FilterCommand.RaiseCanExecuteChanged();
            }
        }

        public MyICommand<string> InputKeyCommand { get; set; }
        public MyICommand<object> TextBoxGotFocusCommand { get; set; }
        public MyICommand<object> TextBoxLostFocusCommand { get; set; }
        public MyICommand HideKeyboardCommand { get; set; }
        public MyICommand BackspaceCommand { get; set; }
        public MyICommand<string> InputNumberCommand { get; set; }
        public MyICommand<TextBox> TextChangedCommand { get; set; }
        public MyICommand AddEntityCommand { get; set; }
        public MyICommand RemoveEntityCommand { get; set; }
        public MyICommand FilterCommand { get; set; }
        public MyICommand ClearFiltersCommand { get; set; }

        public EntitiesViewModel()
        {

            ReactorTemperatures = MainWindowViewModel.ReactorTemperatures;
            FilteredReactors = new ObservableCollection<ReactorTemperature>();

            foreach (ReactorTemperature r in ReactorTemperatures)
            {
                FilteredReactors.Add(r);
            }

            InputKeyCommand = new MyICommand<string>(InputKey);
            InputNumberCommand = new MyICommand<string>(InputNumber);

            TextBoxGotFocusCommand = new MyICommand<object>(TextBoxGotFocus);
            TextBoxLostFocusCommand = new MyICommand<object>(TextBoxLostFocus);

            BackspaceCommand = new MyICommand(Backspace);
            TextChangedCommand = new MyICommand<TextBox>(OnTextChanged);

            HideKeyboardCommand = new MyICommand(HideKeyboard);


            AddEntityCommand = new MyICommand(OnAddEntity, CanAddEntity);
            RemoveEntityCommand = new MyICommand(OnRemoveEntity, CanRemoveEntity);

            FilterCommand = new MyICommand(Filter, CanFilter);
            ClearFiltersCommand = new MyICommand(ClearFilters);

            Types = new List<string>
            {
                "RTD",
                "TermoSprega"
            };

            IDBorderBrush = new SolidColorBrush(Colors.Transparent);

            IDText = "";
            NameText = "";
            SelectedType = Types[0];

            KeyboardVisibility = Visibility.Hidden;
            IsKeyboardEnabled = false;

        }

        private bool CanFilter()
        {
            if (IsTypeChecked || IsNameChecked)
            {
                return true;
            }

            return false;
        }
        private void ClearFilters()
        {
            IsTypeChecked = false;
            IsNameChecked = false;
            FilterText = string.Empty;
            FilterType = null;

            FilteredReactors.Clear();
            foreach (ReactorTemperature r in ReactorTemperatures)
            {
                FilteredReactors.Add(r);
            }
        }
        private void Filter()
        {
            HideKeyboard();
            FilteredReactors.Clear();

            foreach (ReactorTemperature reactorTemperature in ReactorTemperatures)
            {
                bool found = true;
                if (!string.IsNullOrWhiteSpace(FilterText))
                {
                    if (IsNameChecked && reactorTemperature.Name.Contains(FilterText))
                    {
                        found = true;
                    }
                    else if (IsTypeChecked && reactorTemperature.EntityType.Name.Contains(FilterText))
                    {
                        found = true;
                    }
                    else
                    {
                        found = false;
                    }
                }

                if (found)
                {
                    FilteredReactors.Add(reactorTemperature);
                }
            }

            ToastNotify.RaiseToast("Successful", "Filtering done!", Notification.Wpf.NotificationType.Notification);
        }

        private void OnTextChanged(TextBox textBox)
        {
            if (textBox.Name.Equals("IDTextBox"))
            {
                if (Regex.IsMatch(textBox.Text, @"^\d+$"))
                {
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        // Remove the last character
                        textBox.Text = textBox.Text.Remove(textBox.Text.Length - 1);
                        textBox.CaretIndex = textBox.Text.Length;
                    }


                    if (!(textBox.Background is SolidColorBrush))
                    {
                        textBox.Background = new SolidColorBrush(Colors.Transparent);
                    }

                    // Create a color animation
                    var colorAnimation = new ColorAnimation
                    {
                        From = Colors.Red,
                        To = (Color)System.Windows.Application.Current.Resources["PrimaryColorDark"],
                        Duration = TimeSpan.FromSeconds(0.3),
                        AutoReverse = false
                    };

                    var storyboard = new Storyboard();
                    storyboard.Children.Add(colorAnimation);

                    Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(TextBox.Background).(SolidColorBrush.Color)"));

                    Storyboard.SetTarget(colorAnimation, textBox);

                    storyboard.Begin();
                }
            }
        }

        private bool CanRemoveEntity()
        {
            if (SelectedEntity != null)
            {
                return true;
            }
            return false;
        }
        private void OnRemoveEntity()
        {
            if (MessageBox.Show("Are you sure you want to remove the selected entity?", "Confirmation Dialog",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                SaveState();
                ReactorTemperatures.Remove(SelectedEntity);

                if (DisplayViewModel.AddedToGrid != null)
                {
                    var keyToRemove = DisplayViewModel.AddedToGrid.FirstOrDefault(
                    x => EqualityComparer<ReactorTemperature>.Default.Equals(x.Value, SelectedEntity)).Key;

                    if (!EqualityComparer<int>.Default.Equals(keyToRemove, default))
                    {
                        DisplayViewModel.AddedToGrid.Remove(keyToRemove);
                        List<int> connections = DisplayViewModel.FindAllConnections(keyToRemove);
                        foreach (int connectedTo in connections)
                        {
                            int source = Math.Min(keyToRemove, connectedTo);
                            int destination = Math.Max(keyToRemove, connectedTo);
                            DisplayViewModel.DeleteLine(source, destination);
                        }
                    }
                }

                ToastNotify.RaiseToast("Successful", "Entity deleted!", Notification.Wpf.NotificationType.Information);

                SelectedEntity = null;
                ClearFilters();
            }


        }
        private bool CanAddEntity()
        {
            bool ret = true;

            if (IDText.Trim().Length > 0)
            {
                int intID;
                try
                {
                    intID = int.Parse(IDText);

                    foreach (ReactorTemperature f in ReactorTemperatures)
                    {
                        if (f.ID == intID)
                        {
                            IDBorderBrush = new SolidColorBrush(Colors.Red);
                            ret = false;
                            break;
                        }
                    }
                    if (ret)
                    {
                        IDBorderBrush = new SolidColorBrush(Colors.Transparent);
                    }
                }
                catch
                {
                    ret = false;
                }

            }
            else
            {
                ret = false;
            }

            if (NameText.Trim().Length == 0)
            {
                ret = false;
            }

            return ret;
        }
        private void OnAddEntity()
        {
            SaveState();

            ReactorTemperature newReactorTemperature = new ReactorTemperature
            {
                ID = int.Parse(IDText),
                Name = NameText.Trim()
            };
            string type = (SelectedType as string);
            newReactorTemperature.EntityType = new EntityType(type, $"../../Resources/Images/{type.ToLower()}.png");

            ReactorTemperatures.Add(newReactorTemperature);

            IDText = string.Empty;
            NameText = string.Empty;
            SelectedType = Types[0];

            HideKeyboard();

            ClearFilters();

            AddEntityCommand.RaiseCanExecuteChanged();

            ToastNotify.RaiseToast("Successful", "Created new entity!", Notification.Wpf.NotificationType.Success);
        }

        private void SaveState()
        {
            MainWindowViewModel.UndoStack.Push(new SaveState<CommandType, object> 
                (CommandType.EntityChange, new ObservableCollection<ReactorTemperature>(ReactorTemperatures)));
        }

        private void HideKeyboard()
        {
            KeyboardVisibility = Visibility.Hidden;
            IsKeyboardEnabled = false;
        }
        private void Backspace()
        {
            // Removing selected symbols
            if (!string.IsNullOrEmpty(SelectedTextBox.SelectedText))
            {
                int selectionStart = SelectedTextBox.SelectionStart;
                SelectedTextBox.Text = SelectedTextBox.Text.Remove(selectionStart, SelectedTextBox.SelectionLength);
                SelectedTextBox.CaretIndex = selectionStart;
                return;
            }
            // Removing one symbol
            if (SelectedTextBox.Text.Length > 0)
            {
                SelectedTextBox.Text = SelectedTextBox.Text.Remove(SelectedTextBox.Text.Length - 1, 1);
            }
        }

        private void TextBoxGotFocus(object obj)
        {
            if (obj is TextBox textBox)
            {
                SelectedTextBox = textBox;
                SelectedTextBox.Focus();
                KeyboardVisibility = Visibility.Visible;
                IsKeyboardEnabled = true;
            }
        }
        private void TextBoxLostFocus(object obj)
        {
            if (obj is TextBox)
            {
                KeyboardVisibility = Visibility.Hidden;
                IsKeyboardEnabled = false;
            }
        }
        private void InputKey(string keyPressed)
        {
            if (SelectedTextBox != null && !SelectedTextBox.Name.Equals("IDTextBox"))
            {
                SelectedTextBox.Text += keyPressed;
            }
            else if (SelectedTextBox.Name.Equals("IDTextBox"))
            {
                // Create a color animation
                var colorAnimation = new ColorAnimation
                {
                    From = Colors.Red,
                    To = (Color)System.Windows.Application.Current.Resources["PrimaryColorDark"],
                    Duration = TimeSpan.FromSeconds(0.3),
                    AutoReverse = false,
                    RepeatBehavior = new RepeatBehavior(1)
                };

                // Adding the animation to storyboard
                var storyboard = new Storyboard();
                storyboard.Children.Add(colorAnimation);

                Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(TextBox.Background).(SolidColorBrush.Color)"));

                Storyboard.SetTarget(colorAnimation, SelectedTextBox);

                storyboard.Begin();
            }

        }
        private void InputNumber(string keyPressed)
        {
            if (SelectedTextBox != null)
            {
                SelectedTextBox.Text += keyPressed;
            }
        }

    }

}
