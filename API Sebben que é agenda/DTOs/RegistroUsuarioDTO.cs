using System.ComponentModel.DataAnnotations;

namespace API_Sebben_que_é_agenda.DTOs
{
    public class RegistroUsuarioDTO
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Senha { get; set; }

        [Required(ErrorMessage = "O tipo de usuário é obrigatório.")]
        [Range(1, 2, ErrorMessage = "Tipo de usuário inválido. Use 1 (Secretária) ou 2 (Médico).")]
        public int TipUsuario { get; set; }
    }
}

