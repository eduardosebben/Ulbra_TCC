using System.ComponentModel.DataAnnotations;

namespace API_Sebben_que_é_agenda.Services
{
    public class SmtpOptions
    {
        [Required] public string Host { get; set; } = "";
        [Range(1, 65535)] public int Port { get; set; } = 587;
        public string? User { get; set; }
        public string? Password { get; set; }
        [Required] public string From { get; set; } = "no-reply@localhost";
        public string FromName { get; set; } = "Sebben Agenda";
        public bool UseStartTls { get; set; } = true;
        public bool UseSsl { get; set; } = false;
    }
}
