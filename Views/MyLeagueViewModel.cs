using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using LonestarShowdown.Custom;
using LonestarShowdown.Database;
using LonestarShowdown.Properties;

namespace LonestarShowdown.Views
{
    internal class MyLeagueViewModel : Screen
    {
        private readonly int _topNumber = 15;
        private LonestarDbContext _db;
        private List<ScheduleItem> _leagueSchedule;
        private List<PlayerStatsData> _playerStatsList = new List<PlayerStatsData>();
        private int _selectedStat = -1;
        private string _statName;

        private List<string> _statsList = new List<string>
        {
            "Top Scorers",
            "Appearances",
            "Assists",
            "Red Cards",
            "Yellow Cards"
        };

        private List<Team> _teamList;
        private List<TeamT> _teamStandings = new List<TeamT>();

        public MyLeagueViewModel()
        {
            LoadData();
        }

        public string StatName
        {
            get { return _statName; }
            set
            {
                if (_statName == value)
                    return;

                _statName = value;
                NotifyOfPropertyChange(() => StatName);
            }
        }

        public List<TeamT> TeamStandings
        {
            get { return _teamStandings; }
            set
            {
                if (_teamStandings == value)
                    return;

                _teamStandings = value;
                NotifyOfPropertyChange(() => TeamStandings);
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

        public int SelectedStat
        {
            get { return _selectedStat; }
            set
            {
                if (_selectedStat == value)
                    return;

                _selectedStat = value;
                LoadStats(_selectedStat);
                NotifyOfPropertyChange(() => SelectedStat);
            }
        }

        public List<string> StatsList
        {
            get { return _statsList; }
            set
            {
                if (_statsList == value)
                    return;

                _statsList = value;
                NotifyOfPropertyChange(() => StatsList);
            }
        }

        public List<PlayerStatsData> PlayerStatsList
        {
            get { return _playerStatsList; }
            set
            {
                if (_playerStatsList == value)
                    return;

                _playerStatsList = value;
                NotifyOfPropertyChange(() => PlayerStatsList);
            }
        }

        /// <summary>
        ///     Loads all the necessary data into the view.
        /// </summary>
        private void LoadData()
        {
            using (_db = new LonestarDbContext())
            {
                // Loads the list of all the teams.
                _teamList = (from t in _db.Teams
                    where t.TeamID != 1
                    select t).Where(t => t.IsApproved == true).ToList();

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
                        AwayTeamId = a.AwayTeamID
                    }
                    ).ToList<ScheduleItem>();

                // Converts the schedule into the team standings by calculating each team's stats.
                foreach (var item in _teamList)
                {
                    _teamStandings.Add(TeamStatsGenerator.GenerateTeamStats(item, LeagueSchedule));
                }

                // Sort the team standings by
                // 1. Total Points
                // 2. Goal Difference
                // 3. Goals For
                _teamStandings = _teamStandings
                    .OrderByDescending(p => p.PointsTotal)
                    .ThenByDescending(p => p.GoalDifference)
                    .ThenByDescending(p => p.GoalsFor)
                    .ToList();

                // Assigns the position of each team after sorting.
                for (var i = 0; i < _teamStandings.Count; i++)
                {
                    _teamStandings.ElementAt(i).Position = i + 1;
                }
            }
        }

        protected override void OnViewLoaded(object view)
        {
            SelectedStat = 0;
            base.OnViewLoaded(view);
        }

