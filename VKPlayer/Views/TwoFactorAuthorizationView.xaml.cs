using System.ComponentModel;
using System.Windows;
using VKPlayer.ViewModels;

namespace VKPlayer.Views
{
    public partial class TwoFactorAuthorizationView
    {
        private TwoFactorAuthorizationViewModel _viewModel;

        private bool _isClosing;

        public TwoFactorAuthorizationView()
        {
            InitializeComponent();
        }

        private void TwoFactorAuthorizationView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = (TwoFactorAuthorizationViewModel)e.NewValue;
            _viewModel.PropertyChanged += VmOnPropertyChanged;
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TwoFactorAuthorizationViewModel.IsOpen))
            {
                if (!_viewModel.IsOpen && !_isClosing)
                {
                    Close();
                }
            }
        }

        private void TwoFactorAuthorizationView_OnClosing(object sender, CancelEventArgs e)
        {
            _isClosing = true;
            _viewModel.IsOpen = false;
        }
    }
}
