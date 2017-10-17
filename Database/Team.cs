using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LonestarShowdown.Database
{
    [Table("Team")]
    public partial class Team
    {
        public Team()
        {
            Personnels = new HashSet<Personnel>();
            Personnels1 = new HashSet<Personnel>();
        }

        public int TeamID { get; set; }

        [Required]
        [StringLength(55)]
        public string City { get; set; }

        [Required]
        [StringLength(30)]
        public string TeamName { get; set; }

        public byte[] TeamLogo { get; set; }

        [Required]
        [StringLength(10)]
        public string PrimaryColor { get; set; }

        [Required]
        [StringLength(10)]
        public string SecondaryColor { get; set; }

        public bool? IsApproved { get; set; }

        [Required]
        public bool IsSuspended { get; set; }

        public virtual ICollection<Personnel> Personnels { get; set; }

        public virtual ICollection<Personnel> Personnels1 { get; set; }
    }
}
