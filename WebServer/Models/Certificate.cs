using System;

namespace WebServer.Models
{
    public class Certificate
    {
        public string Fingerprint { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}
