using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace NanaManagerAPI.IO
{
    /// <summary>
    /// A helper class for decoding standard data from a string. Encode with <see cref="DataEncoder"/>
    /// </summary>
    public class DataDecoder
    {
        /// <summary>
        /// The data currently loaded inside the decoder
        /// </summary>
        public readonly string data;

        /// <summary>
        /// The current read location of the decoder
        /// </summary>
        public long Location { get; private set; }
        /// <summary>
        /// The length of the loaded data in bytes
        /// </summary>
        public long Length { get; }
        /// <summary>
        /// Whether or not the read location has reached the end of the data
        /// </summary>
        public bool EndReached => Location >= Length;

        /// <summary>
        /// Constructor for the <see cref="DataDecoder"/> class.
        /// </summary>
        /// <param name="ToRead">The data to be read and decoded, as a string</param>
        public DataDecoder( string ToRead ) {
            data = ToRead;
            Length = ToRead.Length;
            Location = 0;
        }

        /// <summary>
        /// Constructor for the <see cref="DataDecoder"/> class.
        /// </summary>
        /// <param name="ToRead">The data to be read and decoded, as a byte array</param>
        public DataDecoder( byte[] ToRead ) {
            data = Encoding.UTF8.GetString( ToRead );
            Length = ToRead.Length;
            Location = 0;
        }

        /// <summary>
        /// Reads a <see cref="char"/> from the data stream. Moves along the reader head by 1 byte
        /// </summary>
        public char ReadChar() => data[Location++];
        /// <summary>
        /// Reads a <see cref="byte"/> from the data stream. Moves along the reader head by 1 byte
        /// </summary>
        public byte ReadByte() => (byte)data[Location++];
        /// <summary>
        /// Reads a <see cref="sbyte"/> from the data stream. Moves along the reader head by 1 byte
        /// </summary>
        public sbyte ReadSByte() => (sbyte)data[Location++];
        /// <summary>
        /// Reads a <see cref="short"/> from the data stream. Moves along the reader head by 2 bytes
        /// </summary>
        public short ReadInt16() => (short)(data[Location++] | (data[Location++] << 8));
        /// <summary>
        /// Reads a <see cref="ushort"/> from the data stream. Moves along the reader head by 2 bytes
        /// </summary>
        public ushort ReadUInt16() => (ushort)(data[Location++] | (data[Location++] << 8));
        /// <summary>
        /// Reads a <see cref="int"/> from the data stream. Moves along the reader head by 4 bytes
        /// </summary>
        public int ReadInt32() => (int)(uint)(data[Location++] | ((data[Location++]) << 8) | ((data[Location++]) << 16) | ((data[Location++]) << 24));
        /// <summary>
        /// Reads a <see cref="uint"/> from the data stream. Moves along the reader head by 4 bytes
        /// </summary>
        public uint ReadUInt32() => (uint)(data[Location++] | ((data[Location++]) << 8) | ((data[Location++]) << 16) | ((data[Location++]) << 24));
        /// <summary>
        /// Reads a <see cref="long"/> from the data stream. Moves along the reader head by 8 bytes
        /// </summary>
        public long ReadInt64() => data[Location++] | ((data[Location++]) << 8) | ((data[Location++]) << 16) | ((data[Location++]) << 24) | ((data[Location++]) << 32) | ((data[Location++]) << 40) | ((data[Location++]) << 48) | ((data[Location++]) << 56);
        /// <summary>
        /// Reads a <see cref="ulong"/> from the data stream. Moves along the reader head by 8 bytes
        /// </summary>
        public ulong ReadUInt64() => (ulong)(data[Location++] | ((data[Location++]) << 8) | ((data[Location++]) << 16) | ((data[Location++]) << 24) | ((data[Location++]) << 32) | ((data[Location++]) << 40) | ((data[Location++]) << 48) | ((data[Location++]) << 56));
        /// <summary>
        /// Reads a <see cref="bool"/> from the data stream. Moves along the reader head by 1 byte
        /// </summary>
        public bool ReadBoolean() => data[Location++] != 0;
        /// <summary>
        /// Reads a <see cref="float"/> from the data stream. Moves along the reader head by 4 bytes
        /// </summary>
        public float ReadSingle() => BitConverter.ToSingle( new byte[] { (byte)data[Location++], (byte)data[Location++], (byte)data[Location++], (byte)data[Location++] }, 0 );
        /// <summary>
        /// Reads a <see cref="double"/> from the data stream. Moves along the reader head by 8 bytes
        /// </summary>
        public double ReadDouble() => BitConverter.ToDouble( new byte[] { (byte)data[Location++], (byte)data[Location++], (byte)data[Location++], (byte)data[Location++], (byte)data[Location++], (byte)data[Location++], (byte)data[Location++], (byte)data[Location++] }, 0 );
        /// <summary>
        /// Reads a <see cref="decimal"/> from the data stream. Moves along the reader head by 16 bytes
        /// </summary>
        public decimal ReadDecimal() {
            int[] data = new int[] { ReadInt32(), ReadInt32(), ReadInt32(), ReadInt32() };
            return new decimal( data[0], data[1], data[2], (data[3] & 0x80000000) != 0, (byte)((data[3] >> 16) & 0x7F) );
        }
        /// <summary>
        /// Reads a <see cref="string"/> from the data stream. Moves along the reader head by 4 bytes plus the length of the string
        /// </summary>
        public string ReadString() {
            int length = ReadInt32();
            int start = Location;
            Location += length;
            StringBuilder sb = new StringBuilder();
            for ( int i = start; i < (start + length); i++ )
                sb.Append( data[i] );

            return sb.ToString();
        }
        
        //TODO - Generalised ReadArray method, and input delegate for the read
        //TODO - Convert ReadString to a converted ReadCharArray
        
        /// <summary>
        /// Reads a <see cref="char"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="char"/> collection in bytes (1 byte per item)
        /// </summary>
        public IEnumerable<char> ReadCharArray() {
            bool isLong = ReadBoolean();
            List<char> builder = new List<char>();
            if ( isLong ) {
                long length = ReadInt64();
                for ( long i = 0; i < length; i++ )
                    builder.Add( ReadChar() );
            }
            else {
                int length = ReadInt32();
                for ( int i = 0; i < length; i++ )
                    builder.Add( ReadChar() );
            }

            return builder.AsEnumerable();
        }
        /// <summary>
        /// Reads a <see cref="byte"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="byte"/> collection in bytes (1 byte per item)
        /// </summary>
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
        /// <summary>
        /// Reads a <see cref="sbyte"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="sbyte"/> collection in bytes (1 byte per item)
        /// </summary>
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
        /// <summary>
        /// Reads a <see cref="short"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="short"/> collection in bytes (2 bytes per item)
        /// </summary>
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
        /// <summary>
        /// Reads a <see cref="ushort"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="ushort"/> collection in bytes (2 bytes per item)
        /// </summary>
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
        /// <summary>
        /// Reads a <see cref="int"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="int"/> collection in bytes (4 bytes per item)
        /// </summary>
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
        /// <summary>
        /// Reads a <see cref="uint"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="uint"/> collection in bytes (4 bytes per item)
        /// </summary>
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
        /// <summary>
        /// Reads a <see cref="long"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="long"/> collection in bytes (8 bytes per item)
        /// </summary>
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
        /// <summary>
        /// Reads a <see cref="ulong"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="ulong"/> collection in bytes (8 bytes per item)
        /// </summary>
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
        /// <summary>
        /// Reads a <see cref="bool"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="bool"/> collection in bytes (1 byte per item)
        /// </summary>
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
        /// <summary>
        /// Reads a <see cref="float"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="float"/> collection in bytes (4 bytes per item)
        /// </summary>
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
        /// <summary>
        /// Reads a <see cref="double"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="double"/> collection in bytes (8 bytes per item)
        /// </summary>
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
        /// <summary>
        /// Reads a <see cref="decimal"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="decimal"/> collection in bytes (16 bytes per item)
        /// </summary>
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
        /// <summary>
        /// Reads a <see cref="string"/> collection from the data stream. Reads a boolean followed by a <see cref="int"/> or a <see cref="long"/> plus the length of the <see cref="string"/> collection in bytes (4 bytes + string length per item)
        /// </summary>
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
