using CertServer.Models;

namespace CertServer
{
    public class CAConfig
    {
		public static readonly string APIBasePath = "api";
		public static readonly string APIName = "Core CA API";
		public static readonly string APIVersion = "v1";

		public static readonly string CoreCACertPath = "./KeyStore/core_ca_pub_priv_keys.pfx";
        // XXX: Change password
        public static readonly string CoreCACertPW = "TEST";

        // XXX: @Loris, how long should the certificate be valid? (3rd parameter, 
        // currently randomly chosen 90 days)
        public static readonly int UserCertValidityPeriod = 90;

        public static readonly CipherSuite[] CipherSuites = 
        {
            new CipherSuite {
                Alg = "RSA", 
                HashAlg = "SHA512", 
                KeySize = 4096
            },
            new CipherSuite {
                Alg = "RSA", 
                HashAlg = "SHA512", 
                KeySize = 2048
            },
            new CipherSuite {
                Alg = "ECDSA", 
                HashAlg = "SHA512",
                KeySize = 384
            },
            new CipherSuite {
                Alg = "ECDSA", 
                HashAlg = "SHA512",
                KeySize = 521
            },
        };
    }
}