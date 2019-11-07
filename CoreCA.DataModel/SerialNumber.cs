using System;

namespace CoreCA.DataModel
{
	public class SerialNumber
	{
		public ulong SerialNr { get; set; }

		// Convert serial number to its big endian representation
		public byte[] SerialNrBytes  {
			get
			{
				byte[] serialNrBytes = BitConverter.GetBytes(SerialNr);
				Array.Reverse(serialNrBytes);
				return serialNrBytes;
			}
		}
	}
}
