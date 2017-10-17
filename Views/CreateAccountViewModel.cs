using System;
using System.Collections.Generic;
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
    internal class CreateAccountViewModel : Screen
    {
        private LonestarDbContext _db;
        private string _imageFilePath;
        private bool _isPlayer = true;
        private Dictionary<string, int> _myPositionList;
        private Dictionary<string, int> _mySecurityQuestionList;
        private Dictionary<string, int> _myTeamList;
        private Personnel _newUser;
        private string _passwordMain;
        private string _passwordRetype;
        private string _securityAnswer;
        private string _selectedPosition;
        private string _selectedSecurityQuestion;
        private string _selectedTeam;

        public Dictionary<string, int> MyTeamList
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

        public string ImageFilePath
        {
            get { return _imageFilePath; }
            set
            {
                if (_imageFilePath == value)
                    return;

                _imageFilePath = value;
                NotifyOfPropertyChange(() => ImageFilePath);
            }
        }

        public Personnel NewUser
        {
            get { return _newUser; }
            set
            {
                if (_newUser == value)
                    return;

                _newUser = value;
                NotifyOfPropertyChange(() => NewUser);
            }
        }

        public bool IsPlayer
        {
            get { return _isPlayer; }
            set
            {
                if (_isPlayer == value)
                    return;

                _isPlayer = value;
                NewUser.PermissionLevel = _isPlayer ? 1 : 2;

                NotifyOfPropertyChange(() => MinDate);
                NotifyOfPropertyChange(() => IsPlayer);
            }
        }

        public DateTime MaxDate
        {
            get { return DateTime.Now.AddYears(-18); }
        }

        public DateTime MinDate
        {
            get
            {
                if (_isPlayer)
                {
                    var tMinDate = DateTime.Now.AddYears(-25);
                    if (DateTime.Compare(NewUser.DOB, tMinDate) < 0)
                    {
                        NewUser.DOB = tMinDate;
                    }
                    return tMinDate;
                }
                return Convert.ToDateTime(DateTime.Now.AddYears(-100));
            }
        }

        public string FirstName
        {
            get { return NewUser.FirstName; }

            set { NewUser.FirstName = value; }
        }

        public string LastName
        {
            get { return NewUser.LastName; }

            set { NewUser.LastName = value; }
        }

        public string Phone
        {
            get { return NewUser.Phone; }

            set { NewUser.Phone = value; }
        }

        public string Email
        {
            get { return NewUser.Email; }

            set { NewUser.Email = value; }
        }

        public string City
        {
            get { return NewUser.City; }

            set { NewUser.City = value; }
        }

        public string Address
        {
            get { return NewUser.Address; }

            set { NewUser.Address = value; }
        }

        public string SelectedSecurityQuestion
        {
            get { return _selectedSecurityQuestion; }
            set
            {
                if (_selectedSecurityQuestion == value)
                    return;

                _selectedSecurityQuestion = value;
                NewUser.SecurityQuestionID = MySecurityQuestionList[_selectedSecurityQuestion];
                NotifyOfPropertyChange(() => SelectedSecurityQuestion);
            }
        }

        public string SelectedTeam
        {
            get { return _selectedTeam; }
            set
            {
                if (_selectedTeam == value)
                    return;

                _selectedTeam = value;
                NotifyOfPropertyChange(() => SelectedTeam);
            }
        }

        public string SelectedPosition
        {
            get { return _selectedPosition; }
            set
            {
                if (_selectedPosition == value)
                    return;

                _selectedPosition = value;
                NotifyOfPropertyChange(() => SelectedPosition);
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

        #region [Initialization Methods]

        protected override void OnInitialize()
        {
            // Creates a new user record.
            NewUser = new Personnel {DOB = MaxDate, PermissionLevel = 1};

            // Generates new unique salt data for a new user.
            RandomNumberGenerator rng = new RNGCryptoServiceProvider();
            var tokenData = new byte[64];
            rng.GetBytes(tokenData);
            NewUser.SaltData = tokenData;

            using (var db = new LonestarDbContext())
            {
                // Loads the list of available teams into the dictionary.
                MyTeamList =
                    db.Teams.Where(t => t.IsApproved == true)
                        .Select(t => new {t.TeamName, t.TeamID})
                        .ToDictionary(t => t.TeamName, t => t.TeamID);

                // Loads the list of available positions into the dictionary.
                MyPositionList =
                    db.Positions.Select(t => new {t.Abbreviation, t.PositionID})
                        .ToDictionary(t => t.Abbreviation, t => t.PositionID);

                // Loads the list of the available security questions into the dictionary.
                MySecurityQuestionList =
                    db.SecurityQuestions.Select(t => new {t.SecurityQ, t.SecurityQuestionID})
                        .ToDictionary(t => t.SecurityQ, t => t.SecurityQuestionID);
            }

            base.OnInitialize();
        }

        #endregion

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
        ///     Opens up a new image selection dialog for user to select a profile image.
        /// </summary>
        public void SelectProfilePictureAction()
        {
            // Brings up the file selection dialog.
            var openFile = new OpenFileDialog {Filter = Resources.ImageFilter};
            if (openFile.ShowDialog() == true)
            {
                // Gets the image file path if the selected file doesn't exceed the max file size.
                if (new FileInfo(openFile.FileName).Length < MyConfig.Configuration.MaxFileSize)
                {
                    ImageFilePath = openFile.FileName;
                }
                else
                {
                    // Displays an error message.
                    MessageBox.Show(Resources.FileSizeExceededMessage, Resources.ErrorTitle, MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        ///     Creates new account and saves changes to the database.
        /// </summary>
        public void CreateAccountAction()
        {
            try
            {
                if (ValidateValues())
                {
                    using (_db = new LonestarDbContext())
                    {
                        // Trim the user data.
                        NewUser.FirstName = NewUser.FirstName.Trim();
                        NewUser.LastName = NewUser.LastName.Trim();
                        NewUser.City = NewUser.City.Trim();
                        NewUser.Address = NewUser.Address.Trim();

                        // Encodes the security answer and password.
                        HashAlgorithm hash = new SHA256Managed();
                        NewUser.SecurityAnswer =
                            hash.ComputeHash(NewUser.SaltData.Concat(Encoding.UTF8.GetBytes(SecurityAnswer)).ToArray());
                        NewUser.Password =
                            hash.ComputeHash(NewUser.SaltData.Concat(Encoding.UTF8.GetBytes(_passwordMain)).ToArray());

                        // Sets the 'Last changed' date for password and security question/answer to current time.
                        NewUser.PasswordChangeDate = DateTime.Now;
                        NewUser.SecurityQuestionChangeDate = DateTime.Now;

                        // Verifies that the image is selected and doesn't exceed the preset maximum file size.
                        if (!string.IsNullOrEmpty(ImageFilePath) &&
                            new FileInfo(ImageFilePath).Length < MyConfig.Configuration.MaxFileSize)
                        {
                            var imageData = File.ReadAllBytes(ImageFilePath);
                            NewUser.Picture = imageData;
                        }
                        if (IsPlayer)
                        {
                            NewUser.PermissionLevel = 1;

                            // Creates a new stats record for the player 
                            var newPlayerStat = new PlayerStat
                            {
                                Assists = 0,
                                GamesPlayed = 0,
                                GamesStarted = 0,
                                Goals = 0,
                                GoalsAllowed = 0,
                                RedCards = 0,
                                Saves = 0,
                                YellowCards = 0
                            };

                            // Looks up the positionId associated with the selected position.
                            NewUser.Position = MyPositionList[_selectedPosition];
                            // If the user chose to be a free agent => nullifies his requested team.
                            NewUser.TeamRequested = MyTeamList[_selectedTeam] == 1
                                ? (int?)null
                                : MyTeamList[_selectedTeam];

                            // Adds a new record and links it to the user.
                            _db.PlayerStats.Add(newPlayerStat);
                            _db.SaveChanges();
                            NewUser.StatID = newPlayerStat.StatID;
                        }

                        // Add a new user to the database.
                        _db.Personnels.Add(NewUser);
                        _db.SaveChanges();
                    }

                    // Take the user back to the sign in screen.
                    BackAction();
                }
            }
            catch (Exception)
            {

                MessageBox.Show("There's been an error. Please, verify all of the fields and try again.", Resources.ErrorTitle);
            }
        }

        /// <summary>
        ///     Validates password rules and if the email is already taken.
        /// </summary>
        private bool ValidateValues()
        {
            using (_db = new LonestarDbContext())
            {
                // Check if the record with the specified email already exists.
                var userEmail = NewUser.Email.ToLower();
                var email = _db.Personnels.FirstOrDefault(x => x.Email.ToLower() == userEmail);
                if (email != null)
                {
                    DisplayError(string.Format(Resources.EmailExistsMessage, NewUser.Email));
                    return false;
                }
            }

            if (string.IsNullOrEmpty(_passwordMain) && _passwordMain.Length <= 8)
            {
                DisplayError(Resources.EmptyPasswordMessage);
                return false;
            }

            if (_passwordMain != _passwordRetype)
            {
                DisplayError(Resources.PasswordsDontMatchMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Displays a generic MessageBox with a specified message.
        /// </summary>
        /// <param name="message">
        ///     Specifies the content of the MessageBox.
        /// </param>
        private static void DisplayError(string message)
        {
            MessageBox.Show(message, Resources.ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        /// <summary>
        ///     Takes the user back to the sign in screen.
        /// </summary>
        public void BackAction()
        {
            var parentConductor = (Conductor<object>) (Parent);
            parentConductor.ActivateItem(new SignInViewModel());
        }
    }
}