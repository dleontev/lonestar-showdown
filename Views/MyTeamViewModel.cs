using System;
using System.Collections.Generic;
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
    internal class MyTeamViewModel : Screen
    {
        private LonestarDbContext _db;
        private string[] _firstElevenNames;
        private int?[] _firstElevenNumbers;
        private bool _isColorEditable;
        private bool _isManager;
        private List<ScheduleItem> _mySchedule;
        private List<Player> _mySquad;
        private Team _myTeam;
        private List<Player> _myTeamSelection;
        private TeamT _myTeamStats;
        private Player _selectedPlayer;

        public bool IsPlayerListEmpty
        {
            get { return (MySquad.Count == 0); }
        }

        public bool IsScheduleEmpty
        {
            get { return (MySchedule.Count == 0); }
        }

        public Player SelectedPlayer
        {
            get { return _selectedPlayer; }
            set
            {
                if (_selectedPlayer == value)
                    return;

                _selectedPlayer = value;
                NotifyOfPropertyChange(() => SelectedPlayer);
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

        public bool IsColorEditable
        {
            get { return _isColorEditable; }
            set
            {
                if (_isColorEditable == value)
                    return;

                _isColorEditable = value;
                NotifyOfPropertyChange(() => IsColorEditable);
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

        public List<Player> MySquad
        {
            get { return _mySquad; }
            set
            {
                if (_mySquad == value)
                    return;

                _mySquad = value;
                NotifyOfPropertyChange(() => MySquad);
            }
        }

        public List<ScheduleItem> MySchedule
        {
            get { return _mySchedule; }
            set
            {
                if (_mySchedule == value)
                    return;

                _mySchedule = value;
                NotifyOfPropertyChange(() => MySchedule);
            }
        }

        public List<Player> MyTeamSelection
        {
            get { return _myTeamSelection; }
            set
            {
                if (_myTeamSelection == value)
                    return;

                _myTeamSelection = value;
                NotifyOfPropertyChange(() => MyTeamSelection);
            }
        }

        public TeamT MyTeamStats
        {
            get { return _myTeamStats; }
            set
            {
                _myTeamStats = value;
                NotifyOfPropertyChange(() => MyTeamStats);
            }
        }

        public string[] FirstElevenNames
        {
            get { return _firstElevenNames; }
            set
            {
                _firstElevenNames = value;
                NotifyOfPropertyChange(() => FirstElevenNames);
            }
        }

        public int?[] FirstElevenNumbers
        {
            get { return _firstElevenNumbers; }
            set
            {
                _firstElevenNumbers = value;
                NotifyOfPropertyChange(() => FirstElevenNumbers);
            }
        }

        /// <summary>
        ///     Brings up the <see cref="TeamSelectionView" /> screen.
        /// </summary>
        public void ViewTeamSelectionAction()
        {
            MyConfig.Configuration.CurrentUserScreenId = 4;
            var parentConductor = (Conductor<object>.Collection.OneActive) (Parent);
            parentConductor.ActivateItem(new TeamSelectionViewModel(MyTeamSelection, MyTeam));
        }

        #region [Initialization Methods]

        public MyTeamViewModel(Team t, bool isManager)
        {
            MyTeam = t;

            IsManager = isManager;

            LoadData();
        }

        protected override void OnActivate()
        {
            if (MyConfig.Configuration.HasTeamChanged)
            {
                using (_db = new LonestarDbContext())
                {
                    LoadPlayers();
                }
            }
            base.OnActivate();
        }

        #endregion

        #region [Data Load Methods]

        /// <summary>
        ///     Loads all the data into the form.
        /// </summary>
        private void LoadData()
        {
            using (_db = new LonestarDbContext())
            {
                LoadSchedule();
                LoadStats();
                LoadPlayers();
            }
        }

        /// <summary>
        ///     Loads the team stats.
        /// </summary>
        private void LoadStats()
        {
            if (MyTeam != null && MySchedule != null)
            {
                MyTeamStats = TeamStatsGenerator.GenerateTeamStats(MyTeam, MySchedule);
            }
        }

        /// <summary>
        ///     Loads the team schedule.
        /// </summary>
        private void LoadSchedule()
        {
            MySchedule = (from a in _db.Schedule
                join b in _db.Teams on a.HomeTeamID equals b.TeamID
                join c in _db.Teams on a.AwayTeamID equals c.TeamID
                where a.HomeTeamID == MyTeam.TeamID || a.AwayTeamID == MyTeam.TeamID
                select new ScheduleItem
                {
                    Date = (DateTime) a.Date,
                    HomeTeamId = (a.HomeTeamID == MyTeam.TeamID) ? a.HomeTeamID : a.AwayTeamID,
                    HomeTeamName = (a.HomeTeamID == MyTeam.TeamID) ? c.TeamName : b.TeamName,
                    HomeTeamGoals = (a.HomeTeamID == MyTeam.TeamID) ? a.HomeTeamGoals : a.AwayTeamGoals,
                    AwayTeamGoals = (a.HomeTeamID == MyTeam.TeamID) ? a.AwayTeamGoals : a.HomeTeamGoals,
                    AwayTeamId = (a.HomeTeamID == MyTeam.TeamID) ? a.AwayTeamID : a.HomeTeamID,
                    Venue = (a.HomeTeamID == MyTeam.TeamID) ? "H" : "A"
                })
                .OrderBy(a => a.Date).ToList();

            NotifyOfPropertyChange(() => IsScheduleEmpty);
        }

        /// <summary>
        ///     Loads the team players.
        /// </summary>
        private void LoadPlayers()
        {
            // Gets the list of all the player in the team with all of their info.
            MySquad = (from a in _db.Personnels
                join b in _db.Positions on a.Position equals b.PositionID
                join e in _db.PlayerStats on a.StatID equals e.StatID
                join c in _db.Squads on a.LineUpID equals c.LineUpID into joined
                where a.TeamID == MyTeam.TeamID && a.PermissionLevel == 1
                from d in joined.DefaultIfEmpty()
                select new {a, b, d, e})
                .Select(x => new Player
                {
                    LineUpId = x.a.LineUpID,
                    PlayerId = x.a.PID,
                    ImageData = x.a.Picture,
                    FirstName = x.a.FirstName,
                    LastName = x.a.LastName,
                    DateOfBirth = x.a.DOB,
                    Address = x.a.Address,
                    City = x.a.City,
                    PhoneNumber = x.a.Phone,
                    LineUpPosition = x.d.SquadPosition,
                    Position = x.b.Description,
                    JerseyNumber = (int) x.a.JerseyNumber,
                    GamesPlayed = x.e.GamesPlayed,
                    GamesStarted = x.e.GamesStarted,
                    Goals = x.e.Goals,
                    Assists = x.e.Assists,
                    YellowCards = x.e.YellowCards,
                    RedCards = x.e.RedCards,
                    Saves = (int) x.e.Saves,
                    GoalsAllowed = (int) x.e.GoalsAllowed
                }).ToList();

            NotifyOfPropertyChange(() => IsPlayerListEmpty);

            // Orders the list of players by the name of the position in the lineup.
            MyTeamSelection = MySquad.OrderBy(x => x.LineUpPosition == null).ThenBy(x => x.LineUpPosition).ToList();

            // Extracts the first eleven's jersey numbers and last names.
            FirstElevenNames = new string[11];
            FirstElevenNumbers = new int?[11];
            foreach (var item in MySquad)
            {
                if (item.LineUpId == null || (item.LineUpId > 11)) continue;
                FirstElevenNames[(int) item.LineUpId - 1] = item.LastName;
                FirstElevenNumbers[(int) item.LineUpId - 1] = item.JerseyNumber;
            }

            // Forces the view to update all of the bindings.
            NotifyOfPropertyChange(null);
        }

        /// <summary>
        ///     Reloads the players' list.
        /// </summary>
        public void RefreshPlayersAction()
        {
            using (_db = new LonestarDbContext())
            {
                LoadPlayers();
            }
            NotifyOfPropertyChange(null);
        }

        /// <summary>
        ///     Reloads the schedule.
        /// </summary>
        public void RefreshScheduleAction()
        {
            using (_db = new LonestarDbContext())
            {
                LoadSchedule();
            }
            NotifyOfPropertyChange(null);
        }

        #endregion

        #region [Delete Methods]

        /// <summary>
        ///     Deletes user team.
        /// </summary>
        public void DeleteTeamAction()
        {
            // Confirms that the deletion of the team.
            var confirmation = MessageBox.Show(Resources.DeleteTeamMessage, Resources.ConfirmTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (confirmation != MessageBoxResult.Yes) return;
            using (_db = new LonestarDbContext())
            {
                // Gets the team's record from the db.
                var userTeam = _db.Teams.Find(MyTeam.TeamID);
                if (userTeam == null) return;
                // Finds all of the people assigned to the team and release them.
                var teamPersonnel = (from a in _db.Personnels where a.TeamID == MyTeam.TeamID select a);
                foreach (var player in teamPersonnel)
                {
                    player.TeamID = null;
                }

                // Makes the team disapproved.
                userTeam.IsApproved = null;
                // Adds [DELETED] to the team's name.
                userTeam.TeamName = string.Format("{0} {1}", userTeam.TeamName, "[DELETED]");
                // Sets the global team identificator to null.
                MyConfig.Configuration.CurrentUserTeamId = null;
                MyConfig.Configuration.HasTeamChanged = true;
                _db.SaveChanges();
                // Brings up the NoTeam view.
                var parentConductor = (Conductor<object>.Collection.OneActive) (Parent);
                parentConductor.ActivateItem(new NoTeamViewModel(true, null));
                TryClose();
            }
        }

        /// <summary>
        ///     Deletes the player from the team.
        /// </summary>
        public void DeletePlayerAction()
        {
            if (SelectedPlayer == null) return;
            var confirmation = MessageBox.Show(string.Format(Resources.RemovePlayerMessage, SelectedPlayer.Name),
                Resources.ConfirmTitle, MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (confirmation != MessageBoxResult.Yes) return;
            using (_db = new LonestarDbContext())
            {
                // Finds the selected player's record in the database.
                var myPlayer = _db.Personnels.Find(SelectedPlayer.PlayerId);
                // Releases from the team and nullify the jersey number
                myPlayer.TeamID = null;
                myPlayer.JerseyNumber = null;
                myPlayer.LineUpID = null;
                // Marks the modified properties
                _db.Entry(myPlayer).Property(p => p.TeamID).IsModified = true;
                _db.Entry(myPlayer).Property(p => p.JerseyNumber).IsModified = true;
                _db.Entry(myPlayer).Property(p => p.LineUpID).IsModified = true;
                _db.SaveChanges();
                //  Reload the list of team's players.
                LoadPlayers();
                MyConfig.Configuration.HasPlayerListChanged = true;
            }
        }

        #endregion

        #region [Team Logo Update Methods]

        /// <summary>
        ///     Updates the team logo locally.
        /// </summary>
        public void UpdateTeamLogoAction()
        {
            // Brings up the file selection dialog.
            var openFile = new OpenFileDialog {Filter = Resources.ImageFilter};
            if (openFile.ShowDialog() != true) return;
            if (new FileInfo(openFile.FileName).Length < MyConfig.Configuration.MaxFileSize)
            {
                // Reads the selected file into a byte array.
                var imageData = File.ReadAllBytes(openFile.FileName);
                // Updates the logo and the bindings.
                MyTeam.TeamLogo = imageData;
                NotifyOfPropertyChange(null);
                // Saves changes.
                SavePictureChangesToDatabase();
            }
            else
            {
                MessageBox.Show(Resources.FileSizeExceededMessage, Resources.ErrorTitle, MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        ///     Updates the user image in the database.
        /// </summary>
        private void SavePictureChangesToDatabase()
        {
            using (_db = new LonestarDbContext())
            {
                // Attaches the current team to the db, updates the logo and saves changes.
                _db.Teams.Attach(MyTeam);
                _db.Entry(MyTeam).Property(p => p.TeamLogo).IsModified = true;
                _db.SaveChanges();
            }
        }

        #endregion

        #region [Color Edit Methods]

        /// <summary>
        ///     Allows the manager to edit team colors.
        /// </summary>
        public void EditTeamColorsAction()
        {
            IsColorEditable = true;
        }

        /// <summary>
        ///     Save the team colors to the database.
        /// </summary>
        public void SaveTeamColorsAction()
        {
            IsColorEditable = false;
            using (_db = new LonestarDbContext())
            {
                _db.Teams.Attach(MyTeam);
                _db.Entry(MyTeam).Property(p => p.PrimaryColor).IsModified = true;
                _db.Entry(MyTeam).Property(p => p.SecondaryColor).IsModified = true;
                _db.SaveChanges();
            }
        }

        #endregion
    }
}