using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreCA.DataModel
{
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column("uid")]
        [Required, StringLength(64, MinimumLength = 1)]
        public string Id { get; set; }

        [Column("firstname")]
        [Required, StringLength(64, MinimumLength = 1)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Column("lastname")]
        [Required, StringLength(64, MinimumLength = 1)]
        public string LastName { get; set; }

        [Column("email")]
        [Required, StringLength(64, MinimumLength = 1)]
        [Display(Name = "Email Address"), EmailAddress]
        public string Email { get; set; }

        [Column("pwd")]
        [Required, StringLength(64, MinimumLength = 1)]
        public string PasswordHash { get; set; }
    }
}
