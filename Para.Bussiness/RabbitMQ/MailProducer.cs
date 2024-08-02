using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

namespace Para.Bussiness.RabbitMQ
{
    public class MailProducer
    {
        private readonly RabbitMQClient _rabbitMQClient;

        public MailProducer(IConfiguration configuration)
        {
            var rabbitMqSettings = configuration.GetSection("RabbitMQ");

            // RabbitMQ ayarlarını kontrol edin
            if (rabbitMqSettings == null)
            {
                throw new ArgumentException("RabbitMQ ayarları yapılandırılmamış.");
            }

            if (string.IsNullOrEmpty(rabbitMqSettings["Hostname"]) ||
                string.IsNullOrEmpty(rabbitMqSettings["Port"]) ||
                string.IsNullOrEmpty(rabbitMqSettings["UserName"]) ||
                string.IsNullOrEmpty(rabbitMqSettings["Password"]) ||
                string.IsNullOrEmpty(rabbitMqSettings["QueueName"]))
            {
                throw new ArgumentException("RabbitMQ ayarlarında eksik değerler var.");
            }

            int port;
            try
            {
                port = int.Parse(rabbitMqSettings["Port"]);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Geçersiz port numarası.", ex);
            }

            _rabbitMQClient = new RabbitMQClient(
                rabbitMqSettings["Hostname"],
                port,
                rabbitMqSettings["UserName"],
                rabbitMqSettings["Password"],
                rabbitMqSettings["QueueName"]
            );
        }

        public void QueueEmail(string subject, string email, string content)
        {
            var emailMessage = $"{subject}|{email}|{content}";

            // RabbitMQ'ya mesaj gönderirken oluşan hataları yakalayın
            try
            {
                _rabbitMQClient.SendMessage(emailMessage);
            }
            catch (Exception ex)
            {
                // Hata mesajını yazdırın veya alternatif işlem yapın
                Console.WriteLine($"RabbitMQ'ya mesaj gönderilirken hata oluştu: {ex.Message}");
            }
        }
    }
}