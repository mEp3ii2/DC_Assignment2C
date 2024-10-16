using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIClasses
{
    public class Client
    {
        public int ClientID { get; set; }
        public string IPAddr { get; set; }
        public int Port { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
