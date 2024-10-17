using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using APIClasses;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace ClientApp
{
    [ServiceBehavior(InstanceContextMode =InstanceContextMode.Single)]
    public class Server: MarshalByRefObject, IJobService
    {
        private volatile bool shutdown = false;
        private Client currClient;
        public List<Job> availableJobs = new List<Job>();
        private Dictionary<int, string> jobResults = new Dictionary<int, string>();

        public Server(){}
        public Server(Client client)
        {
            this.currClient = client;
        }
        public void Run()
        {
            try
            {
                if(ChannelServices.GetChannel("JobService")!= null)
                {
                    ChannelServices.UnregisterChannel(ChannelServices.GetChannel("JobService"));
                }
                // Initialize and register the TCP channel
                TcpChannel channel = new TcpChannel(currClient.Port);
                ChannelServices.RegisterChannel(channel, false);

                // Register this server as a remote object
                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof(Server),               
                    "JobService",                 
                    WellKnownObjectMode.Singleton 
                );

                Console.WriteLine($"Server is running on {currClient.IPAddr}:{currClient.Port}");

                // Server main loop
                while (!shutdown)
                {
                    Console.WriteLine("Server Thread running, hosting jobs");
                    Task.Delay(5000).Wait();
                }

                // Shutdown the service when needed
                Console.WriteLine("Shutting down the server...");
                if (ChannelServices.GetChannel("JobService") != null)
                {
                    ChannelServices.UnregisterChannel(channel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting server: {ex.Message}");
                ShutDown();
            }
        }

        public void ShutDown()
        {
            shutdown = true;
        }

        public Job GetJob()
        {
            
            lock (availableJobs)
            {
                if (availableJobs.Count > 0)
                {
                    Console.WriteLine("Job Availiable serving job");
                    var job = availableJobs[0];
                    availableJobs.RemoveAt(0);
                    return job;
                }

                Console.WriteLine("No jobs availiable to serve");
                return null;
            }
        }

        public void SubmitSolution(int Jobid, string result)
        {
            lock (jobResults)
            {
                jobResults[Jobid] = result;
            }
        }

        public void AddJob(Job newJob)
        {
            lock (availableJobs)
            {
                availableJobs.Add(newJob);
                Console.WriteLine($"Server {currClient.ClientID} has added new job");
            }
        }
        
    }
}
