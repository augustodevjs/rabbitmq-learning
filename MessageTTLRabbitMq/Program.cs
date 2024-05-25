using System.Text;
using RabbitMQ.Client;

namespace MessageTTLRabbitMq;

public class Program
{
    static void Main(string[] args)
    {
        var queueName = "test_time_to_live";

        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // se passar 20 segundos e não for consumida a mensagem do rabbitmq vai ser
            // deletado automaticamente
            var arguments = new Dictionary<string, object>
            {
                { "x-message-ttl", 20000 }
            };

            channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: arguments
             );
                
            var body = Encoding.UTF8.GetBytes($"Hello World! Data/Hora: {DateTime.Now}");

            channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
        }

        Console.WriteLine("Press [enter] to exit.");
        Console.ReadLine();
    }
}
