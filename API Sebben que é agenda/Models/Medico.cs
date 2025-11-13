namespace API_Sebben_que_é_agenda.Models
{
    public class Medico
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string CRM { get; set; }

        public int EspecialidadeId { get; set; }
        public int idUsuario { get; set; }  
        public Usuario usuario { get; set; }    
        public Especialidade Especialidade { get; set; }

        public ICollection<Consulta> Consultas { get; set; }
    }
}
