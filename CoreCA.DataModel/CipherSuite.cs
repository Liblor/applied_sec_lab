using System;
using System.ComponentModel.DataAnnotations;

namespace CoreCA.DataModel
{
	public class CipherSuite
	{
        // TODO: consider replacing string Alg/HashAlg fields with enums
		[Required]
		public string Alg { get; set; }
		[Required]
		public string HashAlg { get; set; }

		public int KeySize { get; set; }

		public override bool Equals(Object obj)
		{
			if (obj is CipherSuite) 
			{
				CipherSuite cipherSuite = (CipherSuite) obj;

				return cipherSuite.Alg.Equals(Alg)
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
			return (Alg + HashAlg + KeySize).GetHashCode();
		}
 
	}
}
