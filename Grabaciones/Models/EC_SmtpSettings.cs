namespace Grabaciones.Models
{
    public class EC_SmtpSettings
    {
        public string ?Server { get; set; }
        public int Port { get; set; }
        public string ?Username { get; set; }
        public string ?Password { get; set; }
        public string ?From { get; set; }
        public List<string> ?To { get; set; }
        public List<string> ?CC { get; set; }
        public List<string> ?BCC { get; set; }
    }
}
