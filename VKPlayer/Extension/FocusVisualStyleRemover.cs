using System.Windows;

namespace VKPlayer.Extension
{
    public class FocusVisualStyleRemover
    {
        static FocusVisualStyleRemover()
        {
            EventManager.RegisterClassHandler(typeof(FrameworkElement), UIElement.GotFocusEvent, new RoutedEventHandler(RemoveFocusVisualStyle), true);
        }

        public static void Init()
        {
            // intentially empty
        }

        private static void RemoveFocusVisualStyle(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).FocusVisualStyle = null;
        }
    }
}
