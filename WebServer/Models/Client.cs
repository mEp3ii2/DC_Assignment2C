namespace WebServer.Models
{
    public class Client
    {
        public int ClientID { get; set; }
        public string IPAddr { get; set; }
        public int Port { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
