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
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public static string Password;

        public Login() {
            InitializeComponent();
        }

        private void tbxUsername_KeyDown( object sender, KeyEventArgs e ) {
            if ( e.Key == Key.Enter )
                tbxPassword.Focus();
        }

        private void checkDetails( string username, string password ) {
            byte[] pass = Encoding.ASCII.GetBytes( password );
            HMACSHA512 hmac1 = new HMACSHA512( pass );
            StringBuilder sb = new StringBuilder();
            byte[] passHash = hmac1.ComputeHash( pass );
            foreach ( byte b in passHash )
                sb.Append( (char)b );
            if ( sb.ToString() == Properties.Settings.Default.Password ) {
                sb.Clear();
                HMACSHA384 hmac2 = new HMACSHA384( passHash );
                byte[] key = hmac2.ComputeHash( pass );
                for ( int i = 0; i < username.Length; i++ )
                    sb.Append( (char)((byte)username[i] ^ key[i % key.Length]) );
                if ( sb.ToString() == Properties.Settings.Default.Username ) {
                    Password = password;
                    Paging.LoadPage( Pages.Welcome );
                    lblBadInput.Visibility = Visibility.Hidden;
                    return;
                }
            }
            lblBadInput.Visibility = Visibility.Visible;
        }

        private void button_Click( object sender, RoutedEventArgs e ) {
            if ( MessageBox.Show( "Are you sure you want to change your login?", "Change Login", MessageBoxButton.YesNo ) == MessageBoxResult.Yes ) {
                string password;
                while ( true ) {
                    password = Microsoft.VisualBasic.Interaction.InputBox( "Please enter your password to continue", "Change Login" );
                    if ( string.IsNullOrWhiteSpace( password ) )
                        break;
                    else {
                        byte[] pass = Encoding.ASCII.GetBytes( password );
                        HMACSHA512 hmac1 = new HMACSHA512( pass );
                        StringBuilder sb = new StringBuilder();
                        byte[] passHash = hmac1.ComputeHash( pass );
                        foreach ( byte b in passHash )
                            sb.Append( (char)b );
                        if ( sb.ToString() == Properties.Settings.Default.Password )
                            break;
                        else
                            MessageBox.Show( "Password was incorrect", "Change Login" );
                    }
                }
                if ( string.IsNullOrWhiteSpace( password ) ) {
                    MessageBox.Show( "Login was not reset", "Change Login" );
                    return;
                }

                //Decrypt data and prepare for re-encryption
                if ( !ContentFile.CheckValidity() )
                    ContentFile.Decrypt( password );

                //TODO - MOVE TO CONTENT FILE
                Properties.Settings.Default.Password = "";
                Properties.Settings.Default.Username = "";
                Properties.Settings.Default.Save();

                Paging.LoadPage( Pages.Register );
            }
            else
                MessageBox.Show( "Login was not reset" );
        }

        private void button_Click_1( object sender, RoutedEventArgs e ) {
            if ( !string.IsNullOrWhiteSpace( tbxPassword.Password ) && !string.IsNullOrWhiteSpace( tbxUsername.Text ) )
                checkDetails( tbxUsername.Text, tbxPassword.Password );
        }

        private void tbxPassword_KeyDown( object sender, KeyEventArgs e ) {
            if ( e.Key == Key.Enter && !string.IsNullOrWhiteSpace( tbxPassword.Password ) && !string.IsNullOrWhiteSpace( tbxUsername.Text ) )
                checkDetails( tbxUsername.Text, tbxPassword.Password );
        }

        private void page_Loaded( object sender, RoutedEventArgs e ) {
            lblBadInput.Visibility = Visibility.Collapsed;
            tbxUsername.Text = "";
            tbxPassword.Password = "";
        }
    }
}