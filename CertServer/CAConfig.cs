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
		public static readonly string CoreCAPrivKeyPath = "./KeyStore/core_ca_key_pkcs8.pem";

        public static readonly CipherSuite[] CipherSuites = 
        {
            new CipherSuite {
                Alg = "RSA", 
                HashAlg = "SHA512", 
                KeySize = 4096
            }
        };
    }
}