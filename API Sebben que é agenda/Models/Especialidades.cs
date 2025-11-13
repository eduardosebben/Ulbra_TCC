namespace API_Sebben_que_é_agenda.Models
{
    public class Especialidade
    {
        public int Id { get; set; }
        public string Nome { get; set; }

        public ICollection<Medico> Medicos { get; set; }
    }

}
