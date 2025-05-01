using System.Net;
using System.Net.Mail;

namespace Backend.TCC.PUCRS.Services;
public class EmailService
{
    private readonly string SmtpHost = "";
    private readonly int SmtpPort = 587; // Porta padrão para TLS
    private readonly string SmtpUser = "";
    private readonly string SmtpPass = "";

    private readonly bool UseSsl = false; // Define se deve usar SSL/TLS

    public void SendEmailAsync(string to, string subject, string body)
    {
        using var smtpClient = new SmtpClient(SmtpHost, SmtpPort)
        {
            Credentials = new NetworkCredential(SmtpUser, SmtpPass),
            EnableSsl = UseSsl
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(SmtpUser, "Meu Teste de envio de email"),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        mailMessage.To.Add(to);

        try
        {
            smtpClient.SendMailAsync(mailMessage);
            Console.WriteLine("Email enviado com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            throw;
        }
    }

    public void SendEmail2(string to, string subject, string body)
    {
        var fromAddress = new MailAddress("", "");
        var toAddress = new MailAddress(to, "");
        const string senha = "";

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, senha)
        };

        using (var mensagem = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body
        })
        {
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(fromAddress.Address, senha);
            smtp.Send(mensagem);
            smtp.SendCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    Console.WriteLine($"Erro ao enviar: {e.Error.Message}");
                }
                else
                {
                    Console.WriteLine("E-mail enviado com sucesso.");
                }
            };
        }
    }
}