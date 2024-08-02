using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Para.Bussiness.Notification;

public class NotificationService : INotificationService
{
    private readonly IConfiguration _configuration;

    public NotificationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendEmail(string subject, string email, string content)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");

        // SMTP ayarlarýný kontrol edin
        if (smtpSettings == null)
        {
            throw new ArgumentException("SMTP ayarlarý yapýlandýrýlmamýþ.");
        }

        if (string.IsNullOrEmpty(smtpSettings["Host"]) ||
            string.IsNullOrEmpty(smtpSettings["Port"]) ||
            string.IsNullOrEmpty(smtpSettings["Username"]) ||
            string.IsNullOrEmpty(smtpSettings["Password"]) ||
            string.IsNullOrEmpty(smtpSettings["From"]))
        {
            throw new ArgumentException("SMTP ayarlarýnda eksik deðerler var.");
        }

        
        int port;
        try
        {
            port = int.Parse(smtpSettings["Port"]);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Geçersiz port numarasý.", ex);
        }

        SmtpClient smtpClient = new SmtpClient(smtpSettings["Host"])
        {
            Port = port,
            Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
            EnableSsl = true 
        };

        MailMessage mailMessage = new MailMessage
        {
            From = new MailAddress(smtpSettings["From"]),
            Subject = subject,
            Body = content,
            IsBodyHtml = true
        };

        mailMessage.To.Add(email);

        try
        {
            smtpClient.Send(mailMessage);
        }
        catch (SmtpException ex)
        {
            Console.WriteLine($"Mail gönderimi baþarýsýz oldu: {ex.Message}");
        }
    }
}