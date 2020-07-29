using System.Text;

using NanaManagerAPI.IO.Cryptography;

namespace NanaManager.Cryptography
{
    /// <summary>
    /// Providing basic encryption technologies for file storage
    /// </summary>
    public class Cryptography : ICryptographyProvider
    {
        public void Initialise( string Key ) {
        }

        public void Terminate() {
        }

        public byte[] Encrypt( byte[] Data ) {
            return Data;
        }

        public string Encrypt( string PlainText ) => Encoding.UTF8.GetString( Encrypt( Encoding.UTF8.GetBytes( PlainText ) ) );

        public byte[] Decrypt( byte[] Data ) {
            return Data;
        }
        public string Decrypt( string Data ) => Encoding.UTF8.GetString( Decrypt( Encoding.UTF8.GetBytes( Data ) ) );
    }
}