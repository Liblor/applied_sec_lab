using System.ComponentModel.DataAnnotations;

using CoreCA.DataModel;

namespace WebServer.Models.Account
{
    public class UpdatePasswordDetails
    {
        [Required, DataType(DataType.Password)]
        [Display(Name = "Old password")]
        public string OldPassword { get; set; }

        [Required, DataType(DataType.Password), StringLength(64, MinimumLength = Constants.MinPasswordLength)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }
    }
}
