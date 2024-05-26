using RabbitMQ.Client;
using System.Text;

namespace PublishPersistenceMessage;

public class Program
{
    static void Main(string[] args)
    {
        var queueName = "persistence-message";

        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
             );

            string message = "Hello world!";

            var body = Encoding.UTF8.GetBytes(message);

            var basicProp = channel.CreateBasicProperties();

            basicProp.Persistent = true;

            channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: basicProp, body: body);

            Console.WriteLine($"[x] sent {message}");
        }

        Console.WriteLine("Press [enter] to exit.");
        Console.ReadLine();
    }
}
