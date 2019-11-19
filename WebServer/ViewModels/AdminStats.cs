namespace WebServer.ViewModels
{
    public class AdminStats
    {
        public int NumIssuedCertificates { get; set; }
        public int NumRevokedCertificates { get; set; }
        public ulong CurrentSerialNum { get; set; }
    }
}
