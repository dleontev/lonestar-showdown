using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using LonestarShowdown.Custom;
using LonestarShowdown.Database;
using LonestarShowdown.Properties;

namespace LonestarShowdown.Views
{
    internal class MyTeamAdminViewModel : Screen
    {
        private LonestarDbContext _db;
        private List<ScheduleItem> _leagueSchedule;
        private List<Player> _myFullPlayerList;
        private List<Player> _myManagersList;
        private List<Player> _myPlayersList;
        private Dictionary<string, int> _myTeamDictionary;
        private List<TeamT> _myTeamsList;
        private Schedule _newScheduleItem = new Schedule();
        private Player _selectedPlayer;
        private ScheduleItem _selectedScheduleItem = new ScheduleItem();
        private string[] _scores = new string[100];

        public int AddHomeTeamGoals
        {
            get
            {
                return (NewScheduleItem.HomeTeamGoals == null) ? -1 : (int)NewScheduleItem.HomeTeamGoals + 1;
            }
            set
            {
                NewScheduleItem.HomeTeamGoals = (value == 0 || value == -1) ? (int?)null : value - 1;
            }
        }

        public int AddAwayTeamGoals
        {
            get
            {
                return (NewScheduleItem.AwayTeamGoals == null) ? -1 : (int)NewScheduleItem.AwayTeamGoals + 1;
            }
            set
            {
                NewScheduleItem.AwayTeamGoals = (value == 0 || value == -1) ? (int?)null : value - 1;
            }
        }

        public int EditHomeTeamGoals
        {
            get
            {
                if (SelectedScheduleItem != null)
                {
                    return (SelectedScheduleItem.HomeTeamGoals == null) ? 0 : (int)SelectedScheduleItem.HomeTeamGoals + 1;
                }
                return 0;
            }
            set
            {
                if (SelectedScheduleItem != null)
                {
                    SelectedScheduleItem.HomeTeamGoals = (value == 0 || value == -1) ? (int?)null : value - 1;
                }
            }
        }

        public int EditAwayTeamGoals
        {
            get
            {
                if (SelectedScheduleItem != null)
                {
                    return (SelectedScheduleItem.AwayTeamGoals == null) ? 0 : (int)SelectedScheduleItem.AwayTeamGoals + 1;
                }
                return 0;
            }
            set
            {
                if (SelectedScheduleItem != null)
                {
                    SelectedScheduleItem.AwayTeamGoals = (value == 0 || value == -1) ? (int?)null : value - 1;
                }
            }
        }

        #region [Initialization Methods]

        public MyTeamAdminViewModel()
        {
            LoadData();

            _scores[0] = "N/A";

            for (int i = 1; i < _scores.Length; i++)
            {
                _scores[i] = (i-1).ToString();
            }
        }

        #endregion

        public string UserFilter
        {
            set { MyPlayersList = _myFullPlayerList.Where(x => x.Name.ToLower().Contains(value.ToLower())).ToList(); }
        }

        public string[] ScoresList
        {
            get 
            { 
                return _scores; 
            }
        }

        public List<ScheduleItem> LeagueSchedule
        {
            get { return _leagueSchedule; }
            set
            {
                if (_leagueSchedule == value)
                    return;

                _leagueSchedule = value;
                NotifyOfPropertyChange(() => LeagueSchedule);
            }
        }

        public Dictionary<string, int> MyTeamDictionary
        {
            get { return _myTeamDictionary; }
            set
            {
                if (_myTeamDictionary == value)
                    return;

                _myTeamDictionary = value;
                NotifyOfPropertyChange(() => MyTeamDictionary);
            }
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

        public ScheduleItem SelectedScheduleItem
        {
            get { return _selectedScheduleItem; }
            set
            {
                if (_selectedScheduleItem == value)
                    return;

                _selectedScheduleItem = value;
                NotifyOfPropertyChange(() => SelectedScheduleItem);
                if (value != null)
                {
                    NotifyOfPropertyChange(() => EditHomeTeamGoals);
                    NotifyOfPropertyChange(() => EditAwayTeamGoals);
                }
            }
        }

        public List<Player> MyPlayersList
        {
            get { return _myPlayersList; }
            set
            {
                if (_myPlayersList == value)
                    return;

                _myPlayersList = value;
                NotifyOfPropertyChange(() => MyPlayersList);
            }
        }

        public string SelectedHomeTeam
        {
            set { _newScheduleItem.HomeTeamID = MyTeamDictionary[value]; }
        }

        public string SelectedAwayTeam
        {
            set { _newScheduleItem.AwayTeamID = MyTeamDictionary[value]; }
        }

        public Schedule NewScheduleItem
        {
            get { return _newScheduleItem; }
            set
            {
                if (_newScheduleItem == value)
                    return;

                _newScheduleItem = value;
                NotifyOfPropertyChange(() => NewScheduleItem);
            }
        }

        public List<TeamT> MyTeamsList
        {
            get { return _myTeamsList; }
            set
            {
                if (_myTeamsList == value)
                    return;

                _myTeamsList = value;
                NotifyOfPropertyChange(() => MyTeamsList);
            }
        }

        public List<Player> MyManagersList
        {
            get { return _myManagersList; }
            set
            {
                if (_myManagersList == value)
                    return;

                _myManagersList = value;
                NotifyOfPropertyChange(() => MyManagersList);
            }
        }

        #region [Team Methods]

        /// <summary>
        ///     Approves the team.
        /// </summary>
        public void ApproveTeamAction(TeamT team)
        {
            if (team == null) return;
            // Confir
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
                LoadTeams();
            }
        }

