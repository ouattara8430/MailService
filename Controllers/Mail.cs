namespace MailService.Controllers
{
    public class Mail
    {
        public string to { get; set; }
        public string subject { get; set; }
        public string emailBody { get; set; }
        public string copy { get; set; }
        public string attachment { get; set; }
    }
}