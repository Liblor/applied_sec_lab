using CoreCA.DataModel;
using System.Linq;

namespace CertServer.Models
{
    public static class CipherSuiteExtensions
    {
        public static bool IsVaildCipherSuite(this CipherSuite cipherSuite) =>
            CAConfig.CipherSuites.Any(elem => elem.Equals(cipherSuite));
    }
}
