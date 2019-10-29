using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CertServer.Models
{
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("uid")]
        public string Uid { get; set; }

        [StringLength(64, MinimumLength = 1)]
        [Required]
        [Column("firstname")]
        public string FirstName { get; set; }

        [StringLength(64, MinimumLength = 1)]
        [Required]
        [Column("lastname")]
        public string LastName { get; set; }

        [StringLength(64, MinimumLength = 1)]
        [Required, EmailAddress]
        [Column("email")]
        public string Email { get; set; }

        [StringLength(64, MinimumLength = 1)]
        [Required]
        [Column("pwd")]
        public string PasswordHash { get; set; }
    }
}