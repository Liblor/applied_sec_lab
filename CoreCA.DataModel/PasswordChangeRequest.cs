using System.ComponentModel.DataAnnotations;
using CoreCA.DataModel;

namespace CoreCA.DataModel
{
    public class PasswordChangeRequest
    {
        [Required]
        public string Uid { get; set; }

        [Required]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(64, MinimumLength = Constants.MinPasswordLength)]
        public string NewPassword { get; set; }
    }
}
