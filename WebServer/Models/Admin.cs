using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebServer.Models
{
    public class Admin
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column("uid")]
        [Required, StringLength(64, MinimumLength = 1)]
        public string Uid { get; set; }
    }
}
