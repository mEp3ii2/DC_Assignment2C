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
using System.Windows;

namespace ClientApp
{
    [ServiceBehavior(InstanceContextMode =InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Server: MarshalByRefObject, IJobService
    {
        private volatile bool shutdown = false;
        private Client currClient;
        public static Queue<Job> availableJobs = new Queue<Job>();
        public static Dictionary<int, Job> heldJobs = new Dictionary<int, Job>();
        private readonly Guid instanceId = Guid.NewGuid();
        private CurrentStatus status;
       
        public Server(){}
        public Server(Client client,CurrentStatus status)
        {
            this.currClient = client;
            this.status = status;
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
                    WellKnownObjectMode.SingleCall // Change from Singleton to PerCall
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
            Console.WriteLine($"Server Unique ID: {instanceId}");
            Console.WriteLine($"ty Current job count: {availableJobs.Count}");
            lock (availableJobs)
            {
                if (availableJobs.Count > 0)
                {
                    Console.WriteLine("Job Availiable serving job");
                    Job job = availableJobs.Dequeue();
                    heldJobs[job.JobId] = job;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MainWindow.Instance.updateJobBoard(job.JobId, "Processing", null);
                    });
                    return job;
                }

                Console.WriteLine("No jobs availiable to serve");
                return null;
            }
        }

        public void confirmJob(Job job, bool success)
        {
            if (heldJobs.ContainsKey(job.JobId))
            {
                if (success)
                {
                    heldJobs.Remove(job.JobId);
                }
                else
                {
                    availableJobs.Enqueue(job);
                    heldJobs.Remove(job.JobId);
                }
            }
        }
        public void SubmitSolution(int Jobid, string result)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.Instance.updateJobBoard(Jobid, "Completed",result);
            });
        }

        public void AddJob(Job newJob)
        {
            Console.WriteLine($"{currClient.IPAddr}:{currClient.Port} with the id of {instanceId} adding a new job to its job board");
            lock (availableJobs)
            {
                availableJobs.Enqueue(newJob);
                //availableJobs.Append(newJob);
                Console.WriteLine($"{currClient.IPAddr}:{currClient.Port} with the id of {instanceId} has added a new job to its job board");
                Console.WriteLine($"Current job count: {availableJobs.Count}");
                status.jobsPosted++;
            }
        }

        public CurrentStatus GetCurrentStatus()
        {
            return status;
        }

        
    }
}
