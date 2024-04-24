using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CruiseProcessing.Controls
{
    public class ButtonItem : ContentControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text),
                typeof(string),
                typeof(ButtonItem),
                new PropertyMetadata());

        public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(ButtonItem),
                new FrameworkPropertyMetadata());

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
    }
}
