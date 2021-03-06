using CoreCA.DataModel;
using System.Security.Cryptography;

namespace CertServer
{
    public class CAConfig
    {
        public static readonly string APIBasePath = "api";
        public static readonly string APIName = "Core CA API";
        public static readonly string APIVersion = "v1";

        public static readonly string CrlDistributionPoint = "http://imovies.ch/crl/revoked.crl";

        public static readonly string CoreCACertPath = "/home/coreca/pki/private/iMovies_external_"
            + System.Environment.MachineName
            + "_Intermediate_CA_cert_and_priv_key.pfx";
        public static readonly string BackupServerCertPath = "/etc/pki/bkp/certs/iMovies_bkp.crt";
        public static readonly string BackupFolder = "/home/coreca/backup/";
        public static readonly string BackupScript = "/etc/borgbackup/run.sh";

        public static readonly int UserCertValidityPeriod = 150;
        public static readonly double SerialNumberWarningThreshold = 0.9;

        public static readonly string PasswordListPath = "/usr/share/wordlists/rockyou.txt";

        // Order by decreasing preference, the first option is the default cipher suite.
        public static readonly CipherSuite[] CipherSuites =
        {
            // Different ECDSA curves are only supported on Windows.
            new CipherSuite {
                Alg = EncryptionAlgorithms.ECDSA,
                HashAlg = HashAlgorithmName.SHA512.Name,
                KeySize = 521
            },
            new CipherSuite {
                Alg = EncryptionAlgorithms.RSA,
                HashAlg = HashAlgorithmName.SHA512.Name,
                KeySize = 4096
            },
            new CipherSuite {
                Alg = EncryptionAlgorithms.RSA,
                HashAlg = HashAlgorithmName.SHA512.Name,
                KeySize = 2048
            }
        };

        public static readonly string IMoviesUserTableName = "users";
        public static readonly string IMoviesCAPublicCertsTableName = "public_certificates";
        public static readonly string IMoviesCAPrivateCertsTableName = "private_certificates";
    }
}
