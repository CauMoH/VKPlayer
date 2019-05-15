using System.Windows.Input;
using VKPlayer.ViewModels;

namespace VKPlayer.Views
{
    public partial class PlayerView
    {
        public PlayerView()
        {
            InitializeComponent();
        }

        private void ProgressBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var absoluteMouse = e.GetPosition(ProgressBar).X;
            double calcFactor = ProgressBar.ActualWidth / ProgressBar.Maximum;
            double relativeMouse = absoluteMouse / calcFactor;
            ((MainViewModel) DataContext).SetPosition((float)relativeMouse);
        }
    }
}
