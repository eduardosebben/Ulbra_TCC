namespace API_Sebben_que_é_agenda.DTOs
{
    public class ConsultaCreateDto
    {
        public int PacienteId { get; set; }
        public int MedicoId { get; set; }
        public DateTime DataHora { get; set; }
        public string? Observacoes { get; set; }
    }

    public sealed class ConsultaUpdateDto
    {
        public int Id { get; set; }
        public int MedicoId { get; set; }
        public int PacienteId { get; set; }
        public DateTime DataHora { get; set; }
        public string? Observacoes { get; set; }
    }

    public sealed class ConsultasGetDto
    {
        public int? MedicoId { get; set; }
        public int? PacienteId { get; set; }
        public DateOnly? Data { get; set; }
    }

    public sealed class ConsultaRespDto
    {
        public int Id { get; set; }
        public DateTime DataHora { get; set; }
        public string? Observacoes { get; set; }
        public int MedicoId { get; set; }
        public int PacienteId { get; set; }
        public string MedicoNome { get; set; } = "";
        public string Especialidade { get; set; } = "";
        public string PacienteNome { get; set; } = "";
    }

}
