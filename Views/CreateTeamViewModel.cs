using System.IO;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using LonestarShowdown.Custom;
using LonestarShowdown.Database;
using LonestarShowdown.Properties;
using Microsoft.Win32;

namespace LonestarShowdown.Views
{
    internal class CreateTeamViewModel : Screen
    {
        private LonestarDbContext _db;
        private Team _myTeam = new Team {IsApproved = false, PrimaryColor = "#000000", SecondaryColor = "#FFFFFF", IsSuspended = false};
        private byte[] _teamLogoImage;

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

        public byte[] TeamLogoImage
        {
            get { return _teamLogoImage; }
            set
            {
                if (_teamLogoImage == value)
                    return;

                _teamLogoImage = value;
                NotifyOfPropertyChange(() => TeamLogoImage);
            }
        }

        public string TeamName
        {
            get { return MyTeam.TeamName; }
            set { MyTeam.TeamName = value; }
        }

        public string City
        {
            get { return MyTeam.City; }
            set { MyTeam.City = value; }
        }

        /// <summary>
        ///     Creates a new team.
        /// </summary>
        public void CreateTeamAction()
        {
            using (_db = new LonestarDbContext())
            {
                // Verifies that the name is not taken.
                if (!TeamExists(MyTeam.TeamName))
                {
                    // If the manager has not selected a logo, assigns the default 'No Logo' logo.
                    MyTeam.TeamLogo = TeamLogoImage ?? Resources.DefaultTeamLogo;
                    _db.Teams.Add(MyTeam);
                    _db.SaveChanges();

                    // Assigns the TeamID of a newly created team to the user.
                    var myUser = _db.Personnels.Find(MyConfig.Configuration.CurrentUserPid);
                    myUser.TeamID = MyTeam.TeamID;
                    _db.SaveChanges();

                    MyConfig.Configuration.CurrentUserTeamId = MyTeam.TeamID;
                    MyConfig.Configuration.HasTeamChanged = true;

                    // Takes user to a new MyTeamView screen with a newly created team.
                    var parentConductor = (Conductor<object>.Collection.OneActive) (Parent);
                    parentConductor.ActivateItem(new MyTeamViewModel(MyTeam, true));
                    parentConductor.CloseItem(this);
                }
                else
                {
                    MessageBox.Show(string.Format(Resources.TeamExistsMessage, MyTeam.TeamName), Resources.ErrorTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        ///     Checks that the team name is not taken.
        /// </summary>
        private bool TeamExists(string teamName)
        {
            var team = _db.Teams.FirstOrDefault(x => x.TeamName == teamName);
            return (team != null);
        }

        /// <summary>
        ///     Gets the team logo from the user.
        /// </summary>
        public void SelectTeamLogoAction()
        {
            // Brings up the file selection dialog.
            var openFile = new OpenFileDialog {Filter = Resources.ImageFilter};
            if (openFile.ShowDialog() != true) return;
            // Verify that the selected file doesn't exceed the specified max file size.
            if (new FileInfo(openFile.FileName).Length < MyConfig.Configuration.MaxFileSize)
            {
                // Load the image file.
                TeamLogoImage = File.ReadAllBytes(openFile.FileName);
            }
            else
            {
                MessageBox.Show(Resources.FileSizeExceededMessage, Resources.ErrorTitle, MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        ///     Takes the user back to the NoTeamView screen.
        /// </summary>
        public void BackAction()
        {
            TryClose();
        }
    }
}