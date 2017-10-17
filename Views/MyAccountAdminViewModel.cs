using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using LonestarShowdown.Custom;
using LonestarShowdown.Database;
using LonestarShowdown.Properties;
using Microsoft.Win32;

namespace LonestarShowdown.Views
{
    internal class MyAccountAdminViewModel : Screen
    {
        private LonestarDbContext _db;
        private bool _isPersonalInfoEditable;
        private bool _isSecurityInfoEditable;
        private Dictionary<string, int> _mySecurityQuestionList;
        private List<TeamT> _myTeamList;
        private Personnel _myUser;
        private PasswordBox _newPasswordBoxMain;
        private PasswordBox _newPasswordBoxRetype;
        private string _newPasswordMain;
        private string _newPasswordRetype;
        private PasswordBox _oldPasswordBox;
        private string _oldPasswordMain;
        private string _securityAnswer;
        private string _selectedSecurityQuestion;
        private Personnel _tUser;

        #region [Initialization Methods]

        // Default constructor.
        public MyAccountAdminViewModel(Personnel p)
        {
            MyUser = p;

            LoadData();
        }

        #endregion

        public DateTime MaxDate
        {
            get { return DateTime.Now.AddYears(-18); }
        }

        public string FirstName
        {
            get { return MyUser.FirstName; }

            set { MyUser.FirstName = value; }
        }

        public string LastName
        {
            get { return MyUser.LastName; }

            set { MyUser.LastName = value; }
        }

        public string Phone
        {
            get { return MyUser.Phone; }

            set { MyUser.Phone = value; }
        }

        public string Email
        {
            get { return MyUser.Email; }

            set { MyUser.Email = value; }
        }

        public string City
        {
            get { return MyUser.City; }

            set { MyUser.City = value; }
        }

        public string Address
        {
            get { return MyUser.Address; }

            set { MyUser.Address = value; }
        }

        public string SelectedSecurityQuestion
        {
            get { return _selectedSecurityQuestion; }
            set
            {
                if (_selectedSecurityQuestion == value)
                    return;

                _selectedSecurityQuestion = value;
                NotifyOfPropertyChange(() => SelectedSecurityQuestion);
            }
        }

        public bool IsAllowedToSave
        {
            get
            {
                return (!string.IsNullOrEmpty(_oldPasswordMain) && !string.IsNullOrEmpty(_newPasswordMain) &&
                        !string.IsNullOrEmpty(_newPasswordRetype)) || !(string.IsNullOrEmpty(SecurityAnswer));
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
                NotifyOfPropertyChange(() => IsAllowedToSave);
            }
        }

        public Dictionary<string, int> MySecurityQuestionList
        {
            get { return _mySecurityQuestionList; }
            set
            {
                if (_mySecurityQuestionList == value)
                    return;

                _mySecurityQuestionList = value;
                NotifyOfPropertyChange(() => MySecurityQuestionList);
            }
        }

        public string PasswordLastChangeMessage
        {
            get
            {
                return string.Format(Resources.PWLastChangedMessage,
                    MyUser.PasswordChangeDate.ToShortDateString(),
                    MyUser.PasswordChangeDate.ToShortTimeString());
            }
        }

        public string SecurityQuestionLastChangedMessage
        {
            get
            {
                return string.Format(Resources.SQLastChangedMessage,
                    MyUser.SecurityQuestionChangeDate.ToShortDateString(),
                    MyUser.SecurityQuestionChangeDate.ToShortTimeString());
            }
        }

        public Personnel MyUser
        {
            get { return _myUser; }
            set
            {
                if (_myUser == value)
                    return;

                _myUser = value;
                NotifyOfPropertyChange(null);
            }
        }

        public string DisplayedName
        {
            get { return string.Format("{0} {1}", MyUser.FirstName, MyUser.LastName); }
        }

        public bool IsPersonalInfoEditable
        {
            get { return _isPersonalInfoEditable; }
            set
            {
                if (_isPersonalInfoEditable == value)
                    return;

                _isPersonalInfoEditable = value;
                NotifyOfPropertyChange(() => IsPersonalInfoEditable);
            }
        }

