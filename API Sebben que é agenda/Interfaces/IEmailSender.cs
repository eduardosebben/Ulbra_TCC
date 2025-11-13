using API_Sebben_que_é_agenda.Models;

namespace API_Sebben_que_é_agenda.Interfaces
{
    public interface IEmailSender
    {
        Task SendConsultaConfirmacao(Consulta consulta, CancellationToken ct = default);
    }
}
