// SmtpEmailSender.cs
using API_Sebben_que_é_agenda.Interfaces;
using API_Sebben_que_é_agenda.Models;
using API_Sebben_que_é_agenda.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Mail;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _opt;
    private readonly ILogger<SmtpEmailSender> _log;

    public SmtpEmailSender(IOptions<SmtpOptions> opt, ILogger<SmtpEmailSender> log)
    {
        _opt = opt.Value;
        _log = log;
    }

    public async Task SendConsultaConfirmacao(Consulta c, CancellationToken ct = default)
    {
        // Obtém email do paciente (ajuste se seu modelo usar outro nome de propriedade)
        var pacienteEmail = c.Paciente?.Email;
        var pacienteNome = c.Paciente?.Nome ;

        if (string.IsNullOrWhiteSpace(pacienteEmail))
        {
            _log.LogWarning("Consulta {Id}: paciente sem e-mail. Notificação não enviada.", c.Id);
            return;
        }

        var medicoNome = c.Medico?.Nome;
        var especialidade = c.Medico?.Especialidade?.Nome;
        var dataHoraStr = c.DataHora.ToString("dd/MM/yyyy HH:mm");

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_opt.FromName, _opt.From));
        msg.To.Add(new MailboxAddress(pacienteNome ?? "", pacienteEmail));
        msg.Subject = $"Confirmação de Consulta - {dataHoraStr}";

        var body = $@"
        Olá{(string.IsNullOrWhiteSpace(pacienteNome) ? "" : $" {pacienteNome}")},

        Sua consulta foi agendada.

        Médico: {medicoNome}
        Especialidade: {especialidade}
        Data/Hora: {dataHoraStr}
        {(string.IsNullOrWhiteSpace(c.Observacoes) ? "" : $"Observações: {c.Observacoes}")}

        Qualquer dúvida, responda este e-mail.
        Att,
        {_opt.FromName}
        ".Trim();

        msg.Body = new TextPart("plain") { Text = body };

        using var client = new MailKit.Net.Smtp.SmtpClient();
        try
        {
            SecureSocketOptions sslOpt =
                _opt.UseSsl ? SecureSocketOptions.SslOnConnect :
                _opt.UseStartTls ? SecureSocketOptions.StartTls :
                SecureSocketOptions.Auto;

            await client.ConnectAsync(_opt.Host, _opt.Port, sslOpt, ct);

            if (!string.IsNullOrWhiteSpace(_opt.User))
                await client.AuthenticateAsync(_opt.User, _opt.Password, ct);

            await client.SendAsync(msg, ct);
        }
        finally
        {
            await client.DisconnectAsync(true, ct);
        }
    }
}
