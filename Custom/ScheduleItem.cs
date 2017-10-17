using System;

namespace LonestarShowdown.Custom
{
    /// <summary>
    /// </summary>
    public class ScheduleItem
    {
        public DateTime Date { get; set; }
        public string Venue { get; set; }
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public int? HomeTeamGoals { get; set; }
        public int? AwayTeamGoals { get; set; }
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        public byte[] HomeTeamLogo { get; set; }
        public byte[] AwayTeamLogo { get; set; }
        public int Id { get; set; }

        public string Score
        {
            get { return string.Format("{0}-{1}", HomeTeamGoals, AwayTeamGoals); }
        }
    }
}