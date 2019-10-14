using System.Collections.Generic;
using WebServer.Models;

namespace WebServer.ViewModels
{
    public class CertificatesData
    {
        public HashSet<Certificate> Valid { get; set; }
        public HashSet<Certificate> Revoked { get; set; }
        public HashSet<Certificate> Expired { get; set; }
    }
}