        public bool IsSecurityInfoEditable
        {
            get { return _isSecurityInfoEditable; }
            set
            {
                if (_isSecurityInfoEditable == value)
                    return;

                _isSecurityInfoEditable = value;
                NotifyOfPropertyChange(() => IsSecurityInfoEditable);
            }
        }

        public byte[] CustomUserPicture
        {
            get { return MyUser.Picture ?? Resources.BlankProfile; }
        }

        public List<TeamT> MyTeamList

        {
            get { return _myTeamList; }
            set
            {
                if (_myTeamList == value)
                    return;

                _myTeamList = value;
                NotifyOfPropertyChange(() => MyTeamList);
            }
        }

        public bool IsNotApprovedTeamListEmpty
        {
            get { return MyTeamList.Count == 0; }
        }

        /// <summary>
        ///     Loads the required data into the view.
        /// </summary>
        private void LoadData()
        {
            using (_db = new LonestarDbContext())
            {
                // Loads the security questions.
                MySecurityQuestionList =
                    _db.SecurityQuestions.Select(t => new {t.SecurityQ, t.SecurityQuestionID})
                        .ToDictionary(t => t.SecurityQ, t => t.SecurityQuestionID);

                LoadTeamList();
            }
        }

        /// <summary>
        ///     Reloads the list of the unapproved teams.
        /// </summary>
        public void RefreshAction()
        {
            using (_db = new LonestarDbContext())
            {
                LoadTeamList();
            }
        }

        /// <summary>
        ///     Loads the list of the team that have not been approved.
        /// </summary>
        private void LoadTeamList()
        {
            // Gets the team information and the manager's name.
            MyTeamList = (from t in _db.Teams
                join p in _db.Personnels on t.TeamID equals p.TeamID
                where t.IsApproved == false && p.PermissionLevel == 2
                select new TeamT
                {
                    TeamId = t.TeamID,
                    TeamLogo = t.TeamLogo,
                    TeamName = t.TeamName,
                    City = t.City,
                    Manager = p.FirstName + " " + p.LastName
                }).ToList<TeamT>();

            // Updates the view.
            NotifyOfPropertyChange(() => IsNotApprovedTeamListEmpty);
        }

        #region [Team Methods]

        /// <summary>
        ///     Approves the team.
        /// </summary>
        /// <param name="team"></param>
        public void ApproveTeamAction(TeamT team)
        {
            if (team == null) return;
            var confirmation =
                MessageBox.Show(string.Format(Resources.ApproveTeamConfirmationMessage, team.Manager, team.TeamName),
                    Resources.ConfirmTitle,
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes) return;
            using (_db = new LonestarDbContext())
            {
                var t = _db.Teams.Find(team.TeamId);
                t.IsApproved = true;
                _db.SaveChanges();
                LoadTeamList();
            }
        }

        /// <summary>
        ///     Disapproves the team and removes it from the database.
        /// </summary>
        /// <param name="team"></param>
        public void RejectTeamAction(TeamT team)
        {
            if (team == null) return;
            var confirmation =
                MessageBox.Show(string.Format(Resources.RejectTeamConfirmationMessage, team.Manager, team.TeamName),
                    Resources.ConfirmTitle,
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes) return;
            using (_db = new LonestarDbContext())
            {
                var manager = _db.Personnels.FirstOrDefault(p => p.TeamID == team.TeamId && p.PermissionLevel == 2);
                if (manager != null) manager.TeamID = null;
                _db.Teams.Remove(_db.Teams.Find(team.TeamId));
                _db.SaveChanges();
                LoadTeamList();
            }
        }

        #endregion

        #region [Security Info Methods]

        /// <summary>
        ///     Enables personal info editing.
        /// </summary>
        public void EditSecurityInfoAction()
        {
            IsSecurityInfoEditable = true;
        }

