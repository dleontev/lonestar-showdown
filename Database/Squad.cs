using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LonestarShowdown.Database
{
    [Table("Squad")]
    public partial class Squad
    {
        [Key]
        public int LineUpID { get; set; }

        [Required]
        [StringLength(7)]
        public string SquadPosition { get; set; }
    }
}
