using System;
using System.Text;

namespace NanaManagerAPI.IO
{
	/// <summary>
	/// A helper class for dencoding standard data from a string. Encode with <see cref="DataEncoder"/>
	/// </summary>
	public class DataDecoder
	{
		public readonly string data;

		public int Location { get; private set; }
		public int Length { get; }
		public bool EndReached => Location >= Length;

		public DataDecoder( string ToRead ) {
			data = ToRead;
			Length = ToRead.Length;
			Location = 0;
		}

		public DataDecoder( byte[] ToRead ) {
			data = Encoding.UTF8.GetString(ToRead);
			Length = ToRead.Length;
			Location = 0;
		}

		public char ReadChar() => data[Location++];
		public byte ReadByte() => (byte)data[Location++];
		public sbyte ReadSByte() => (sbyte)data[Location++];
		public short ReadInt16() => (short)(data[Location++] | (data[Location++] << 8));
		public ushort ReadUInt16() => (ushort)(data[Location++] | (data[Location++] << 8));
		public int ReadInt32() => (int)(uint)(data[Location++] | ((data[Location++]) << 8) | ((data[Location++]) << 16) | ((data[Location++]) << 24));
		public uint ReadUInt32() => (uint)(data[Location++] | ((data[Location++]) << 8) | ((data[Location++]) << 16) | ((data[Location++]) << 24));
		public long ReadInt64() => data[Location++] | ((data[Location++]) << 8) | ((data[Location++]) << 16) | ((data[Location++]) << 24) | ((data[Location++]) << 32) | ((data[Location++]) << 40) | ((data[Location++]) << 48) | ((data[Location++]) << 56);
		public ulong ReadUInt64() => (ulong)(data[Location++] | ((data[Location++]) << 8) | ((data[Location++]) << 16) | ((data[Location++]) << 24) | ((data[Location++]) << 32) | ((data[Location++]) << 40) | ((data[Location++]) << 48) | ((data[Location++]) << 56));
		public bool ReadBoolean() => data[Location++] != '\0';
		public float ReadSingle() => BitConverter.ToSingle( new byte[] { (byte)data[Location++], (byte)data[Location++], (byte)data[Location++], (byte)data[Location++] }, 0 );
		public double ReadDouble() => BitConverter.ToDouble( new byte[] { (byte)data[Location++], (byte)data[Location++], (byte)data[Location++], (byte)data[Location++], (byte)data[Location++], (byte)data[Location++], (byte)data[Location++], (byte)data[Location++] }, 0 );
		public decimal ReadDecimal() {
			int[] data = new int[] { ReadInt32(), ReadInt32(), ReadInt32(), ReadInt32() };
			return new decimal(data[0], data[1], data[2], (data[3] & 0x80000000) != 0, (byte)((data[3] >> 16) & 0x7F));
		}
		public string ReadString() {
			int length = ReadInt32();
			int start = Location;
			Location += length;
			StringBuilder sb = new StringBuilder();
			for ( int i = start; i < (start + length); i++ )
				sb.Append( data[i] );

			return sb.ToString();
		}
	}
}