        /// <summary>
        ///     Extracts the password string from the password boxes.
        /// </summary>
        public void ParsePassword(PasswordBox passBox, byte type)
        {
            switch (type)
            {
                case 1:
                    _oldPasswordBox = _oldPasswordBox ?? passBox;
                    _oldPasswordMain = passBox.Password;
                    break;
                case 2:
                    _newPasswordBoxMain = _newPasswordBoxMain ?? passBox;
                    _newPasswordMain = passBox.Password;
                    break;
                case 3:
                    _newPasswordBoxRetype = _newPasswordBoxRetype ?? passBox;
                    _newPasswordRetype = passBox.Password;
                    break;
            }
            NotifyOfPropertyChange(() => IsAllowedToSave);
        }

        /// <summary>
        ///     Saves changes to the database.
        /// </summary>
        public void SaveSecurityInfoEditAction()
        {
            HashAlgorithm hash = new SHA256Managed();

            var hasPasswordValidationFailed = false;

            if (!string.IsNullOrEmpty(_oldPasswordMain) && !string.IsNullOrEmpty(_newPasswordMain) &&
                !string.IsNullOrEmpty(_newPasswordRetype))
            {

                if (ValidatePassword())
                {
                    using (var db = new LonestarDbContext())
                    {
                        // Encodes the new password.
                        MyUser.Password =
                            hash.ComputeHash(
                                MyUser.SaltData.Concat(Encoding.UTF8.GetBytes(_newPasswordMain)).ToArray());
                        // Sets the new password change date.
                        MyUser.PasswordChangeDate = DateTime.Now;
                        db.Personnels.Attach(MyUser);
                        // Marks the password properties as modified.
                        db.Entry(MyUser).Property(p => p.Password).IsModified = true;
                        db.Entry(MyUser).Property(p => p.PasswordChangeDate).IsModified = true;
                        // Saves change to the db.
                        db.SaveChanges();
                    }
                    // Updates the visual interface.
                    NotifyOfPropertyChange(() => PasswordLastChangeMessage);
                }
                else
                {
                    hasPasswordValidationFailed = true;
                }
            }

            // Updates the security answer
            if (!string.IsNullOrEmpty(SecurityAnswer) && hasPasswordValidationFailed == false)
            {
                using (var db = new LonestarDbContext())
                {
                    // Updates the user's security question.
                    MyUser.SecurityQuestionID = MySecurityQuestionList[SelectedSecurityQuestion];
                    // Encodes the answer.
                    MyUser.SecurityAnswer =
                        hash.ComputeHash(MyUser.SaltData.Concat(Encoding.UTF8.GetBytes(SecurityAnswer)).ToArray());
                    // Updates the security question/answer date.
                    MyUser.SecurityQuestionChangeDate = DateTime.Now;
                    // Attach the user record to the db and marks the modified properties.
                    db.Personnels.Attach(MyUser);
                    db.Entry(MyUser).Property(p => p.SecurityQuestionID).IsModified = true;
                    db.Entry(MyUser).Property(p => p.SecurityAnswer).IsModified = true;
                    db.Entry(MyUser).Property(p => p.SecurityQuestionChangeDate).IsModified = true;
                    // Save changes to the db.
                    db.SaveChanges();
                }
                // Update the visual interface.
                NotifyOfPropertyChange(() => SecurityQuestionLastChangedMessage);
            }

            if (!hasPasswordValidationFailed)
            {
                IsSecurityInfoEditable = false;
                ClearSecurityTab();
            }
        }

