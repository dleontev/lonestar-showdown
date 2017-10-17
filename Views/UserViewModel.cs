using Caliburn.Micro;
using LonestarShowdown.Custom;
using LonestarShowdown.Database;

namespace LonestarShowdown.Views
{
    internal class UserViewModel : Conductor<object>.Collection.OneActive
    {
        private readonly Personnel _myUser;
        private Screen _myAccount;
        private Screen _myLeague;
        private Team _myTeam;
        private Screen _myTeamView;
        private string _myTeamMenuName = "My Team";
        private string _myLeagueMenuName = "My League";

        public string MyTeamMenuName
        {
            get { return _myTeamMenuName; }
            set
            {
                if (_myTeamMenuName == value)
                    return;

                _myTeamMenuName = value;
                NotifyOfPropertyChange(() => MyTeamMenuName);
            }
        }

        public string MyLeagueMenuName
        {
            get { return _myLeagueMenuName; }
            set
            {
                if (_myLeagueMenuName == value)
                    return;

                _myLeagueMenuName = value;
                NotifyOfPropertyChange(() => MyLeagueMenuName);
            }
        }

        public UserViewModel(Personnel p)
        {
            _myUser = p;
            // Assigns the current User Id and Team Id to the global state manager.
            MyConfig.Configuration.CurrentUserPid = p.PID;
            MyConfig.Configuration.CurrentUserTeamId = p.TeamID;


            if (MyConfig.Configuration.CurrentUserTeamId != null)
            {
                // Loads the user's team information.
                LoadTeam();
            }

            // Creates a new MyAccount view based on user's permission level (player/manager).
            switch (_myUser.PermissionLevel)
            {
                case 1:
                    _myAccount = new MyAccountPlayerViewModel(_myUser, _myTeam);
                    break;
                case 2:
                    _myAccount = new MyAccountManagerViewModel(_myUser);
                    break;
                case 3:
                    _myAccount = new MyAccountAdminViewModel(_myUser);
                    MyTeamMenuName = "Edit League";
                    MyLeagueMenuName = "View League";
                    break;
            }

            DisplayScreen((int) Screens.MyAccountScreen);
        }

        /// <summary>
        ///     Signs out the current user and brings up the sign in view.
        /// </summary>
        public void SignOutAction()
        {
            MyConfig.Configuration.CurrentUserTeamId = null;
            MyConfig.Configuration.CurrentUserPid = null;
            MyConfig.Configuration.CurrentUserScreenId = null;
            _myAccount = null;
            _myTeam = null;
            _myLeague = null;
            var parentConductor = (Conductor<object>) (Parent);
            parentConductor.DeactivateItem(this, true);
            parentConductor.ActivateItem(new SignInViewModel());
        }

        /// <summary>
        ///     Displays screen based on a specified screen ID.
        /// </summary>
        public void DisplayScreen(int screenId)
        {
            if (MyConfig.Configuration.CurrentUserScreenId == screenId) return;
            MyConfig.Configuration.CurrentUserScreenId = screenId;
            object newView = null;
            switch ((Screens) screenId)
            {
                case Screens.MyAccountScreen:
                {
                    newView = _myAccount;
                }
                    break;
                case Screens.MyTeamScreen:
                {
                    if (_myUser.PermissionLevel == 3)
                    {
                        _myTeamView = _myTeamView ?? new MyTeamAdminViewModel();
                    }
                    else
                    {
                        if (MyConfig.Configuration.HasTeamChanged)
                        {
                            MyConfig.Configuration.HasTeamChanged = false;
                            _myTeamView = null;
                        }
                        _myTeamView = _myTeamView ?? LoadTeamView();
                    }
                    newView = _myTeamView;
                }
                    break;
                case Screens.MyLeagueScreen:
                {
                    _myLeague = _myLeague ?? new MyLeagueViewModel();
                    newView = _myLeague;
                }
                    break;
            }
            ActivateItem(newView);
        }

        /// <summary>
        ///     Loads the team.
        /// </summary>
        private void LoadTeam()
        {
            using (var db = new LonestarDbContext())
            {
                _myTeam = db.Teams.Find(MyConfig.Configuration.CurrentUserTeamId);
            }
        }

        /// <summary>
        ///     Loads the MyTeamView screen.
        /// </summary>
        private Screen LoadTeamView()
        {
            if (MyConfig.Configuration.CurrentUserTeamId == null)
            {
                return new NoTeamViewModel(_myUser.PermissionLevel == 2, _myUser.PID);
            }
            LoadTeam();
            return new MyTeamViewModel(_myTeam, (_myUser.PermissionLevel == 2));
        }

        /// <summary>
        /// </summary>
        private enum Screens
        {
            MyAccountScreen = 1,
            MyTeamScreen = 2,
            MyLeagueScreen = 3
        }
    }
}