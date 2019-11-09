using CoreCA.DataModel;

namespace CertServer
{
	public class CAConfig
	{
		public static readonly string APIBasePath = "api";
		public static readonly string APIName = "Core CA API";
		public static readonly string APIVersion = "v1";

		public static readonly string CrlDistributionPoint = "https://www.imovies.ch/crl/revoked.crl";

		public static readonly string CoreCACertPath = "/home/coreca/pki/private/iMovies_external_"
			+ System.Environment.MachineName
			+ "_Intermediate_CA_cert_and_priv_key.pfx";

		public static readonly int UserCertValidityPeriod = 150;
		public static readonly int CRLNextUpdatedIntervalMinutes = 10;
		public static readonly double SerialNumberWarningThreshold = 0.9;

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

		public static readonly string IMoviesUserTableName = "users";
		public static readonly string IMoviesCAPublicCertsTableName = "public_certificates";
		public static readonly string IMoviesCAPrivateCertsTableName = "private_certificates";
	}
}
