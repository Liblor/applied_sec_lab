using System;
using System.Linq;
using System.Security.Cryptography;

namespace CertServer.Models
{
    public static class CryptographyExtension
    {
        private static string FormatB64(string b64str)
        {
            int lineLength = 64;
            // Round up
            int partsNr = (b64str.Length + lineLength - 1)/ lineLength;
            return string.Join(
                "\n",
                Enumerable.Range(0, partsNr).Select(
                    // "Safe" substring where end index can be out of bound
                    i => new string(
                        b64str.Skip(i * lineLength).Take(lineLength).ToArray()
                    )
                )
            );
        }

        // There is no internal support for PEM format
        public static string ToPem(this RSA privateKey)
        {
            return "-----BEGIN RSA PRIVATE KEY-----\n" +
                FormatB64(
                    Convert.ToBase64String(
                        privateKey.ExportRSAPrivateKey()
                    )
                 ) +
                "\n-----END RSA PRIVATE KEY-----\n";
        }

        // There is no internal support for PEM format
        public static string ToPem(this ECDsa privateKey)
        {
            return "-----BEGIN EC PRIVATE KEY-----\n" +
                FormatB64(
                    Convert.ToBase64String(
                        privateKey.ExportECPrivateKey()
                    )
                ) +
                "\n-----END EC PRIVATE KEY-----\n";
        }
    }
}