using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;

namespace NetworkProgrammingP12
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private String ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""D:\visual studio projects\NetworkProgrammingP12\Database1.mdf"";Integrated Security=True";
        public AuthWindow()
        {
            InitializeComponent();
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using SqlConnection connection = new(ConnectionString);
                connection.Open();

                using SqlCommand command = connection.CreateCommand();
                string code = Guid.NewGuid().ToString()[..6].ToUpper();
                command.CommandText = "INSERT INTO [Users](Email, Password, ConfirmCode) " +
                                      $"VALUES(N'{textboxEmail.Text}', N'{textboxPassword.Password}', '{code}')";
                command.ExecuteNonQuery();
                using SmtpClient? smtpClient = GetSmtpClient();
                if (smtpClient is null) { MessageBox.Show("Connection to smtp failed..."); return; }

                MailMessage mailMessage = new MailMessage(
                    App.GetConfiguration("smtp:email")!,
                    textboxEmail.Text,
                    "Sign up seccussfull",
                    $"Congratulations! To confirm Email use code: <span style='color: tomato; font-weight: bold;'>{code}</span>"
                )
                { IsBodyHtml = true };

                smtpClient.Send(mailMessage);

                MessageBox.Show("Check Email");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private SmtpClient? GetSmtpClient()
        {
            #region get and check config
            String? host = App.GetConfiguration("smtp:host");
            if (host == null)
            {
                MessageBox.Show("Error getting host");
                return null;
            }
            String? portString = App.GetConfiguration("smtp:port");
            if (portString == null)
            {
                MessageBox.Show("Error getting port");
                return null;
            }
            int port;
            try { port = int.Parse(portString); }
            catch
            {
                MessageBox.Show("Error parsing port");
                return null;
            }
            String? email = App.GetConfiguration("smtp:email");
            if (email == null)
            {
                MessageBox.Show("Error getting email");
                return null;
            }
            String? password = App.GetConfiguration("smtp:password");
            if (password == null)
            {
                MessageBox.Show("Error getting password");
                return null;
            }
            String? sslString = App.GetConfiguration("smtp:ssl");
            if (sslString == null)
            {
                MessageBox.Show("Error getting ssl");
                return null;
            }
            bool ssl;
            try { ssl = bool.Parse(sslString); }
            catch
            {
                MessageBox.Show("Error parsing ssl");
                return null;
            }
            #endregion

            return new(host, port)
            {
                EnableSsl = ssl,
                Credentials = new NetworkCredential(email, password)
            };
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmContainer.Tag is String savedCode)
            {
                if (textboxCode.Text.Equals(savedCode))
                {
                    using SqlConnection connection = new(ConnectionString);
                    connection.Open();

                    using SqlCommand command = connection.CreateCommand();
                    command.CommandText = $"UPDATE [Users] SET ConfirmCode = NULL " +
                                          $"WHERE [Email] = '{textboxEmail.Text}' AND [Password] = '{textboxPassword.Password}'";
                    command.ExecuteNonQuery();

                    ConfirmContainer.Visibility = Visibility.Hidden;
                    logBlock.Text += "Email confirmed!\n";
                }
                else
                {
                    logBlock.Text += "Email not confirmed\n";
                }
            }
        }

        private void SigninButton_Click(object sender, RoutedEventArgs e)
        {
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM [Users] WHERE " +
                $" [Email]=N'{textboxEmail.Text}' " +
                $" AND [Password]=N'{textboxPassword.Password}' ";
            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())  // Користувач знайдений
            {
                if (!reader.IsDBNull("ConfirmCode"))   // код не NULL - не підтверджений
                {
                    String code = reader.GetString("ConfirmCode");
                    ConfirmContainer.Visibility = Visibility.Visible;
                    ConfirmContainer.Tag = code;
                    textboxCode.Focus();
                    logBlock.Text += "Welcome, Email needs confirmation\n";
                }
                else
                {
                    logBlock.Text += "Welcome, Email confirmed\n";
                }
            }
            else  // Неправильні логін/пароль
            {
                MessageBox.Show("Credentials incorrect");
            }
        }
    }
}
