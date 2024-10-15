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
        public MainWindow()
        {
            InitializeComponent();

            

            network = new Network();
            networkThread = new Thread(new ThreadStart(network.Run));
            networkThread.IsBackground = true;
            networkThread.Start();

            server = new Server();
            serverThread = new Thread(new ThreadStart(server.Run));
            serverThread.IsBackground = true; // Optional
            serverThread.Start();

            btnSubmit.IsEnabled = false;

            //listner for code 
            codeTxt.TextChanged += JobDetailsChanged;
            JobTileTxt.TextChanged += JobDetailsChanged;

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

        
    }
}
