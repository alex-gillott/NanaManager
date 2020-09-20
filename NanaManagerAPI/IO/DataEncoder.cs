using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanaManagerAPI
{
	/// <summary>
	/// A helper class for encoding standard data as a string. Decode with <see cref="DataDecoder"/>
	/// </summary>
	public class DataEncoder
	{
		private readonly StringBuilder sb = new StringBuilder();

		public void Write( byte Value ) => sb.Append( (char)Value );
		public void Write( sbyte Value ) => sb.Append( (char)Value );
		public void Write( short Value ) => sb.Append( string.Concat( (char)(255 & Value), (char)((65280 & Value) >> 8) ) );
		public void Write( ushort Value ) => sb.Append( string.Concat( (char)(255 & Value), (char)((65280 & Value) >> 8) ) );
		public void Write( int Value ) => sb.Append( string.Concat( (char)(255 & Value), (char)((65280 & Value) >> 8), (char)((16711680 & Value) >> 16), (char)((4278190080 & Value) >> 24) ) );
		public void Write( uint Value ) => sb.Append( string.Concat( (char)(255 & Value), (char)((65280 & Value) >> 8), (char)((16711680 & Value) >> 16), (char)((4278190080 & Value) >> 24) ) );
		public void Write( long Value ) => sb.Append( string.Concat( (char)(255 & Value), (char)((65280 & Value) >> 8), (char)((16711680 & Value) >> 16), (char)((4278190080 & Value) >> 24), (char)((1095216660480 & Value) >> 32), (char)((280375465082880 & Value) >> 40), (char)((71776119061217280 & Value) >> 48), (char)((-72057594037927936 & Value) >> 56) ) );
		public void Write( ulong Value ) => sb.Append( string.Concat( (char)(255 & Value), (char)((65280 & Value) >> 8), (char)((16711680 & Value) >> 16), (char)((4278190080 & Value) >> 24), (char)((1095216660480 & Value) >> 32), (char)((280375465082880 & Value) >> 40), (char)((71776119061217280 & Value) >> 48), (char)((18374686479671623680 & Value) >> 56) ) );
		public void Write( bool Value ) => sb.Append( Value ? (char)1 : (char)0 );
		public void Write( char Value ) => sb.Append( Value );
		public void Write( string Value ) {
			Write( Value.Length );
			sb.Append( Value );
		}
		public void Write( float Value ) {
			byte[] data = BitConverter.GetBytes( Value );
			sb.Append( string.Concat( (char)data[0], (char)data[1], (char)data[2], (char)data[3] ) );
		}
		public void Write( double Value ) {
			byte[] data = BitConverter.GetBytes( Value );
			sb.Append( string.Concat((char)data[0], (char)data[1], (char)data[2], (char)data[3], (char)data[4], (char)data[5], (char)data[6], (char)data[7] ) );
		}
		public void Write( decimal Value ) {
			int[] bits = decimal.GetBits( Value );
			foreach ( int i in bits )
				Write( i );
		}
		
		//Iteration City. Basically, I wanted to be able to make it easy for people to read and write arrays
		public void Write( IEnumerable<byte> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( byte b in Value )
				Write( b );
		}
		public void Write( IEnumerable<byte> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( byte b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( byte b in Value )
					Write( b );
			}
		}
		public void Write( IEnumerable<sbyte> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( sbyte b in Value )
				Write( b );
		}
		public void Write( IEnumerable<sbyte> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( sbyte b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( sbyte b in Value )
					Write( b );
			}
		}
		public void Write( IEnumerable<short> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( short b in Value )
				Write( b );
		}
		public void Write( IEnumerable<short> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( short b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( short b in Value )
					Write( b );
			}
		}
		public void Write( IEnumerable<ushort> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( ushort b in Value )
				Write( b );
		}
		public void Write( IEnumerable<ushort> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( ushort b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( ushort b in Value )
					Write( b );
			}
		}
		public void Write( IEnumerable<uint> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( uint b in Value )
				Write( b );
		}
		public void Write( IEnumerable<uint> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( uint b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( uint b in Value )
					Write( b );
			}
		}
		public void Write( IEnumerable<int> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( int b in Value )
				Write( b );
		}
		public void Write( IEnumerable<int> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( int b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( int b in Value )
					Write( b );
			}
		}
		public void Write( IEnumerable<ulong> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( ulong b in Value )
				Write( b );
		}
		public void Write( IEnumerable<ulong> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( ulong b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( ulong b in Value )
					Write( b );
			}
		}
		public void Write( IEnumerable<long> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( long b in Value )
				Write( b );
		}
		public void Write( IEnumerable<long> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( long b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( long b in Value )
					Write( b );
			}
		}
		public void Write( IEnumerable<bool> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( bool b in Value )
				Write( b );
		}
		public void Write( IEnumerable<bool> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( bool b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( bool b in Value )
					Write( b );
			}
		}
		public void Write( IEnumerable<string> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( string b in Value )
				Write( b );
		}
		public void Write( IEnumerable<string> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( string b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( string b in Value )
					Write( b );
			}
		}
		public void Write( IEnumerable<char> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( char b in Value )
				Write( b );
		}
		public void Write( IEnumerable<char> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( char b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( char b in Value )
					Write( b );
			}
		}
		public void Write( IEnumerable<float> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( float b in Value )
				Write( b );
		}
		public void Write( IEnumerable<float> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( float b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( float b in Value )
					Write( b );
			}
		}
		public void Write( IEnumerable<double> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( double b in Value )
				Write( b );
		}
		public void Write( IEnumerable<double> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( double b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( double b in Value )
					Write( b );
			}
		}
		public void Write( IEnumerable<decimal> Value ) {
			Write( false );
			Write( Value.Count() );
			foreach ( decimal b in Value )
				Write( b );
		}
		public void Write( IEnumerable<decimal> Value, bool IsLong ) {
			Write( IsLong );
			if ( IsLong ) {
				Write( Value.LongCount() );
				foreach ( decimal b in Value )
					Write( b );
			}
			else {
				Write( Value.Count() );
				foreach ( decimal b in Value )
					Write( b );
			}
		}
		/// <summary>
		/// Returns the data encoded as a string
		/// </summary>
		/// <returns>A string representing the encoded data</returns>
		public override string ToString() => sb.ToString();
		public byte[] ToByteArray() => Encoding.UTF8.GetBytes( sb.ToString() );

		public static explicit operator byte[](DataEncoder Encoder) => Encoder.ToByteArray();
		public static explicit operator string(DataEncoder Encoder) => Encoder.ToString();
	}
}