        /// <summary>
        ///     Validates the passwords entered by the user.
        /// </summary>
        private bool ValidatePassword()
        {
            // Verifies that the passwords are the same.
            if (_newPasswordMain != _newPasswordRetype)
            {
                MessageBox.Show(Resources.PasswordsDontMatchMessage, Resources.ErrorTitle, MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return false;
            }

            // Verifies that the passwords are at least 8 characters long.
            if (_newPasswordMain.Length < 8 || _newPasswordRetype.Length < 8)
            {
                MessageBox.Show(Resources.EmptyPasswordMessage, Resources.ErrorTitle, MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return false;
            }

            HashAlgorithm hash = new SHA256Managed();

            // Verifies that the old password is correct.
            if (!MyUser.Password.SequenceEqual(
                hash.ComputeHash(MyUser.SaltData.Concat(Encoding.UTF8.GetBytes(_oldPasswordMain)).ToArray())))
            {
                MessageBox.Show(Resources.WrongPassword, Resources.ErrorTitle, MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Disables the security edit mode without saving any changes.
        /// </summary>
        public void CancelSecurityInfoEditAction()
        {
            IsSecurityInfoEditable = false;
            ClearSecurityTab();
        }

        /// <summary>
        ///     Resets the security tab.
        /// </summary>
        private void ClearSecurityTab()
        {
            _oldPasswordMain = string.Empty;
            _newPasswordMain = string.Empty;
            _newPasswordRetype = string.Empty;
            SafeClear(_oldPasswordBox);
            SafeClear(_newPasswordBoxMain);
            SafeClear(_newPasswordBoxRetype);
            SecurityAnswer = string.Empty;
        }

        /// <summary>
        /// </summary>
        /// <param name="passwordBox"></param>
        private static void SafeClear(PasswordBox passwordBox)
        {
            if (passwordBox != null)
                passwordBox.Clear();
        }

        #endregion

        #region [Personal Info Methods]

        /// <summary>
        ///     Enables personal info editing
        /// </summary>
        public void EditPersonalInfoAction()
        {
            IsPersonalInfoEditable = true;
            // Creates a copy of the current user's record.
            using (var db = new LonestarDbContext())
            {
                _tUser = db.Personnels.Find(MyUser.PID);
            }
        }

        /// <summary>
        ///     Saves changes to the database
        /// </summary>
        public void SavePersonalInfoEditAction()
        {
            // Trims the user data.
            MyUser.FirstName = MyUser.FirstName.Trim();
            MyUser.LastName = MyUser.LastName.Trim();
            MyUser.City = MyUser.City.Trim();
            MyUser.Address = MyUser.Address.Trim();
            // Disables the edit mode.
            IsPersonalInfoEditable = false;
            using (var db = new LonestarDbContext())
            {
                db.Personnels.Attach(MyUser);
                db.Entry(MyUser).State = EntityState.Modified;
                db.SaveChanges();
            }
            NotifyOfPropertyChange(null);
        }

        /// <summary>
        ///     Disables editing and revert any changes
        /// </summary>
        public void CancelPersonalInfoEditAction()
        {
            IsPersonalInfoEditable = false;
            // Restores the old values.
            MyUser = _tUser;
            _tUser = null;
            // Updates the visual interface.
            NotifyOfPropertyChange(null);
        }

        #endregion

        #region [User Image Methods]

        /// <summary>
        ///     Update the user image
        /// </summary>
        public void UpdateProfilePictureAction()
        {
            var openFile = new OpenFileDialog {Filter = Resources.ImageFilter};
            if (openFile.ShowDialog() != true) return;
            if (new FileInfo(openFile.FileName).Length < MyConfig.Configuration.MaxFileSize)
            {
                var imageData = File.ReadAllBytes(openFile.FileName);
                MyUser.Picture = imageData;
                NotifyOfPropertyChange(() => CustomUserPicture);
                SavePictureChangesToDatabase();
            }
            else
            {
                MessageBox.Show(Resources.FileSizeExceededMessage);
            }
        }

        /// <summary>
        ///     Deletes the user image locally.
        /// </summary>
        public void DeleteProfilePictureAction()
        {
            // Displays the confirmation message.
            var confirmationMessage = MessageBox.Show(Resources.DeletePictureMessage, Resources.ConfirmTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmationMessage != MessageBoxResult.Yes) return;
            // Clears the user picture and save changes to the db.
            MyUser.Picture = null;
            NotifyOfPropertyChange(() => CustomUserPicture);
            SavePictureChangesToDatabase();
        }

        /// <summary>
        ///     Updates the user image in the database.
        /// </summary>
        private void SavePictureChangesToDatabase()
        {
            using (var db = new LonestarDbContext())
            {
                db.Personnels.Attach(MyUser);
                db.Entry(MyUser).Property(p => p.Picture).IsModified = true;
                db.SaveChanges();
            }
        }

        #endregion
    }
}