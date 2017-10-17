namespace LonestarShowdown.Custom
{
    /// <summary>
    /// </summary>
    public class PlayerStatsData
    {
        /// <summary>
        /// </summary>
        public byte[] ImageData { get; set; }

        /// <summary>
        /// </summary>
        public string Name
        {
            get { return FirstName + " " + LastName; }
        }

        /// <summary>
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// </summary>
        public int StatData { get; set; }

        /// <summary>
        /// </summary>
        public byte[] TeamLogo { get; set; }

        /// <summary>
        /// </summary>
        public string TeamName { get; set; }
    }
}