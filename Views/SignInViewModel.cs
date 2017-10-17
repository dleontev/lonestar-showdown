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
    internal class SignInViewModel : Screen
    {
        private string _enteredPassword = string.Empty;

        public string EnteredPassword
        {
            get { return _enteredPassword; }

            set
            {
                _enteredPassword = value;
                NotifyOfPropertyChange(() => EnteredPassword);
            }
        }

        /// <summary>
        ///     Initiates the sign in.
        /// </summary>
        public void SignInAction(string email)
        {
            using (var db = new LonestarDbContext())
            {
                // Verifies that the email is in the database.
                var myPerson = db.Personnels.FirstOrDefault(p => p.Email == email.Trim());
                if (myPerson != null)
                {
                    // Calculates the hash of the entered password using the found user's salt.
                    HashAlgorithm hash = new SHA256Managed();
                    var hashBytes =
                        hash.ComputeHash(myPerson.SaltData.Concat(Encoding.UTF8.GetBytes(EnteredPassword)).ToArray());

                    // Verifies that the hashes match.
                    if (myPerson.Password.SequenceEqual(hashBytes))
                    {
                        // Erases the password from the memory.
                        EnteredPassword = string.Empty;
                        // Brings up the main user window.
                        var parentconductor = (Conductor<object>) (Parent);
                        parentconductor.ActivateItem(new UserViewModel(myPerson));
                    }
                    else
                    {
                        // Displays an error.
                        DisplayInvalidCredentialsMessage();
                    }
                }
                else
                {
                    // Displays an error.
                    DisplayInvalidCredentialsMessage();
                }
            }
        }

        /// <summary>
        /// </summary>
        private static void DisplayInvalidCredentialsMessage()
        {
            MessageBox.Show(Resources.InvalidCredentialsMessage, Resources.ErrorTitle, MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// </summary>
        /// <param name="mainPasswordBox"></param>
        public void ParsePassword(PasswordBox mainPasswordBox)
        {
            EnteredPassword = mainPasswordBox.Password;
        }

        /// <summary>
        /// </summary>
        public void ShowRestorePasswordScreenAction()
        {
            var parentConductor = (Conductor<object>) (Parent);
            parentConductor.ActivateItem(new RestorePasswordScreenAViewModel());
        }

        /// <summary>
        /// </summary>
        public void CreateAccountAction()
        {
            var parentConductor = (Conductor<object>) (Parent);
            parentConductor.ActivateItem(new CreateAccountViewModel());
        }

        /// <summary>
        /// </summary>
        public void CloseAction()
        {
            var parentConductor = (Conductor<object>) (Parent);
            parentConductor.TryClose();
        }
    }
}