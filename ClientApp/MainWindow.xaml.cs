using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Forms;
using APIClasses;
using System.Net.Sockets;
using System.Net;
using RestSharp;

namespace ClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread networkThread;
        private Thread serverThread;

        private Network network;
        private Server server;
        Client currClient;
        public MainWindow()
        {
            InitializeComponent();

            currClient = setUpClient();
            this.Closing += MainWindowClosing;

            server = new Server(currClient);
            Task.Run(() => server.Run());

            network = new Network(currClient);
            Task.Run(() => network.Run());
            

            
            

            btnSubmit.IsEnabled = false;

            //listner for code 
            codeTxt.TextChanged += JobDetailsChanged;
            JobTileTxt.TextChanged += JobDetailsChanged;

        }

        public Client setUpClient()
        {

            TcpListener listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            //string localIP = GetLocalIPAddress();
            string localIP = "127.0.0.1";

            Client currClient = new Client
            {
                IPAddr = localIP,
                Port = port,
                LastUpdated = DateTime.Now
            };

            CurrentIPAddressLabel.Content = $"IP Address: {localIP}";
            CurrentPortLabel.Content = $"Port Number: {port}";
            return currClient;
        }
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void JobDetailsChanged(object sender, EventArgs e)
        {
            btnSubmit.IsEnabled = !string.IsNullOrWhiteSpace(codeTxt.Text) && !string.IsNullOrWhiteSpace(JobTileTxt.Text); 
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            String codeText = codeTxt.Text.ToString();
            String JobTitleText = JobTileTxt.Text.ToString();
            codeTxt.Clear();
            JobTileTxt.Clear();
            btnSubmit.IsEnabled=false;
            //do logic here 
        }

        private void MainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            network.ShutDown();
            server.ShutDown();

            try
            {
                // Assuming RestClient is set up to interact with the WebServer
                RestClient client = new RestClient("http://localhost:5013");
                var request = new RestRequest("api/Client/RemoveClient", Method.Delete);
                request.AddParameter("ipAddr", currClient.ClientID);
                request.AddParameter("port",currClient.Port);

                var response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    Console.WriteLine("Client removed successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to remove client.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during shutdown: {ex.Message}");
            }
        }
    }
}
