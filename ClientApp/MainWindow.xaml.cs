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
using System.Security.Cryptography;
using System.Collections.ObjectModel;

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
        private static CurrentStatus status;
        public static MainWindow Instance { get; private set; }

        public ObservableCollection<Job> jobsList { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            currClient = setUpClient();
            this.Closing += MainWindowClosing;

            status = new CurrentStatus
            {
                ClientID = currClient.ClientID,
                IPAddr = currClient.IPAddr,
                Port = currClient.Port,
                jobsPosted = 0,
                jobCompleted = 0,
                LastUpdated = currClient.LastUpdated
            };

            network = new Network(currClient,status);
            Task.Run(() => network.Run());

            server = new Server(currClient,status);
            Task.Run(() => server.Run());
            

            jobsList = new ObservableCollection<Job>();
            JobBoardTbl.ItemsSource = jobsList;




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
            listener.Stop();
            return currClient;
        }

        /*
         * Currently not used as not sure if curtin device lets me use other addess
         * Defualting to localhost
         */
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

        /*
         * Enables the submit job button if text is detected in the code and name txt boxes
         */
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

            string base64Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(codeText));

            using(SHA256 sha256Hash = SHA256.Create())
            {
                byte[] hashBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(base64Code));
                string hash = BitConverter.ToString(hashBytes).Replace("-","").ToLower();

                Job job = new Job
                {
                    JobName = JobTitleText,
                    Status = "Pending",
                    Result = null,
                    Base64Code = base64Code,
                    Hash = hash
                };

                jobsList.Add(job);
                server.AddJob(job);
                
            }
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
       
        public void updateJobBoard(int jobId, string newStatus, string newResult)
        {
            // Find the job in the ObservableCollection by JobId
            Job jobToUpdate = jobsList.FirstOrDefault(job => job.JobId == jobId);

            if (jobToUpdate != null)
            {
                // Only update Status if a new value is provided
                if (!string.IsNullOrEmpty(newStatus))
                {
                    jobToUpdate.Status = newStatus;
                }

                // Only update Result if a new value is provided
                if (!string.IsNullOrEmpty(newResult))
                {
                    jobToUpdate.Result = newResult;
                }

                Console.WriteLine($"Job {jobId} updated with Status: {jobToUpdate.Status} and Result: {jobToUpdate.Result}");
            }
            else
            {
                Console.WriteLine($"Job with ID {jobId} not found.");
            }
        }

        public void updateWorkStatus(string currentStatus)
        {
            CurrentStatusLabel.Content = "Current Status: " + currentStatus;
        }

        public void updateJobCount(int jobCount)
        {
            CompletedJobsLabel.Content = "Completed Jobs: "+jobCount.ToString();
        }
    }
}
