using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LonestarShowdown.Database
{
    [Table("Schedule")]
    public partial class Schedule
    {
        public int ID { get; set; }

        public int HomeTeamID { get; set; }

        public int AwayTeamID { get; set; }

        public int? HomeTeamGoals { get; set; }

        public int? AwayTeamGoals { get; set; }

        public DateTime? Date { get; set; }
    }
}
