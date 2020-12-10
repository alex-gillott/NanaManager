using NanaManagerAPI.IO.Cryptography;
using System.Text;

namespace NanaManager.Cryptography
{
    /// <summary>
    /// Providing basic encryption technologies for file storage
    /// </summary>
    public class Cryptography : ICryptographyProvider
    {
        public void Initialise( string Key ) { //Required for the Cryptography Provider
        }

        public void Terminate() { //Required for the Cryptography Provider
        }

        public byte[] Encrypt( byte[] Data ) {
            return Data; //Don't encrypt
        }

        public string Encrypt( string PlainText ) => Encoding.UTF8.GetString( Encrypt( Encoding.UTF8.GetBytes( PlainText ) ) ); //CEncrypt as a byte array, and return as a string

        public byte[] Decrypt( byte[] Data ) {
            return Data; //Nothing to decrypt
        }

        public string Decrypt( string Data ) => Encoding.UTF8.GetString( Decrypt( Encoding.UTF8.GetBytes( Data ) ) ); //Decrypt as a byte array, and return as a string
    }
}
