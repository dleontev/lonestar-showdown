using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LonestarShowdown.Database
{
    public partial class SecurityQuestion
    {
        public SecurityQuestion()
        {
            Personnels = new HashSet<Personnel>();
        }

        public int SecurityQuestionID { get; set; }

        [StringLength(255)]
        public string SecurityQ { get; set; }

        public virtual ICollection<Personnel> Personnels { get; set; }
    }
}
