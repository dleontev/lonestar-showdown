using System.Linq;
using System.Windows;
using Caliburn.Micro;
using LonestarShowdown.Database;
using LonestarShowdown.Properties;

namespace LonestarShowdown.Views
{
    internal class RestorePasswordScreenAViewModel : Screen
    {
        /// <summary>
        ///     Verifies that the email exists.
        /// </summary>
        public void ProceedToTheNextScreenAction(string email)
        {
            using (var db = new LonestarDbContext())
            {
                var personnel = db.Personnels.FirstOrDefault(p => p.Email == email);

                if (personnel != null)
                {
                    var parentConductor = (Conductor<object>) (Parent);

                    var securityQuestion = db.SecurityQuestions.Find(personnel.SecurityQuestionID).SecurityQ;
                    parentConductor.ActivateItem(new RestorePasswordScreenBViewModel(email, personnel.Phone,
                        personnel.SaltData, personnel.SecurityAnswer, securityQuestion));
                }
                else
                {
                    MessageBox.Show(Resources.EmailNotFoundMessage, Resources.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
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
    }
}