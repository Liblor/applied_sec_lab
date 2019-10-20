using CertServer.Models;

namespace CertServer
{
	public class CAConfig
	{
		public static readonly string APIBasePath = "api";
		public static readonly string APIName = "Core CA API";
		public static readonly string APIVersion = "v1";

		public static readonly string CrlDistributionPoint = CAConfig.APIBasePath + "/cdp";

		public static readonly string CoreCACertPath = "/home/coreca/keys/"
			+ System.Environment.MachineName
			+ "_immediate_ca_cert_and_priv_key.pfx";

		public static readonly int UserCertValidityPeriod = 150;

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
			// Different ECDSA curves are only supported on Windows.
			new CipherSuite {
				Alg = "ECDSA", 
				HashAlg = "SHA512", 
				KeySize = 521
			}
		};
	}
}
