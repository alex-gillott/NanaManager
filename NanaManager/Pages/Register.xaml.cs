using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Security.Cryptography;

using NanaManagerAPI.IO;
using NanaManagerAPI.UI;

namespace NanaManager
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Page
    {
        public Register() {
            InitializeComponent();
        }
        private void tbxUsername_KeyDown( object sender, KeyEventArgs e ) {
            if ( e.Key == Key.Enter )
                tbxPassword.Focus();
        }

        private void tbxPassword_KeyDown( object sender, KeyEventArgs e ) {
            if ( e.Key == Key.Enter && !string.IsNullOrWhiteSpace(tbxUsername.Text) && !string.IsNullOrWhiteSpace(tbxPassword.Password) )
                add( tbxUsername.Text, tbxPassword.Password );
        }

        private void add( string username, string password ) {
            byte[] pass = Encoding.ASCII.GetBytes( password );
            HMACSHA512 hmac1 = new HMACSHA512( pass );
            StringBuilder sb = new StringBuilder();
            byte[] passHash = hmac1.ComputeHash( pass );
            foreach ( byte b in passHash )
                sb.Append( (char)b );
            Properties.Settings.Default.Password = sb.ToString();
            sb.Clear();
            HMACSHA384 hmac2 = new HMACSHA384( passHash );
            byte[] key = hmac2.ComputeHash( pass );
            for ( int i = 0; i < username.Length; i++ )
                sb.Append( (char)((byte)username[i] ^ key[i % key.Length]) );
            Properties.Settings.Default.Username = sb.ToString();
            Properties.Settings.Default.Save();
            ContentFile.Encrypt( password );
            MessageBox.Show( "Registered your user!" );
            Paging.LoadPage(Pages.Login);
        }

        private void button_Click( object sender, RoutedEventArgs e ) {
            if ( !string.IsNullOrWhiteSpace(tbxUsername.Text) && !string.IsNullOrWhiteSpace(tbxPassword.Password) )
                add( tbxUsername.Text, tbxPassword.Password );
        }
    }
}