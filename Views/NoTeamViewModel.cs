using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using LonestarShowdown.Custom;
using LonestarShowdown.Database;
using LonestarShowdown.Properties;

namespace LonestarShowdown.Views
{
    internal class NoTeamViewModel : Screen
    {
        private readonly int _userId;
        private LonestarDbContext _db;
        private bool _isManager;
        private Dictionary<string, int> _myTeamList;
        private string _noTeamMessage;
        private string _selectedTeam;

        public NoTeamViewModel(bool isManager, int? userId)
        {
            // Identifies whether or not the current user is a manager.
            IsManager = isManager;
            if (!IsManager && userId != null)
            {
                _userId = (int) userId;
                LoadData();
            }
            else
            {
                NoTeamMessage = string.Format(Resources.NoTeamManagerMessage);
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

        public string NoTeamMessage
        {
            get { return _noTeamMessage; }
            set
            {
                if (_noTeamMessage == value)
                    return;

                _noTeamMessage = value;
                NotifyOfPropertyChange(() => NoTeamMessage);
            }
        }

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

        public bool IsManager
        {
            get { return _isManager; }
            set
            {
                if (_isManager == value)
                    return;

                _isManager = value;
                NotifyOfPropertyChange(() => IsManager);
            }
        }

        /// <summary>
        ///     Loads all of the required data into the view.
        /// </summary>
        private void LoadData()
        {
            using (_db = new LonestarDbContext())
            {
                // Loads the list of available teams into the dictionary.
                MyTeamList =
                    _db.Teams.Where(t => t.IsApproved == true && t.TeamID != 1)
                        .Select(t => new {t.TeamName, t.TeamID})
                        .ToDictionary(t => t.TeamName, t => t.TeamID);

                var team = _db.Teams.Find(_db.Personnels.Find(_userId).TeamRequested);

                var teamName = string.Empty;
                if (team != null)
                {
                    teamName = team.TeamName;
                }

                NoTeamMessage = string.IsNullOrEmpty(teamName)
                    ? Resources.NoTeamFreeAgentMessage
                    : string.Format(Resources.NoTeamPlayerMessage, teamName);
            }
        }

        /// <summary>
        ///     Changes the team user requested.
        /// </summary>
        public void RequestTeamAction()
        {
            using (_db = new LonestarDbContext())
            {
                var myUser = _db.Personnels.Find(_userId);
                if (myUser == null) return;
                myUser.TeamRequested = MyTeamList[SelectedTeam];
                _db.SaveChanges();
            }
            NoTeamMessage = string.Format(Resources.NoTeamPlayerMessage, SelectedTeam);
            MessageBox.Show(string.Format(Resources.ChangeTeamRequestedMessage, SelectedTeam), Resources.SuccessTitle,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        ///     Brings up the team creation screen.
        /// </summary>
        public void DisplayCreateTeamScreenAction()
        {
            MyConfig.Configuration.CurrentUserScreenId = 5;
            var parentConductor = (Conductor<object>.Collection.OneActive) (Parent);
            parentConductor.ActivateItem(new CreateTeamViewModel());
            TryClose();
        }
    }
}