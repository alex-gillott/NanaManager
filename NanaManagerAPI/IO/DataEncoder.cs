using System;
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
		public void Write( string Value ) {
			Write( Value.Length );
			sb.Append( Value );
		}
		public void Write( char Value ) => sb.Append( Value );
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
		/// <summary>
		/// Returns the data encoded as a string
		/// </summary>
		/// <returns>A string representing the encoded data</returns>
		public override string ToString() {
			return sb.ToString();
		}
	}
}
