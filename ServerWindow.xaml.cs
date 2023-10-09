using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
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
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {

        private Socket? listenSocket;
        private IPEndPoint? endPoint;

        public ServerWindow()
        {
            InitializeComponent();
        }

        private void SwitchServer_Click(object sender, RoutedEventArgs e)
        {
            if (listenSocket == null)
            {
                try
                {
                    IPAddress ip = IPAddress.Parse(HostTextBox.Text);
                    int port = Convert.ToInt32(PortTextBox.Text);
                    endPoint = new(ip, port);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Неправильні параметри кофігурації : " + ex.Message);
                    return;
                }
                listenSocket = new(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);
                new Thread(StartServer).Start();
            }
            else
            {
                listenSocket.Close();
            }
        }
        private void StartServer()
        {
            if (listenSocket == null || endPoint == null)
            {
                MessageBox.Show("Спроба запуску без ініціалізації данних");
                return;
            }

            try
            {
                listenSocket.Bind(endPoint);
                listenSocket.Listen(10);
                
                
                Dispatcher.Invoke(() => {
                    StatusLabel.Background = Brushes.LightGreen;
                    StatusLabel.Content = "Увімкнено";
                    SwitchServer.Content = "Вимкнути";
                    ServerLog.Text += "Сервер запущен\n";
                });

                byte[] buffer = new byte[1024];
                while (true)
                {
                    Socket socket = listenSocket.Accept();
                    /*
                    StringBuilder stringBuilder = new();
                    do
                    {
                        int n = socket.Receive(buffer);
                        stringBuilder.Append(Encoding.UTF8.GetString(buffer, 0, n));
                    } while (socket.Available > 0);
                    String str = stringBuilder.ToString(); */
                    MemoryStream memoryStream = new();
                    do
                    {
                        int n = socket.Receive(buffer);
                        memoryStream.Write(buffer, 0, n);
                    } while (socket.Available > 0);
                    String str = Encoding.UTF8.GetString(memoryStream.ToArray());

                    ServerResponse serverResponse = new();
                    var clientRequest = JsonSerializer.Deserialize<ClientRequest>(str);
                    if(clientRequest == null)
                    {
                        str = "Error decoding JSON: " + str;
                        serverResponse.Status = "400 bad request";
                        serverResponse.Data = "Error decoding JSON";
                    }
                    else
                    {
                        str = clientRequest.Data;
                        serverResponse.Status = "200 OK";
                        serverResponse.Data = "Received " + DateTime.Now;
                    }
                    Dispatcher.Invoke(() => ServerLog.Text += $"{DateTime.Now} {str} \n");

                    socket.Send(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(serverResponse)));
                    socket.Close();
                }
            }
            catch (Exception ex)
            {
                listenSocket = null;
                
                Dispatcher.Invoke(() =>
                {
                    StatusLabel.Background = Brushes.Pink;
                    StatusLabel.Content = "Вимкнено";
                    SwitchServer.Content = "Увімкнути";
                    ServerLog.Text += "Сервер зупинено\n";
                });
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listenSocket?.Close();
        }
    }
}
