using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using APIClasses;
using System.Net;
using System.Net.Sockets;

namespace ClientApp
{
    public class Network
    {
        private volatile bool shutdown = false;
        public async void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();           
            string port = ((IPEndPoint)listener.LocalEndpoint).Port.ToString();
            
            var clientObj = new Client
            {
                IPAddr = "127.0.0.1",
                Port = port,
                LastUpdated = DateTime.Now
            };

            RestClient client = new RestClient("http://localhost:5013/api/Client");
            RestRequest request = new RestRequest("RegisterClient", Method.Post);
            request.AddJsonBody(clientObj);

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                Loop();    
            }            
        }

        public void Loop()
        {
            while (!shutdown)
            {

            }
        }
        
        public void ShutDown()
        {
            shutdown = true;
        }

        public void ClientLookUp() {

        }

        public void JobLook()
        {

        }

        public void ExecuteJob()
        {

        }
    } 
}

