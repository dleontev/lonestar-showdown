using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LonestarShowdown.Database
{
    [Table("Personnel")]
    public partial class Personnel
    {
        [Key]
        public int PID { get; set; }

        [Required]
        [StringLength(35)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(35)]
        public string LastName { get; set; }

        [Column(TypeName = "date")]
        public DateTime DOB { get; set; }

        [Required]
        [StringLength(55)]
        public string Address { get; set; }

        [Required]
        [StringLength(55)]
        public string City { get; set; }

        [Required]
        [StringLength(12)]
        public string Phone { get; set; }

        public int? TeamID { get; set; }

        public int? TeamRequested { get; set; }

        public int? LineUpID { get; set; }

        public int? Position { get; set; }

        public int PermissionLevel { get; set; }

        public int? JerseyNumber { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        public int SecurityQuestionID { get; set; }

        public byte[] Picture { get; set; }

        [Required]
        [MaxLength(64)]
        public byte[] SecurityAnswer { get; set; }

        [Required]
        [MaxLength(64)]
        public byte[] Password { get; set; }

        [Required]
        [MaxLength(64)]
        public byte[] SaltData { get; set; }

        public DateTime PasswordChangeDate { get; set; }

        public DateTime SecurityQuestionChangeDate { get; set; }

        public int? StatID { get; set; }

        public virtual PlayerStat PlayerStat { get; set; }

        public virtual Position Position1 { get; set; }

        public virtual SecurityQuestion SecurityQuestion { get; set; }

        public virtual Team Team { get; set; }

        public virtual Team Team1 { get; set; }
    }
}
