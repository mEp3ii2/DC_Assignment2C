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
        public async Task Run()
        {
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
                Thread.Sleep(1000);
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
                RestRequest request = new RestRequest("api/GetClients", Method.Get);
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
                using(TcpClient tcpClient = new TcpClient())
                {
                    tcpClient.Connect(client.IPAddr, int.Parse(client.Port));

                    NetworkStream stream = tcpClient.GetStream();

                    byte[] requestBytes = Encoding.UTF8.GetBytes("GET_JOB");
                    stream.Write(requestBytes, 0, requestBytes.Length);

                    byte[] responseBytes = new byte[4096];
                    int bytesRead = stream.Read(responseBytes, 0, responseBytes.Length);
                    string response = Encoding.UTF8.GetString(responseBytes,0,bytesRead);

                    if(response == "NO_JOB")
                    {
                        return null;
                    }
                    else
                    {
                        Job job = JsonConvert.DeserializeObject<Job>(response);

                        // Verify the hash
                        bool isValid = VerifyHash(job.Base64Code, job.Hash);
                        if (!isValid)
                        {
                            Console.WriteLine("Hash verification failed. Discarding job.");
                            return null;
                        }

                        return job;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to client {client.ClientID}: {ex.Message}");
                return null;
            }
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
                dynamic result = scope.GetVariable("result");
                return result.ToString();

            }
            catch (Exception ex)
            { 
                Console.WriteLine("Error executing job: "+ex.Message);
                
            }
            return null;
        }

        private void PostJobResult(Client client, int jobId, string result)
        {

        }
    } 
}

