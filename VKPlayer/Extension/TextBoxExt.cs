using System.Windows;
using System.Windows.Controls;

namespace VKPlayer.Extension
{
    public sealed class TextBoxExt : TextBox
    {
        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register(nameof(DefaultText), typeof(string), typeof(TextBoxExt));

        public string DefaultText
        {
            get => (string) GetValue(DefaultTextProperty);
            set => SetValue(DefaultTextProperty, value);
        }

    }
}
