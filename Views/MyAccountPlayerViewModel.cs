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
    internal class MyAccountPlayerViewModel : Screen
    {
        private DynamicStat _dynamicStatA;
        private DynamicStat _dynamicStatB;
        private bool _isPersonalInfoEditable;
        private bool _isSecurityInfoEditable;
        private Dictionary<string, int> _myPositionList;
        private Dictionary<string, int> _mySecurityQuestionList;
        private PlayerStat _myStats;
        private Team _myTeam;
        private Personnel _myUser;
        private PasswordBox _newPasswordBoxMain;
        private PasswordBox _newPasswordBoxRetype;
        private string _newPasswordMain;
        private string _newPasswordRetype;
        private PasswordBox _oldPasswordBox;
        private string _oldPasswordMain;
        private string _position;
        private string _securityAnswer;
        private string _selectedSecurityQuestion;
        private Personnel _tUser;

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

        public DateTime MaxDate
        {
            get { return DateTime.Now.AddYears(-18); }
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

        public Personnel MyUser
        {
            get { return _myUser; }
            set
            {
                if (_myUser == value)
                    return;

                _myUser = value;
                NotifyOfPropertyChange(() => MyUser);
            }
        }

        public PlayerStat MyStats
        {
            get { return _myStats; }
            set
            {
                if (_myStats == value)
                    return;

                _myStats = value;
                NotifyOfPropertyChange(() => MyStats);
            }
        }

        public Team MyTeam
        {
            get { return _myTeam; }
            set
            {
                if (_myTeam == value)
                    return;

                _myTeam = value;
                NotifyOfPropertyChange(() => MyTeam);
            }
        }

        public Dictionary<string, int> MyPositionList
        {
            get { return _myPositionList; }
            set
            {
                if (_myPositionList == value)
                    return;

                _myPositionList = value;
                NotifyOfPropertyChange(() => MyPositionList);
            }
        }

        public string Position
        {
            get { return _position; }
            set
            {
                if (_position == value)
                    return;

                _position = value;
                MyUser.Position = MyPositionList[value];
                NotifyOfPropertyChange(() => Position);
            }
        }

        public string DisplayedName
        {
            get { return string.Format("{0} {1}", MyUser.FirstName, MyUser.LastName); }
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

        public bool IsAllowedToSave
        {
            get
            {
                return (!string.IsNullOrEmpty(_oldPasswordMain) && !string.IsNullOrEmpty(_newPasswordMain) &&
                        !string.IsNullOrEmpty(_newPasswordRetype)) || !(string.IsNullOrEmpty(SecurityAnswer));
            }
        }

        public bool IsNoTeam
        {
            get { return (MyTeam == null); }
        }

        public byte[] CustomUserPicture
        {
            get { return MyUser.Picture ?? Resources.BlankProfile; }
        }

        public byte[] CustomTeamPicture
        {
            get { return IsNoTeam ? Resources.BlankTeam : MyTeam.TeamLogo; }
        }

        public DynamicStat DynamicStatA
        {
            get { return _dynamicStatA; }
            set
            {
                _dynamicStatA = value;
                NotifyOfPropertyChange(() => DynamicStatA);
            }
        }

        public DynamicStat DynamicStatB
        {
            get { return _dynamicStatB; }
            set
            {
                _dynamicStatB = value;
                NotifyOfPropertyChange(() => DynamicStatB);
            }
        }

        #region [Stats Methods]

        // Loads the information for the dynamic player stats.
        private void ChangeStats(bool isPlayerGoalkeeper)
        {
            // Loads the first dynamic stat.
            var statA = isPlayerGoalkeeper ? MyStats.Saves : MyStats.Goals;
            if (statA != null)
                DynamicStatA = new DynamicStat
                {
                    FullName = isPlayerGoalkeeper ? "Saves" : "Goals",
                    ShortName = isPlayerGoalkeeper ? "SV" : "G",
                    Value = (int) statA
                };
            // Loads the second dynamic stat.
            var statB = isPlayerGoalkeeper ? MyStats.GoalsAllowed : MyStats.Assists;
            if (statB != null)
                DynamicStatB = new DynamicStat
                {
                    FullName = isPlayerGoalkeeper ? "GoalsAllowed" : "Assists",
                    ShortName = isPlayerGoalkeeper ? "GA" : "A",
                    Value = (int) statB
                };
        }

        #endregion

        public struct DynamicStat
        {
            public string FullName { get; set; }
            public string ShortName { get; set; }
            public int Value { get; set; }
        }

        #region [Initialization Methods]

        /// <summary>
        /// </summary>
        /// <param name="p"></param>
        /// <param name="t"></param>
        public MyAccountPlayerViewModel(Personnel p, Team t)
        {
            MyUser = p;
            MyTeam = t;
            NotifyOfPropertyChange(() => IsNoTeam);
            LoadData();
        }

        /// <summary>
        ///     Inserts the data from the query into the form.
        /// </summary>
        private void LoadData()
        {
            using (var db = new LonestarDbContext())
            {
                // Loads the security questions.
                MySecurityQuestionList =
                    db.SecurityQuestions.Select(t => new {t.SecurityQ, t.SecurityQuestionID})
                        .ToDictionary(t => t.SecurityQ, t => t.SecurityQuestionID);

                // Loads the available positions.
                MyPositionList =
                    db.Positions.Select(t => new {t.Abbreviation, t.PositionID})
                        .ToDictionary(t => t.Abbreviation, t => t.PositionID);

                // Gets the name of the position.
                Position = MyPositionList.FirstOrDefault(a => a.Value == MyUser.Position).Key;

                // Loads the user stats.
                MyStats = db.PlayerStats.Find(MyUser.StatID);

                ChangeStats(MyUser.Position == 1);
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
        ///     Reset the security tab.
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
        ///     Clears the passwordbox after insuring it's not null.
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
        ///     Enables personal info editing.
        /// </summary>
        public void EditPersonalInfoAction()
        {
            IsPersonalInfoEditable = true;
            using (var db = new LonestarDbContext())
            {
                // Copies the currents user's record.
                _tUser = db.Personnels.Find(MyUser.PID);
            }
        }

        /// <summary>
        ///     Saves changes to the database.
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
                // Updates the visuals.
                ChangeStats(MyUser.Position == 1);
                NotifyOfPropertyChange(() => DisplayedName);
                // Attaches the user record to the db, sets it state to modified, and updates it in the db.
                db.Personnels.Attach(MyUser);
                db.Entry(MyUser).State = EntityState.Modified;
                db.SaveChanges();
            }
            NotifyOfPropertyChange(null);
        }

        /// <summary>
        ///     Disables the edit-mode and reverts any changes
        /// </summary>
        public void CancelPersonalInfoEditAction()
        {
            // Disables edit mode.
            IsPersonalInfoEditable = false;
            // Reloads the copy of the user record to cancel any changes.
            Position = MyPositionList.FirstOrDefault(x => x.Value == _tUser.Position).Key;
            MyUser = _tUser;
            _tUser = null;
            // Updates the visual interface.
            NotifyOfPropertyChange(null);
        }

        #endregion

        #region [User Image Methods]

        /// <summary>
        ///     Updates the user image.
        /// </summary>
        public void UpdateProfilePictureAction()
        {
            // Displays the file selection dialog.
            var openFile = new OpenFileDialog {Filter = Resources.ImageFilter};
            if (openFile.ShowDialog() != true) return;
            // Verifies that the file length doesn't exceed the max file size.
            if (new FileInfo(openFile.FileName).Length < MyConfig.Configuration.MaxFileSize)
            {
                // Read the image into the byte array and update the Picture property..
                var imageData = File.ReadAllBytes(openFile.FileName);
                MyUser.Picture = imageData;
                // Update the visual interface.
                NotifyOfPropertyChange(() => CustomUserPicture);
                SavePictureChangesToDatabase();
            }
            else
            {
                // Displays the file size exceeded error message.
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
            // Delete the user picture.
            MyUser.Picture = null;
            // Update the visual interface.
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
                // Attaches the current user to the db context.
                db.Personnels.Attach(MyUser);
                // Sets the Picture property to modified and save changes to the db.
                db.Entry(MyUser).Property(p => p.Picture).IsModified = true;
                db.SaveChanges();
            }
        }

        #endregion
    }
}