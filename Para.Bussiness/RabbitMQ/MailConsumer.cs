using Microsoft.Extensions.Configuration;
using Para.Bussiness.Notification;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Para.Bussiness.RabbitMQ
{
    public class MailConsumer
    {
        private readonly RabbitMQClient _rabbitMQClient;
        private readonly INotificationService _notificationService;

        public MailConsumer(IConfiguration configuration, INotificationService notificationService)
        {
            var rabbitMqSettings = configuration.GetSection("RabbitMQ");
            _notificationService = notificationService;
            _rabbitMQClient = new RabbitMQClient(
                rabbitMqSettings["Hostname"],
                int.Parse(rabbitMqSettings["Port"]),
                rabbitMqSettings["UserName"],
                rabbitMqSettings["Password"],
                rabbitMqSettings["QueueName"]
            );
        }

        public void StartListening()
        {
            var channel = _rabbitMQClient.GetChannel();
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Mesajı bölmek için bir hata kontrolü eklendi
                var messageParts = message.Split('|');
                if (messageParts.Length != 3)
                {
                    Console.WriteLine("Geçersiz mesaj formatı.");
                    return;
                }

                var emailSubject = messageParts[0];
                var recipientEmail = messageParts[1];
                var emailContent = messageParts[2];

                _notificationService.SendEmail(emailSubject, recipientEmail, emailContent);
            };

            channel.BasicConsume(queue: _rabbitMQClient.GetQueueName(),
                                 autoAck: true,
                                 consumer: consumer);
        }
    }
}
