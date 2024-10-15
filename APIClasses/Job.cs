using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIClasses
{
    public class Job
    {
        public int JobId { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
        public string Base64Code { get; set; } 
        public string Hash { get; set; }

    }
}
