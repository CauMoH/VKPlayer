using System.ComponentModel;
using System.Windows;
using VKPlayer.Extension;
using VKPlayer.ViewModels;

namespace VKPlayer.Views
{
    public partial class ConnectionSetupView
    {
        private ConnectionSetupViewModel _viewModel;

        private bool _isClosing;

        public ConnectionSetupView()
        {
            InitializeComponent();
        }

        private void SetupView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = (ConnectionSetupViewModel)e.NewValue;
            _viewModel.PropertyChanged += VmOnPropertyChanged;

            _viewModel.PbExt = new PasswordBoxExtension(Password);

            if (!string.IsNullOrWhiteSpace(_viewModel.UserName))
            {
                _viewModel.PbExt.SetInitialPassword(_viewModel.Password);
            }
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConnectionSetupViewModel.IsOpen))
            {
                if (!_viewModel.IsOpen && !_isClosing)
                {
                    Close();
                }
            }
        }

        private void SetupView_OnClosing(object sender, CancelEventArgs e)
        {
            _isClosing = true;
            _viewModel.IsOpen = false;
        }
    }
}
