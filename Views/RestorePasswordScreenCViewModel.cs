using System.Windows;
using Caliburn.Micro;
using LonestarShowdown.Properties;

namespace LonestarShowdown.Views
{
    internal class RestorePasswordScreenCViewModel : Screen
    {
        private readonly string _code;
        private readonly string _email;
        private string _securityCodeMessage;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public RestorePasswordScreenCViewModel(string email, string code, string lastFour)
        {
            _code = code;
            _email = email;
            SecurityCodeMessage = string.Format(Resources.SecurityCodeMessage, lastFour);
        }

        public string SecurityCodeMessage
        {
            get { return _securityCodeMessage; }
            set
            {
                if (_securityCodeMessage == value)
                    return;

                _securityCodeMessage = value;
                NotifyOfPropertyChange(() => SecurityCodeMessage);
            }
        }

        /// <summary>
        ///     Returns back to the main screen.
        /// </summary>
        public void CancelAction()
        {
            var parentConductor = (Conductor<object>) (Parent);
            parentConductor.ActivateItem(new SignInViewModel());
        }

        /// <summary>
        ///     Verifies that the code enter is the right one.
        /// </summary>
        public void VerifyCodeAction(string userCode)
        {
            if (!string.IsNullOrEmpty(_code) && _code.Equals(userCode))
            {
                var parentConductor = (Conductor<object>) (Parent);
                parentConductor.ActivateItem(new RestorePasswordScreenDViewModel(_email));
            }
            else
            {
                MessageBox.Show(Resources.WrongCodeMessage, Resources.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}