        /// <summary>
        ///     Loads the top player stats.
        /// </summary>
        private void LoadStats(int statNumber)
        {
            if (_selectedStat == -1) return;

            PlayerStatsList.Clear();
            StatName = StatsList.ElementAt(_selectedStat);
            using (_db = new LonestarDbContext())
            {
                switch (statNumber)
                {
                    case 0:
                        PlayerStatsList = (from a in _db.PlayerStats
                            join b in _db.Personnels on a.StatID equals b.StatID
                            select new
                            {
                                b.Picture,
                                b.FirstName,
                                b.LastName,
                                b.TeamID,
                                a.Goals
                            }).OrderByDescending(x => x.Goals).ThenBy(x => x.LastName).Take(_topNumber).AsEnumerable()
                            .Select(x =>
                            {
                                var team = _teamList.FirstOrDefault(t => t.TeamID == x.TeamID);
                                return team != null
                                    ? new PlayerStatsData
                                    {
                                        ImageData = x.Picture ?? Resources.BlankProfile,
                                        FirstName = x.FirstName,
                                        LastName = x.LastName,
                                        StatData = x.Goals,
                                        TeamLogo = team.TeamLogo,
                                        TeamName = team.TeamName
                                    }
                                    : null;
                            }).ToList();
                        break;
                    case 1:
                        PlayerStatsList = (from a in _db.PlayerStats
                            join b in _db.Personnels on a.StatID equals b.StatID
                            select new
                            {
                                b.Picture,
                                b.FirstName,
                                b.LastName,
                                b.TeamID,
                                a.GamesPlayed
                            }).OrderByDescending(x => x.GamesPlayed)
                            .ThenBy(x => x.LastName)
                            .Take(_topNumber)
                            .AsEnumerable()
                            .Select(x =>
                            {
                                var team = _teamList.FirstOrDefault(t => t.TeamID == x.TeamID);
                                return team != null
                                    ? new PlayerStatsData
                                    {
                                        ImageData = x.Picture ?? Resources.BlankProfile,
                                        FirstName = x.FirstName,
                                        LastName = x.LastName,
                                        StatData = x.GamesPlayed,
                                        TeamLogo = team.TeamLogo,
                                        TeamName = team.TeamName
                                    }
                                    : null;
                            }).ToList();
                        break;
                    case 2:
                        PlayerStatsList = (from a in _db.PlayerStats
                            join b in _db.Personnels on a.StatID equals b.StatID
                            select new
                            {
                                b.Picture,
                                b.FirstName,
                                b.LastName,
                                b.TeamID,
                                a.Assists
                            }).OrderByDescending(x => x.Assists).ThenBy(x => x.LastName).Take(_topNumber).AsEnumerable()
                            .Select(x =>
                            {
                                var team = _teamList.FirstOrDefault(t => t.TeamID == x.TeamID);
                                return team != null
                                    ? new PlayerStatsData
                                    {
                                        ImageData = x.Picture ?? Resources.BlankProfile,
                                        FirstName = x.FirstName,
                                        LastName = x.LastName,
                                        StatData = x.Assists,
                                        TeamLogo = team.TeamLogo,
                                        TeamName = team.TeamName
                                    }
                                    : null;
                            }).ToList();
                        break;
                    case 3:
                        PlayerStatsList = (from a in _db.PlayerStats
                            join b in _db.Personnels on a.StatID equals b.StatID
                            select new
                            {
                                b.Picture,
                                b.FirstName,
                                b.LastName,
                                b.TeamID,
                                a.RedCards
                            }).OrderByDescending(x => x.RedCards).ThenBy(x => x.LastName).Take(_topNumber).AsEnumerable()
                            .Select(x =>
                            {
                                var team = _teamList.FirstOrDefault(t => t.TeamID == x.TeamID);
                                return team != null
                                    ? new PlayerStatsData
                                    {
                                        ImageData = x.Picture ?? Resources.BlankProfile,
                                        FirstName = x.FirstName,
                                        LastName = x.LastName,
                                        StatData = x.RedCards,
                                        TeamLogo = team.TeamLogo,
                                        TeamName = team.TeamName
                                    }
                                    : null;
                            }).ToList();
                        break;
                    case 4:
                        PlayerStatsList = (from a in _db.PlayerStats
                            join b in _db.Personnels on a.StatID equals b.StatID
                            select new
                            {
                                b.Picture,
                                b.FirstName,
                                b.LastName,
                                b.TeamID,
                                a.YellowCards
                            }).OrderByDescending(x => x.YellowCards)
                            .ThenBy(x => x.LastName)
                            .Take(_topNumber)
                            .AsEnumerable()
                            .Select(x =>
                            {
                                var team = _teamList.FirstOrDefault(t => t.TeamID == x.TeamID);
                                return team != null
                                    ? new PlayerStatsData
                                    {
                                        ImageData = x.Picture ?? Resources.BlankProfile,
                                        FirstName = x.FirstName,
                                        LastName = x.LastName,
                                        StatData = x.YellowCards,
                                        TeamLogo = team.TeamLogo,
                                        TeamName = team.TeamName
                                    }
                                    : null;
                            }).ToList();
                        break;
                }

                if (PlayerStatsList.Count > 0)
                {
                    var rank = 1;
                    var previousStat = PlayerStatsList.ElementAt(0).StatData;
                    foreach (var item in PlayerStatsList)
                    {
                        if (item.StatData < previousStat)
                        {
                            previousStat = item.StatData;
                            rank += 1;
                        }
                        item.Rank = rank;
                    }
                }
            }
            NotifyOfPropertyChange(null);
        }
    }
}