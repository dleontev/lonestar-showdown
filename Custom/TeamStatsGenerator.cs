using System.Collections.Generic;
using System.Linq;
using LonestarShowdown.Database;

namespace LonestarShowdown.Custom
{
    internal class TeamStatsGenerator
    {
        public static TeamT GenerateTeamStats(Team team, List<ScheduleItem> leagueSchedule)
        {
            var newTeamT = new TeamT {TeamName = team.TeamName, TeamLogo = team.TeamLogo};

            var query = (from s in leagueSchedule
                where (s.AwayTeamId == team.TeamID || s.HomeTeamId == team.TeamID)
                select s);

            var scheduleItems = query as IList<ScheduleItem> ?? query.ToList();
            foreach (var item in scheduleItems)
            {
                if (item.HomeTeamId == team.TeamID)
                {
                    item.HomeTeamLogo = team.TeamLogo;
                }
                else
                {
                    item.AwayTeamLogo = team.TeamLogo;
                }
            }

            var gamesPlayed =
                (from s in scheduleItems
                    where (s.AwayTeamId == team.TeamID || s.HomeTeamId == team.TeamID) &&
                          (s.HomeTeamGoals != null && s.AwayTeamGoals != null)
                    select s).Count();

            var gamesWon =
                (from s in scheduleItems
                    where (s.AwayTeamId == team.TeamID && s.AwayTeamGoals > s.HomeTeamGoals) ||
                          (s.HomeTeamId == team.TeamID && s.AwayTeamGoals < s.HomeTeamGoals) &&
                          (s.HomeTeamGoals != null && s.AwayTeamGoals != null)
                    select s).Count();

            var gamesDrawn =
                (from s in scheduleItems
                    where
                        (s.AwayTeamId == team.TeamID || s.HomeTeamId == team.TeamID) &&
                        (s.AwayTeamGoals == s.HomeTeamGoals)
                        && (s.HomeTeamGoals != null && s.AwayTeamGoals != null)
                    select s).Distinct().Count();

            var goalsFor =
                (from s in scheduleItems
                    where s.HomeTeamGoals != null && s.HomeTeamId == team.TeamID
                    select s.HomeTeamGoals).Sum() + (from s in leagueSchedule
                        where s.AwayTeamGoals != null && s.AwayTeamId == team.TeamID
                        select s.AwayTeamGoals).Sum();

            var goalsAgainst =
                (from s in scheduleItems
                    where s.HomeTeamGoals != null && s.AwayTeamId == team.TeamID
                    select s.HomeTeamGoals).Sum() + (from s in leagueSchedule
                        where s.AwayTeamGoals != null && s.HomeTeamId == team.TeamID
                        select s.AwayTeamGoals).Sum();


            newTeamT.GamesPlayed = gamesPlayed;
            newTeamT.GamesWon = gamesWon;
            newTeamT.GamesDrawn = gamesDrawn;
            newTeamT.GamesLost = gamesPlayed - (gamesWon + gamesDrawn);
            if (goalsFor != null) newTeamT.GoalsFor = (int) goalsFor;
            if (goalsAgainst != null) newTeamT.GoalsAgainst = (int) goalsAgainst;
            newTeamT.GoalDifference = newTeamT.GoalsFor - newTeamT.GoalsAgainst;
            newTeamT.PointsTotal = gamesWon*3 + gamesDrawn;


            return newTeamT;
        }
    }
}