using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace CoreCA.DataModel
{
    public enum EncryptionAlgorithms {RSA, ECDSA};

    public class CipherSuite
    {
        [Required]
        public EncryptionAlgorithms Alg { get; set; }
        [Required]
        public string HashAlg { get; set; }

        public int KeySize { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj is CipherSuite)
            {
                CipherSuite cipherSuite = (CipherSuite) obj;

                return cipherSuite != null
                    && cipherSuite.Alg.Equals(Alg)
                    && cipherSuite.HashAlg.Equals(HashAlg)
                    && cipherSuite.KeySize == KeySize;
            }
            return (obj is CipherSuite);
        }

        public override string ToString()
        {
            return string.Format(
                "{{\n\tEncryption algorithm: {0}\n\tHash algorithm: {1}\n\tKey size: {2}\n}}",
                Alg, HashAlg, KeySize
            );
        }

        public override int GetHashCode()
        {
            string cipherSuiteName = string.Format("{0}-{1}-{2}", Alg, HashAlg, KeySize);
            return cipherSuiteName.GetHashCode();
        }

    }
}
