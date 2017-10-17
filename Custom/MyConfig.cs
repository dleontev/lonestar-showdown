namespace LonestarShowdown.Custom
{
    /// <summary>
    /// </summary>
    public class MyConfig
    {
        private static readonly MyConfig _configuration = new MyConfig();
        private readonly int _maxFileSize;

        private MyConfig()
        {
            HasTeamChanged = false;
            _maxFileSize = 250000;
        }

        /// <summary>
        /// </summary>
        public static MyConfig Configuration
        {
            get { return _configuration; }
        }

        /// <summary>
        /// </summary>
        public int MaxFileSize
        {
            get { return _maxFileSize; }
        }

        /// <summary>
        /// </summary>
        public int? CurrentUserPid { get; set; }

        /// <summary>
        /// </summary>
        public int? CurrentUserTeamId { get; set; }

        /// <summary>
        /// </summary>
        public int? CurrentUserScreenId { get; set; }

        /// <summary>
        /// </summary>
        public bool HasTeamChanged { get; set; }

        /// <summary>
        /// </summary>
        public bool HasPlayerListChanged { get; set; }
    }
}