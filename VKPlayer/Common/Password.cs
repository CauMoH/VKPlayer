using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace VKPlayer.Common
{
    public sealed class Password
    {
        private SecureString _securePassword = new SecureString();

        public Password()
        {

        }

        public Password(SecureString securePassword)
        {
            _securePassword = securePassword;
        }

        public string Get()
        {
            return ConvertToUnsecureString(_securePassword);
        }

        public SecureString GetSecure()
        {
            return _securePassword;
        }

        public string GetProtected()
        {
            return _securePassword.Length == 0 ? string.Empty : ConvertToUnsecureString(_securePassword).Protect();
        }

        public void Set(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            _securePassword = ConvertToSecureString(password);
        }

        public void SetSecure(SecureString securePassword)
        {
            if (securePassword == null)
            {
                throw new ArgumentNullException(nameof(securePassword));
            }

            _securePassword = securePassword;
        }

        public void SetProtected(string protectedPassword)
        {
            if (protectedPassword == null)
            {
                throw new ArgumentNullException(nameof(protectedPassword));
            }

            if (protectedPassword.Length == 0)
            {
                return;
            }

            _securePassword = ConvertToSecureString(protectedPassword.Unprotect());
        }

        public void Reset()
        {
            _securePassword = new SecureString();
        }

        private static string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
            {
                throw new ArgumentNullException(nameof(securePassword));
            }

            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        private static SecureString ConvertToSecureString(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            unsafe
            {
                fixed (char* passwordChars = password)
                {
                    var securePassword = new SecureString(passwordChars, password.Length);
                    securePassword.MakeReadOnly();
                    return securePassword;
                }
            }
        }
    }
}
