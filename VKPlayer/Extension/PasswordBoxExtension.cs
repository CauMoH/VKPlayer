using System.Security;
using System.Windows.Controls;

namespace VKPlayer.Extension
{
    /// <summary>
    /// Нужен для того, чтобы не хранить пароль в PasswordBox.
    /// Если пользователь ввёл новый пароль, он будет получен через Password,
    /// если нет, то будет возвращён изначальный пароль.
    /// В самом контроле всегда отображается одинаковое количество символов независимо от настойщей длины пароля.
    /// </summary>
    public sealed class PasswordBoxExtension
    {
        private readonly PasswordBox _passwordBox;
        private SecureString _initialPassword;

        private const string PasswordStub = "****************";

        private bool _passwordChanged;

        public PasswordBoxExtension(PasswordBox passwordBox)
        {
            _passwordBox = passwordBox;
            SetInitialPassword(null);
        }

        public void SetInitialPassword(SecureString password)
        {
            _initialPassword = password ?? new SecureString();
            _passwordChanged = false;

            _passwordBox.PasswordChanged -= _passwordBox_PasswordChanged;
            _passwordBox.Password = password != null ? PasswordStub : null;
            _passwordBox.PasswordChanged += _passwordBox_PasswordChanged;
        }

        private void _passwordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            _passwordChanged = true;
        }

        public SecureString Password => _passwordChanged ? _passwordBox.SecurePassword : _initialPassword;
    }
}
