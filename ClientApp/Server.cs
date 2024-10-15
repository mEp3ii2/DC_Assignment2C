using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using APIClasses;
using System.Security.Cryptography;
using System.ServiceModel;
using WebServer;

namespace ClientApp
{
    [ServiceBehavior(InstanceContextMode =InstanceContextMode.Single)]
    public class Server: IJobService
    {
        private volatile bool shutdown = false;
        public void Run()
        {
            while (!shutdown)
            {
                Console.WriteLine("Server Thread running");
                Thread.Sleep(1000);
            }
        }

        public void ShutDown()
        {
            shutdown = true;
        }

        public void AddJob(Job job)
        {
            // Base64-encode the code
            byte[] codeBytes = Encoding.UTF8.GetBytes(job.Base64Code); // Original code
            string base64Code = Convert.ToBase64String(codeBytes);
            job.Base64Code = base64Code;

            // Compute SHA256 hash of the Base64-encoded code
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(base64Code));
                job.Hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }

            lock (jobQueue)
            {
                jobQueue.Add(job);
            }
        }

        List<int> IJobService.GetAvailableJobIds()
        {
            throw new NotImplementedException();
        }

        Job IJobService.GetJob(int jobId)
        {
            throw new NotImplementedException();
        }

        bool IJobService.SubmitJobResult(JobResult result)
        {
            throw new NotImplementedException();
        }
    }
}
