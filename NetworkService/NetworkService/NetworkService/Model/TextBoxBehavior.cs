using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace NetworkService.Model
{
    public static class TextBoxBehavior
    {
        public static readonly DependencyProperty GotFocusCommandProperty =
            DependencyProperty.RegisterAttached(
                "GotFocusCommand", typeof(ICommand), typeof(TextBoxBehavior),
                new PropertyMetadata(null, OnGotFocusCommandChanged));

        public static readonly DependencyProperty LostFocusCommandProperty =
           DependencyProperty.RegisterAttached(
               "LostFocusCommand", typeof(ICommand), typeof(TextBoxBehavior),
               new PropertyMetadata(null, OnLostFocusCommandChanged));

        public static readonly DependencyProperty TextChangedCommandProperty =
            DependencyProperty.RegisterAttached("TextChangedCommand", typeof(ICommand), typeof(TextBoxBehavior),
                new PropertyMetadata(null, OnTextChangedCommandChanged));


        public static ICommand GetGotFocusCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(GotFocusCommandProperty);
        }

        public static void SetGotFocusCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(GotFocusCommandProperty, value);
        }

        public static ICommand GetLostFocusCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(LostFocusCommandProperty);
        }

        public static void SetLostFocusCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(LostFocusCommandProperty, value);
        }

        public static ICommand GetTextChangedCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(TextChangedCommandProperty);
        }

        public static void SetTextChangedCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(TextChangedCommandProperty, value);
        }

        private static void OnGotFocusCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.GotFocus -= TextBox_GotFocus;
                textBox.GotFocus += TextBox_GotFocus;
            }
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var command = GetGotFocusCommand(textBox);
            command?.Execute(textBox);
        }


        private static void OnLostFocusCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.LostFocus -= TextBox_LostFocus;
                textBox.LostFocus += TextBox_LostFocus;
            }
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var command = GetLostFocusCommand(textBox);
            command?.Execute(textBox);
        }


        private static void OnTextChangedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.TextChanged -= TextBox_TextChanged;
                textBox.TextChanged += TextBox_TextChanged;
            }
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            var command = GetTextChangedCommand(textBox);
            command?.Execute(textBox.Text);
        }
    }

}