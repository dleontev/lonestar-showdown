using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LonestarShowdown.Database
{
    public partial class PlayerStat
    {
        public PlayerStat()
        {
            Personnels = new HashSet<Personnel>();
        }

        [Key]
        public int StatID { get; set; }

        public int GamesPlayed { get; set; }

        public int GamesStarted { get; set; }

        public int Goals { get; set; }

        public int Assists { get; set; }

        public int YellowCards { get; set; }

        public int RedCards { get; set; }

        public int? Saves { get; set; }

        public int? GoalsAllowed { get; set; }

        public virtual ICollection<Personnel> Personnels { get; set; }
    }
}
