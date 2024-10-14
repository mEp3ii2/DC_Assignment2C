namespace WebServer.Models
{
    public class Job
    {
        public int JobId { get; set; } // Primary Key
        public string JobData { get; set; } 
        public string Status { get; set; } 
        public string AssignedClientIP { get; set; } 
        public int AssignedClientPort { get; set; } 
        public string Result { get; set; } 
        public DateTime? CompletedAt { get; set; } 
        public DateTime LastUpdated { get; set; } 
    }
}
