namespace LonestarShowdown.Custom
{
    /// <summary>
    /// </summary>
    public class TeamT
    {
        public int Position { get; set; }
        public byte[] TeamLogo { get; set; }
        public string TeamName { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int GamesDrawn { get; set; }
        public int GamesLost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference { get; set; }
        public int PointsTotal { get; set; }
        public string Manager { get; set; }
        public string City { get; set; }
        public int TeamId { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
        public bool? IsApproved { get; set; }

        public bool? IsSuspended { get; set; }
    }
}