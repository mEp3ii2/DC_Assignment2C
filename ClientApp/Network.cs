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
using System.Windows;

namespace ClientApp
{
    
    public class Network
    {
        private static int jobCount = 0;
        private int currentClientId;
        private volatile bool shutdown = false;
        private Client currClient;
        private CurrentStatus status;

        public Network(Client client, CurrentStatus status)
        {
            this.currClient = client;
            this.status = status;
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
                
                foreach (var client in clients)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MainWindow.Instance.updateWorkStatus("SEARCHING");
                    });

                    if (client.ClientID != currentClientId)
                    {
                        Job job = JobLook(client);
                        if(job!= null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MainWindow.Instance.updateWorkStatus($"PROCESSING {job.JobName}");
                            });
                            string result = ExecuteJob(job);
                            PostJobResult(client, job.JobId, result);
                            jobCount++;
                            status.jobCompleted++;
                            
                        }
                    }
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.Instance.updateJobCount(jobCount);
                    MainWindow.Instance.updateWorkStatus("IDLE");
                });
                await updateStatus();
                Task.Delay(10000).Wait();
            }
        }
        private async Task updateStatus()
        {
            try
            {
                RestClient client = new RestClient("http://localhost:5013");
                RestRequest request = new RestRequest("api/Client/UpdateStatus", Method.Post);
                request.AddJsonBody(status);
                RestResponse response = await client.ExecuteAsync(request);

                // Check the status of the response
                if (response.IsSuccessful)
                {
                    Console.WriteLine("Client status updated successfully.");
                }
                else
                {
                    // Handle the failure case (logging, throwing exception, etc.)
                    Console.WriteLine($"Failed to update client status. Status Code: {response.StatusCode}, Error: {response.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in updateStatus: {ex.Message}");
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
                        status.ClientID = currentClientId;
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
                Console.WriteLine($"{currClient.IPAddr}:{currClient.Port} requesting for job from {client.IPAddr}:{client.Port}");
                // Request a job from the client
                Job job = jobService.GetJob();

                if (job != null)
                {

                    Console.WriteLine($"Job found from client {client.ClientID}");
                    // Verify job details (hash, etc.)
                    bool isValid = VerifyHash(job.Base64Code, job.Hash);
                    if (!isValid)
                    {
                        jobService.confirmJob(job,false);
                        Console.WriteLine("Hash verification failed. Discarding job.");
                        return null;
                    }
                    jobService.confirmJob(job,true);
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
            Console.WriteLine("Starting hash verification...");
            Console.WriteLine($"Received base64Code: {base64Code}");
            Console.WriteLine($"Expected hash: {expectedHash}");

            try
            {

                using (SHA256 sha256 = SHA256.Create())
                {
                    // Compute the hash of the Base64-encoded string (without decoding it to byte array)
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(base64Code));
                    Console.WriteLine("Hash computed successfully.");

                    // Convert the byte array hash into a hexadecimal string
                    string computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    Console.WriteLine($"Computed hash: {computedHash}");

                    // Compare the computed hash to the expected hash
                    bool hashesMatch = computedHash == expectedHash.ToLowerInvariant();
                    Console.WriteLine($"Do the hashes match? {hashesMatch}");

                    return hashesMatch;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during hash verification: {ex.Message}");
                return false;
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
            return "Invalid Python Code";
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

