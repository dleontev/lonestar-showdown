using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LonestarShowdown.Database
{
    [Table("Position")]
    public partial class Position
    {
        public Position()
        {
            Personnels = new HashSet<Personnel>();
        }

        public int PositionID { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(255)]
        public string Abbreviation { get; set; }

        public virtual ICollection<Personnel> Personnels { get; set; }
    }
}
