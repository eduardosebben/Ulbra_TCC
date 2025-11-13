using System.ComponentModel.DataAnnotations;

namespace API_Sebben_que_é_agenda.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string SenhaHash { get; set; }
        [Required]
        public int TipUsuario { get; set; }
    }
}
