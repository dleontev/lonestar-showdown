using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using LonestarShowdown.Database;
using LonestarShowdown.Properties;

namespace LonestarShowdown.Views
{
    internal class RestorePasswordScreenDViewModel : Screen
    {
        private readonly string _email;
        private string _passwordMain;
        private string _passwordRetype;

        /// <summary>
        /// </summary>
        public RestorePasswordScreenDViewModel(string email)
        {
            _email = email;
        }

        /// <summary>
        ///     Extracts the password string from the password boxes.
        /// </summary>
        public void ParsePassword(PasswordBox mainPasswordBox, bool isRetype)
        {
            // Determines if the password that is being extracted is a retype.
            if (isRetype)
            {
                _passwordRetype = mainPasswordBox.Password;
            }
            else
            {
                _passwordMain = mainPasswordBox.Password;
            }
        }

        /// <summary>
        ///     Updates the password in the database.
        /// </summary>
        public void SavePasswordAction()
        {
            if (!string.IsNullOrEmpty(_passwordMain) && _passwordMain.Length >= 8)
            {
                if (_passwordMain == _passwordRetype)
                {
                    using (var db = new LonestarDbContext())
                    {
                        var newUser = db.Personnels.FirstOrDefault(p => p.Email == _email);

                        // Encodes the password.
                        HashAlgorithm hash = new SHA256Managed();

                        if (newUser != null)
                        {
                            newUser.Password =
                                hash.ComputeHash(
                                    newUser.SaltData.Concat(Encoding.UTF8.GetBytes(_passwordMain)).ToArray());

                            // Sets the 'Last changed' date for password and security question/answer to current time.
                            newUser.PasswordChangeDate = DateTime.Now;
                        }

                        db.SaveChanges();
                    }

                    MessageBox.Show(Resources.PasswordChangeSuccessMessage, Resources.SuccessTitle, MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    CancelAction();
                }
                else
                {
                    DisplayError(Resources.PasswordsDontMatchMessage);
                }
            }
            else
            {
                DisplayError(Resources.EmptyPasswordMessage);
            }
        }

        /// <summary>
        ///     Display a generic MessageBox with an error.
        /// </summary>
        private static void DisplayError(string message)
        {
            MessageBox.Show(message, Resources.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        /// <summary>
        ///     Returns back to the main screen.
        /// </summary>
        public void CancelAction()
        {
            var parentConductor = (Conductor<object>) (Parent);
            parentConductor.ActivateItem(new SignInViewModel());
        }
    }
}