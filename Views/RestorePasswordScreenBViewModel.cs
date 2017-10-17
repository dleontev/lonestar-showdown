using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Caliburn.Micro;
using LonestarShowdown.Properties;

namespace LonestarShowdown.Views
{
    internal class RestorePasswordScreenBViewModel : Screen
    {
        private readonly string _email;
        private readonly string _phone;
        private readonly byte[] _userAnswer;
        private readonly byte[] _userSalt;
        private string _longPhoneMessage;
        private string _securityAnswer = string.Empty;
        private string _securityQuestion;
        private string _shortPhoneMessage;

        public RestorePasswordScreenBViewModel(string email, string phone, byte[] userSalt, byte[] userAnswer,
            string securityQuestion)
        {
            _email = email;
            _phone = Regex.Replace(phone, @"\D", string.Empty);
            _userSalt = userSalt;
            _userAnswer = userAnswer;
            SecurityQuestion = securityQuestion;
            ShortPhoneMessage = string.Format(Resources.ShortPhoneMessage,
                _phone.Substring(_phone.Length - 2, 2));
            LongPhoneMessage = string.Format(Resources.LongPhoneMessage, _phone.Substring(_phone.Length - 2, 2));
        }

        public string SecurityQuestion
        {
            get { return _securityQuestion; }
            set
            {
                if (_securityQuestion == value)
                    return;

                _securityQuestion = value;
                NotifyOfPropertyChange(() => SecurityQuestion);
            }
        }

        public string SecurityAnswer
        {
            get { return _securityAnswer; }
            set
            {
                if (_securityAnswer == value)
                    return;

                _securityAnswer = value;
                NotifyOfPropertyChange(() => SecurityAnswer);
            }
        }

        public string ShortPhoneMessage
        {
            get { return _shortPhoneMessage; }
            set
            {
                if (_shortPhoneMessage == value)
                    return;

                _shortPhoneMessage = value;
                NotifyOfPropertyChange(() => ShortPhoneMessage);
            }
        }

        public string LongPhoneMessage
        {
            get { return _longPhoneMessage; }
            set
            {
                if (_longPhoneMessage == value)
                    return;

                _longPhoneMessage = value;
                NotifyOfPropertyChange(() => LongPhoneMessage);
            }
        }

        /// <summary>
        ///     Returns back to the SignIn screen.
        /// </summary>
        public void CancelAction()
        {
            var parentConductor = (Conductor<object>) (Parent);
            parentConductor.ActivateItem(new SignInViewModel());
        }

        /// <summary>
        ///     Verifies the code/answer.
        /// </summary>
        public void SendCodeAction(string userDigits, bool isPhoneOptionSelected)
        {
            if (isPhoneOptionSelected)
            {
                var r = new Random();
                var code = 0;
                if (userDigits.Equals(_phone.Substring(_phone.Length - 4, 4)))
                {
                    code = r.Next(1000000, 9999999);
                    TextString(string.Format(Resources.ResetTextMessage, code));
                }
                var parentConductor = (Conductor<object>) (Parent);
                parentConductor.ActivateItem(new RestorePasswordScreenCViewModel(_email, code.ToString(), userDigits));
            }
            else
            {
                HashAlgorithm hash = new SHA256Managed();
                var hashBytes =
                    hash.ComputeHash(_userSalt.Concat(Encoding.UTF8.GetBytes(SecurityAnswer)).ToArray());

                if (_userAnswer.SequenceEqual(hashBytes))
                {
                    var parentConductor = (Conductor<object>) (Parent);
                    parentConductor.ActivateItem(new RestorePasswordScreenDViewModel(_email));
                }
                else
                {
                    MessageBox.Show(Resources.WrongAnswerMessage, Resources.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        /// <summary>
        ///     Sends the code to the user's phone.
        /// </summary>
        private void TextString(string message)
        {
            var request = WebRequest.Create("http://textbelt.com/text");
            request.Method = "POST";
            var postData = string.Format("number={0}&message={1}", _phone, message);
            var byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
        }
    }
}