        /// <summary>
        ///     Rejects the team.
        /// </summary>
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
                LoadTeams();
            }
        }

        public void SuspendTeamAction(TeamT team, bool suspend)
        {
            if (team == null) return;
            var confirmation =
                MessageBox.Show(string.Format(suspend ? Resources.SuspendTeamMessage : Resources.UnsuspendTeamMessage, team.TeamName),
                    Resources.ConfirmTitle,
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes) return;
            using (_db = new LonestarDbContext())
            {
                var t = _db.Teams.Find(team.TeamId);
                t.IsSuspended = suspend;
                _db.SaveChanges();
                LoadTeams();
            }
        }

        #endregion

        #region [Schedule Methods]

        /// <summary>
        /// </summary>
        public void AddScheduleItemAction()
        {
            if (NewScheduleItem.HomeTeamID == 0 || NewScheduleItem.AwayTeamID == 0)
            {
                MessageBox.Show(Resources.InvalidTeamSelectionMessage, Resources.ErrorTitle, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }
            if (NewScheduleItem.HomeTeamID == NewScheduleItem.AwayTeamID)
            {
                MessageBox.Show(Resources.SameTeamSelectionMessage, Resources.ErrorTitle, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }
            if (NewScheduleItem.Date == null)
            {
                MessageBox.Show(Resources.NullDateTimeMessage, Resources.ErrorTitle, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            if (NewScheduleItem.HomeTeamGoals == null || NewScheduleItem.AwayTeamGoals == null)
            {
                NewScheduleItem.HomeTeamGoals = null;
                NewScheduleItem.AwayTeamGoals = null;
            }

            using (_db = new LonestarDbContext())
            {
                _db.Schedule.Add(NewScheduleItem);
                _db.SaveChanges();

                LoadSchedule();
            }
        }

        /// <summary>
        /// </summary>
        public void SaveScheduleItemAction()
        {
            if (SelectedScheduleItem == null) return;
            if (SelectedScheduleItem.HomeTeamId == SelectedScheduleItem.AwayTeamId)
            {
                MessageBox.Show(Resources.SameTeamSelectionMessage, Resources.ErrorTitle, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }
            using (_db = new LonestarDbContext())
            {
                var scheduleItem = _db.Schedule.Find(SelectedScheduleItem.Id);

                if (scheduleItem == null) return;

                scheduleItem.Date = SelectedScheduleItem.Date;

                if (SelectedScheduleItem.HomeTeamGoals == null || SelectedScheduleItem.AwayTeamGoals == null)
                {
                    scheduleItem.HomeTeamGoals = null;
                    scheduleItem.AwayTeamGoals = null;
                }
                else
                {
                    scheduleItem.HomeTeamGoals = SelectedScheduleItem.HomeTeamGoals;
                    scheduleItem.AwayTeamGoals = SelectedScheduleItem.AwayTeamGoals;
                }
                scheduleItem.HomeTeamID = MyTeamDictionary[SelectedScheduleItem.HomeTeamName];
                scheduleItem.AwayTeamID = MyTeamDictionary[SelectedScheduleItem.AwayTeamName];

                _db.SaveChanges();

                LoadSchedule();
            }
        }

        public void SavePlayerAction()
        {
            if (SelectedPlayer == null) return;
            using (_db = new LonestarDbContext())
            {
                var playerStats = _db.PlayerStats.Find(SelectedPlayer.StatId);

                if (playerStats == null) return;

                playerStats.GamesPlayed = SelectedPlayer.GamesPlayed;
                playerStats.GamesStarted = SelectedPlayer.GamesStarted;
                playerStats.Goals = SelectedPlayer.Goals;
                playerStats.Assists = SelectedPlayer.Assists;
                playerStats.RedCards = SelectedPlayer.RedCards;
                playerStats.YellowCards = SelectedPlayer.YellowCards;
                playerStats.Saves = SelectedPlayer.Saves;
                playerStats.YellowCards = SelectedPlayer.YellowCards;
                playerStats.GoalsAllowed = SelectedPlayer.GoalsAllowed;

                _db.SaveChanges();

                LoadPlayers();
            }

            SelectedPlayer = null;
            NotifyOfPropertyChange(() => SelectedPlayer);
        }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public void DeleteScheduleItemAction(ScheduleItem game)
        {
            if (game == null) return;
            var confirmation = MessageBox.Show(Resources.DeleteScheduleItemMessage, Resources.ConfirmTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes) return;

            using (_db = new LonestarDbContext())
            {
                var scheduleItem = _db.Schedule.Find(game.Id);
                if (scheduleItem == null) return;

                _db.Schedule.Remove(scheduleItem);
                _db.SaveChanges();

                LoadSchedule();
            }
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
                LoadPlayers();
                LoadManagers();
                LoadTeams();
                LoadSchedule();
            }
        }

        /// <summary>
        /// </summary>
        public void RefreshManagersAction()
        {
            using (_db = new LonestarDbContext())
            {
                LoadManagers();
            }
        }

        /// <summary>
        /// </summary>
        public void RefreshTeamsAction()
        {
            using (_db = new LonestarDbContext())
            {
                LoadTeams();
            }
        }

        /// <summary>
        /// </summary>
        public void RefreshScheduleAction()
        {
            using (_db = new LonestarDbContext())
            {
                LoadSchedule();
            }
        }

        /// <summary>
        ///     Loads the team players.
        /// </summary>
        private void LoadPlayers()
        {
            // Gets the list of all the player.
            _myFullPlayerList = (from a in _db.Personnels
                join b in _db.Positions on a.Position equals b.PositionID
                join e in _db.PlayerStats on a.StatID equals e.StatID
                where a.PermissionLevel == 1
                select new {a.PID, a.FirstName, a.LastName, a.DOB, a.Address, a.City, a.Phone, a.Email, a.StatID, b, e})
                .Select(x => new Player
                {
                    PlayerId = x.PID,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    DateOfBirth = x.DOB,
                    Address = x.Address,
                    City = x.City,
                    PhoneNumber = x.Phone,
                    Position = x.b.Description,
                    GamesPlayed = x.e.GamesPlayed,
                    GamesStarted = x.e.GamesStarted,
                    Goals = x.e.Goals,
                    Assists = x.e.Assists,
                    YellowCards = x.e.YellowCards,
                    RedCards = x.e.RedCards,
                    Saves = (int) x.e.Saves,
                    GoalsAllowed = (int) x.e.GoalsAllowed,
                    StatId = (int) x.StatID
                }).ToList();

            // Forces the view to update all of the bindings.
            NotifyOfPropertyChange(null);
        }

        private void LoadManagers()
        {
            // Gets the list of all the managers.
            MyManagersList = (from a in _db.Personnels
                where a.PermissionLevel == 2
                select new {a.PID, a.FirstName, a.LastName, a.DOB, a.Address, a.City, a.Phone, a.Email})
                .Select(x => new Player
                {
                    PlayerId = x.PID,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    DateOfBirth = x.DOB,
                    Address = x.Address,
                    City = x.City,
                    PhoneNumber = x.Phone
                }).ToList();
        }

        private void LoadTeams()
        {
            MyTeamsList = (from a in _db.Teams
                join b in _db.Personnels on a.TeamID equals b.TeamID
                where a.TeamID != 1 && b.PermissionLevel == 2
                select new {a, b.FirstName, b.LastName})
                .Select(x => new TeamT
                {
                    City = x.a.City,
                    TeamName = x.a.TeamName,
                    TeamLogo = x.a.TeamLogo,
                    TeamId = x.a.TeamID,
                    PrimaryColor = x.a.PrimaryColor,
                    SecondaryColor = x.a.SecondaryColor,
                    IsSuspended = x.a.IsSuspended,
                    IsApproved = x.a.IsApproved,
                    Manager = x.FirstName + " " + x.LastName
                }
                ).ToList();

            // Loads the list of available teams into the dictionary.
            MyTeamDictionary =
                _myTeamsList.Where(t => t.IsApproved == true && t.TeamId != 1)
                    .Select(t => new {t.TeamName, t.TeamId})
                    .ToDictionary(t => t.TeamName, t => t.TeamId);
        }

        private void LoadSchedule()
        {
            // Loads the league schedule.
            LeagueSchedule = (from a in _db.Schedule
                join b in _db.Teams on a.HomeTeamID equals b.TeamID
                join c in _db.Teams on a.AwayTeamID equals c.TeamID
                select new ScheduleItem
                {
                    Date = (DateTime) a.Date,
                    HomeTeamId = a.HomeTeamID,
                    HomeTeamName = b.TeamName,
                    HomeTeamGoals = a.HomeTeamGoals,
                    AwayTeamGoals = a.AwayTeamGoals,
                    AwayTeamName = c.TeamName,
                    AwayTeamId = a.AwayTeamID,
                    Id = a.ID
                }
                ).ToList<ScheduleItem>();
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

        #endregion
    }
}