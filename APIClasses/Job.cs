using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace APIClasses
{
    [DataContract]
    public class Job
    {
        [DataMember]
        public int JobId { get; set; }

        [DataMember]
        public string JobName { get; set; }
        [DataMember] 
        public string Status { get; set; }
        
        [DataMember] 
        public string Result { get; set; }
        
        [DataMember] 
        public string Base64Code { get; set; }
        
        [DataMember] 
        public string Hash { get; set; }

    }
}
