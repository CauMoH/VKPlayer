using System.ComponentModel;
using System.Windows;
using VKPlayer.ViewModels;

namespace VKPlayer.Views
{
    public partial class CaptchaView
    {
        private CaptchaViewModel _viewModel;

        private bool _isClosing;

        public CaptchaView()
        {
            InitializeComponent();
        }

        private void CaptchaView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = (CaptchaViewModel)e.NewValue;
            _viewModel.PropertyChanged += VmOnPropertyChanged;
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CaptchaViewModel.IsOpen))
            {
                if (!_viewModel.IsOpen && !_isClosing)
                {
                    Close();
                }
            }
        }

        private void CaptchaView_OnClosing(object sender, CancelEventArgs e)
        {
            _isClosing = true;
            _viewModel.IsOpen = false;
        }
    }
}
