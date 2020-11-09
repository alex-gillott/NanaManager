using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
		public bool ReadBoolean() => data[Location++] != 0;
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
		
		public IEnumerable<char> ReadCharArray() {
			bool isLong = ReadBoolean();
			List<char> builder = new List<char>();
			if (isLong) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadChar() );
            } else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadChar() );
            }

			return builder.AsEnumerable();
        }

		public IEnumerable<byte> ReadByteArray() {
			bool isLong = ReadBoolean();
			List<byte> builder = new List<byte>();
			if ( isLong ) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadByte() );
			}
			else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadByte() );
			}

			return builder.AsEnumerable();
		}
		public IEnumerable<sbyte> ReadSByteArray() {
			bool isLong = ReadBoolean();
			List<sbyte> builder = new List<sbyte>();
			if ( isLong ) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadSByte() );
			}
			else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadSByte() );
			}

			return builder.AsEnumerable();
		}

		public IEnumerable<short> ReadInt16Array() {
			bool isLong = ReadBoolean();
			List<short> builder = new List<short>();
			if ( isLong ) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadInt16() );
			}
			else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadInt16() );
			}

			return builder.AsEnumerable();
		}
		public IEnumerable<ushort> ReadUInt16Array() {
			bool isLong = ReadBoolean();
			List<ushort> builder = new List<ushort>();
			if ( isLong ) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadUInt16() );
			}
			else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadUInt16() );
			}

			return builder.AsEnumerable();
		}

		public IEnumerable<int> ReadInt32Array() {
			bool isLong = ReadBoolean();
			List<int> builder = new List<int>();
			if ( isLong ) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadInt32() );
			}
			else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadInt32() );
			}

			return builder.AsEnumerable();
		}
		public IEnumerable<uint> ReadUInt32Array() {
			bool isLong = ReadBoolean();
			List<uint> builder = new List<uint>();
			if ( isLong ) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadUInt32() );
			}
			else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadUInt32() );
			}

			return builder.AsEnumerable();
		}

		public IEnumerable<long> ReadInt64Array() {
			bool isLong = ReadBoolean();
			List<long> builder = new List<long>();
			if ( isLong ) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadInt64() );
			}
			else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadInt64() );
			}

			return builder.AsEnumerable();
		}
		public IEnumerable<ulong> ReadUInt64Array() {
			bool isLong = ReadBoolean();
			List<ulong> builder = new List<ulong>();
			if ( isLong ) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadUInt64() );
			}
			else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadUInt64() );
			}

			return builder.AsEnumerable();
		}

		public IEnumerable<bool> ReadBooleanArray() {
			bool isLong = ReadBoolean();
			List<bool> builder = new List<bool>();
			if ( isLong ) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadBoolean() );
			}
			else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadBoolean() );
			}

			return builder.AsEnumerable();
		}

		public IEnumerable<float> ReadSingleArray() {
			bool isLong = ReadBoolean();
			List<float> builder = new List<float>();
			if ( isLong ) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadSingle() );
			}
			else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadSingle() );
			}

			return builder.AsEnumerable();
		}
		public IEnumerable<double> ReadDoubleArray() {
			bool isLong = ReadBoolean();
			List<double> builder = new List<double>();
			if ( isLong ) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadDouble() );
			}
			else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadDouble() );
			}

			return builder.AsEnumerable();
		}
		public IEnumerable<decimal> ReadDecimalArray() {
			bool isLong = ReadBoolean();
			List<decimal> builder = new List<decimal>();
			if ( isLong ) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadDecimal() );
			}
			else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadDecimal() );
			}

			return builder.AsEnumerable();
		}

		public IEnumerable<string> ReadStringArray() {
			bool isLong = ReadBoolean();
			StringCollection builder = new StringCollection();
			if ( isLong ) {
				long length = ReadInt64();
				for ( long i = 0; i < length; i++ )
					builder.Add( ReadString() );
			}
			else {
				int length = ReadInt32();
				for ( int i = 0; i < length; i++ )
					builder.Add( ReadString() );
			}

			return (IEnumerable<string>)builder;
		}
	}
}
