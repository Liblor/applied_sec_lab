using System.ComponentModel.DataAnnotations;

namespace CoreCA.DataModel
{
    public class DownloadPrivateKeyRequest
    {
        [Required, StringLength(64, MinimumLength = 1)]
        public string Uid { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
