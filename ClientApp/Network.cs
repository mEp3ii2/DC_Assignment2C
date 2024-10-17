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
using Newtonsoft.Json;
using System.Security.Cryptography;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;


namespace ClientApp
{
    
    public class Network
    {
        private int currentClientId;
        private volatile bool shutdown = false;
        private Client currClient;
        

        public Network(Client client)
        {
            this.currClient = client;
        
        }

        
        public async Task Run()
        {
            Console.WriteLine("Pausing to give time for the server to start");
            Task.Delay(4000).Wait();
            await registerForJobs(currClient);
            await Loop();
        }

        public async Task Loop()
        {
            while (!shutdown)
            {
                List<Client> clients = await ClientLookUp();
                foreach(var client in clients)
                {
                    if(client.ClientID != currentClientId)
                    {
                        Job job = JobLook(client);
                        if(job!= null)
                        {
                            string result = ExecuteJob(job);
                            PostJobResult(client, job.JobId, result);
                        }
                    }
                }
                Task.Delay(10000).Wait();
            }
        }

        private async Task registerForJobs(Client currClient)
        {
            try
            {
                RestClient client = new RestClient("http://localhost:5013");
                RestRequest request = new RestRequest("api/Client/RegisterClient", Method.Post);
                request.AddJsonBody(currClient);

                var response = await client.ExecuteAsync<Dictionary<string, int>>(request);

                if (response.IsSuccessful && response.Content != null)
                {
                    // Manually deserialize the content
                    var responseData = JsonConvert.DeserializeObject<Dictionary<string, int>>(response.Content);

                    if (responseData.ContainsKey("clientID"))
                    {
                        currentClientId = responseData["clientID"];
                    }
                    else
                    {
                        Console.WriteLine("Failed to get ClientID in response");
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to register client: {response.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        

        public void ShutDown()
        {
            shutdown = true;
        }

        private async Task<List<Client>> ClientLookUp() 
        {
            try
            {
                RestClient client = new RestClient("http://localhost:5013");
                RestRequest request = new RestRequest("api/Client/GetClients", Method.Get);
                var response = await client.ExecuteAsync<List<Client>>(request);

                if (response.IsSuccessful)
                {
                    return response.Data;
                }
                else
                {
                    Console.WriteLine("Error fetching client list: " + response.ErrorMessage);
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Exception in ClientLookUp: "+ex.Message);
            }
            return new List<Client>();
        }

        private Job JobLook(Client client)
        {
            try
            {
                // Create the URL for the remote job service
                string url = $"tcp://{client.IPAddr}:{client.Port}/JobService";
                Console.WriteLine($"Attempting to connect to client {client.ClientID} at {url}");
                IJobService jobService = (IJobService)Activator.GetObject(typeof(IJobService), url);

                Console.WriteLine($"{currentClientId}Asking for job from {client.ClientID}");
                // Request a job from the client
                Job job = jobService.GetJob();

                if (job != null)
                {
                    Console.WriteLine($"Job found from client {client.ClientID}");
                    // Verify job details (hash, etc.)
                    bool isValid = VerifyHash(job.Base64Code, job.Hash);
                    if (!isValid)
                    {
                        Console.WriteLine("Hash verification failed. Discarding job.");
                        return null;
                    }

                    return job;
                }
                else
                {
                    Console.WriteLine($"No jobs available from {client.ClientID}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to client {client.ClientID}: {ex.Message}");
            }

            return null;
        }

        private bool VerifyHash(string base64Code, string expectedHash)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] codeBytes = Encoding.UTF8.GetBytes(base64Code);
                byte[] hashBytes = sha256.ComputeHash(codeBytes);
                string computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                return computedHash == expectedHash.ToLowerInvariant();
            }
        }
        private string ExecuteJob(Job job)
        {
            try
            {
                byte[] codeBytes = Convert.FromBase64String(job.Base64Code);
                string code = Encoding.UTF8.GetString(codeBytes);
                ScriptEngine engine = Python.CreateEngine();
                ScriptScope scope = engine.CreateScope();
                engine.Execute(code, scope);
                dynamic jobeCodeFunction = scope.GetVariable("jobCode");
                dynamic result = jobeCodeFunction();
                
                return result != null ? result.ToString() : null;

            }
            catch (Exception ex)
            { 
                Console.WriteLine("Error executing job: "+ex.Message);
                
            }
            return null;
        }

        private void PostJobResult(Client client, int jobId, string result)
        {
            try
            {
                // Create the URL for the remote job service
                string url = $"tcp://{client.IPAddr}:{client.Port}/JobService";
                IJobService jobService = (IJobService)Activator.GetObject(typeof(IJobService), url);
                // Submit the solution to the remote client
                jobService.SubmitSolution(jobId, result);

                Console.WriteLine($"Successfully submitted result for Job ID {jobId} to client {client.ClientID}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error posting job result to client {client.ClientID}: {ex.Message}");
            }
        }
    } 
}

