using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetworkProgrammingP12
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        private Random random = new();
        private IPEndPoint? endPoint;
        private DateTime lastSyncMoment;
        private bool isServerOn;

        public ClientWindow()
        {
            InitializeComponent();
            lastSyncMoment = default;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginTextBox.Text = "User " + random.Next(100);
            MessageTextBox.Text = "Hello, all!";
            isServerOn = true;
            Sync();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            String[] adress = HostTextBox.Text.Split(':');
            try
            {
                endPoint = new(
                                IPAddress.Parse(adress[0]),
                                Convert.ToInt32(adress[1]));
                new Thread(SendMessage).Start(new ClientRequest 
                    { 
                    Command = "Message",
                    Message = new()
                    {
                        Login = LoginTextBox.Text,
                        Text = MessageTextBox.Text
                    }
                });
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }

        private async void Sync()
        {
            if (isServerOn)
            {
                String[] adress = HostTextBox.Text.Split(':');
                try
                {
                    endPoint = new(
                                    IPAddress.Parse(adress[0]),
                                    Convert.ToInt32(adress[1]));
                    new Thread(SendMessage).Start(new ClientRequest
                    {
                        Command = "Check",
                        Message = new()
                        {
                            Moment = lastSyncMoment
                        }
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }                
            }
            await Task.Delay(1000);
            Sync();
        }


        private void SendMessage(Object? arg)
        {
            var clientRequest = arg as ClientRequest;
            if (endPoint == null || clientRequest == null)
            {
                return;
            }
            Socket clientSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                clientSocket.Connect(endPoint);
                isServerOn = true;
                clientSocket.Send(
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(clientRequest)));

                MemoryStream memoryStream = new();
                byte[] buffer = new byte[1024];
                do
                {
                    int n = clientSocket.Receive(buffer);
                    memoryStream.Write(buffer, 0, n);
                } while (clientSocket.Available > 0);

                String str = Encoding.UTF8.GetString(memoryStream.ToArray());
                var response = JsonSerializer.Deserialize<ServerResponse>(str);
                if (response == null)
                {
                    str = "JSON Error in " + str;
                }
                else
                {
                    str = "";
                    if(response.Messages != null)
                    {
                        foreach (var message in response.Messages)
                        {
                            str += message + "\n";
                            if(message.Moment > lastSyncMoment)
                            {
                                lastSyncMoment = message.Moment;
                            }
                        }
                    }
                }

                Dispatcher.Invoke(() => ClientLog.Text += str);

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Dispose();
            }
            catch(Exception ex)
            {
                if (isServerOn)
                {
                    isServerOn = false;
                    clientSocket.Dispose();
                    MessageBox.Show(ex.Message);
                    isServerOn = true;
                }                
            }
        }
    }
}
