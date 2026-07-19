using System;

namespace FinalProject.Models
{
    public class Email
    {
        public int Id { get; set; }
        public string EmailReceiver { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime DateSent { get; set; }
        public string EmailSender { get; set; }
        public bool ReadStatus { get; set; }
    }

}
