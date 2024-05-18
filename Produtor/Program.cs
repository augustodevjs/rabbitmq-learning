using System.Text;
using RabbitMQ.Client;

namespace Produtor;

internal class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "round-robin",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        int count = 0;

        while(true)
        {
            string message = $"OrderNumber: {count++}";

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: "round-robin",
                basicProperties: null,
                body: body
            );

            Console.WriteLine($" [x] Sent {message}");
            Thread.Sleep(2000);
        }
    }
}
