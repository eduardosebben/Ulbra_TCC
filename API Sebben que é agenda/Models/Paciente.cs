namespace API_Sebben_que_é_agenda.Models
{
    public class Paciente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int idUsuario { get; set; }
        public Usuario usuario { get; set; }
        public ICollection<Consulta> Consultas { get; set; }
    }
}
