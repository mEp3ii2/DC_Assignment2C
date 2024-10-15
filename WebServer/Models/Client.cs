namespace WebServer.Models
{
    public class Client
    {
        public int ClientID { get; set; }
        public string IPAddr { get; set; }
        public string Port { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
