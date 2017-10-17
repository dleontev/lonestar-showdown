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
    internal class MyAccountManagerViewModel : Screen
    {
        private LonestarDbContext _db;
        private bool _isPersonalInfoEditable;
        private bool _isSecurityInfoEditable;
        private List<Player> _myFreeAgents = new List<Player>();
        private List<Player> _myPlayerRequests = new List<Player>();
        private Dictionary<string, int> _mySecurityQuestionList;
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
        public MyAccountManagerViewModel(Personnel p)
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

        public bool IsPlayerRequestsListEmpty
        {
            get { return (MyPlayerRequests.Count == 0); }
        }

        public bool IsFreeAgentListEmpty
        {
            get { return (MyFreeAgents.Count == 0); }
        }

        public bool? IsAuthorizedToAddPlayers
        {
            get
            {
                if (MyUser.TeamID != null)
                {
                    using (var db = new LonestarDbContext())
                    {
                        return db.Teams.Find(MyUser.TeamID).IsApproved;
                    }
                }
                return false;
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

        public List<Player> MyFreeAgents
        {
            get { return _myFreeAgents; }
            set
            {
                if (_myFreeAgents == value)
                    return;

                _myFreeAgents = value;
                NotifyOfPropertyChange(null);
            }
        }

        public List<Player> MyPlayerRequests
        {
            get { return _myPlayerRequests; }
            set
            {
                if (_myPlayerRequests == value)
                    return;

                _myPlayerRequests = value;
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

        #region [Player Transfer Methods]

        // Call the method that reloads the player lists.
        public void RefreshAction()
        {
            using (_db = new LonestarDbContext())
            {
                ReloadLists();
            }
        }

        /// <summary>
        ///     Loads all of the required data for the view.
        /// </summary>
        private void LoadData()
        {
            using (_db = new LonestarDbContext())
            {
                // Loads the security questions.
                MySecurityQuestionList =
                    _db.SecurityQuestions.Select(t => new {t.SecurityQ, t.SecurityQuestionID})
                        .ToDictionary(t => t.SecurityQ, t => t.SecurityQuestionID);

                ReloadLists();
            }
        }

        protected override void OnActivate()
        {
            // Refreshes the free agent and the player request pool, if the player has been kicked off the team
            if (MyConfig.Configuration.HasPlayerListChanged)
            {
                MyConfig.Configuration.HasPlayerListChanged = false;
                RefreshAction();
            }
            base.OnActivate();
        }

        // Reloads the free agent list and the player request list.
        /// <summary>
        /// </summary>
        private void ReloadLists()
        {
            // Verifies that the manager has the team and it is approved.
            if (IsAuthorizedToAddPlayers != true) return;
            // Finds all the free agents and player requests for the manager's team.
            var tList = (from a in _db.Personnels
                join b in _db.Positions on a.Position equals b.PositionID
                where
                    (a.TeamRequested == MyUser.TeamID || a.TeamRequested == null) && a.TeamID == null &&
                    a.PermissionLevel == 1
                select new Player
                {
                    PlayerId = a.PID,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Position = b.Abbreviation,
                    City = a.City,
                    IsFreeAgent = (a.TeamRequested == null)
                }).ToList<Player>();

            // Splits the free agents and player requests between two list.
            MyFreeAgents = tList.Where(a => a.IsFreeAgent).ToList();
            MyPlayerRequests = tList.Where(a => a.IsFreeAgent == false).ToList();

            // Updates the visual interface.
            NotifyOfPropertyChange(() => IsPlayerRequestsListEmpty);
            NotifyOfPropertyChange(() => IsFreeAgentListEmpty);
        }

        // Removes the player from the player request list into the freeagent pool.
        /// <summary>
        /// </summary>
        /// <param name="selectedPlayer"></param>
        public void RejectPlayerRequestAction(Player selectedPlayer)
        {
            if (selectedPlayer == null) return;
            // Confirms the rejection of the request.
            var confirmation =
                MessageBox.Show(string.Format(Resources.RejectConfirmationMessage, selectedPlayer.Name),
                    Resources.ConfirmTitle, MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (confirmation != MessageBoxResult.Yes) return;
            using (_db = new LonestarDbContext())
            {
                var player = _db.Personnels.Find(selectedPlayer.PlayerId);
                player.TeamRequested = null;
                _db.SaveChanges();
                ReloadLists();
            }
        }

        // Adds the player to the manager's team.
        /// <summary>
        /// </summary>
        /// <param name="selectedPlayer"></param>
        public void AddPlayerToTeam(Player selectedPlayer)
        {
            // Verifies that the player is selected.
            if (selectedPlayer == null) return;
            // Displays the confirmation.
            var confirmation = MessageBox.Show(
                string.Format(Resources.AddConfirmationMessage, selectedPlayer.Name), Resources.ConfirmTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes) return;
            using (_db = new LonestarDbContext())
            {
                // Finds the player in the database.
                var player = _db.Personnels.Find(selectedPlayer.PlayerId);

                // Verifies that the player is not already take to avoid conflict.
                if (player.TeamID == null)
                {
                    // Load all the taken jersey numbers for the current team
                    var jerseyNumbers = (from a in _db.Personnels
                        where a.TeamID == MyUser.TeamID && a.PermissionLevel == 1 && a.JerseyNumber != null
                        select a.JerseyNumber).ToList();

                    // Generates a new jersey number.
                    var r = new Random();
                    int newJersey;
                    do
                    {
                        newJersey = r.Next(1, 99);
                    } while (jerseyNumbers.Contains(newJersey));
                    player.JerseyNumber = newJersey;

                    // Assign the selected player to the manager's team.
                    player.TeamRequested = null;
                    player.TeamID = MyUser.TeamID;
                    _db.SaveChanges();

                    MyConfig.Configuration.HasTeamChanged = true;
                }
                else
                {
                    MessageBox.Show(Resources.PlayerAlreadyTaken, Resources.ErrorTitle, MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                // Reload the free agent list and the player request list.
                ReloadLists();
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
        /// <param name="passBox"></param>
        /// <param name="type"></param>
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