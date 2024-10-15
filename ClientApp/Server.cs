using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ClientApp
{
    public class Server
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
    }
}
