using System.Text;
using RabbitMQ.Client;

namespace RProducer;

public class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "queue-example-basic",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        int count = 0;

        while(true)
        {
            string message = $"{count++} Hello World!";
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: "queue-example-basic",
                basicProperties: null,
                body: body
            );

            Console.WriteLine($" [x] Sent {message}");
            Thread.Sleep(200);
        }
    }
}
