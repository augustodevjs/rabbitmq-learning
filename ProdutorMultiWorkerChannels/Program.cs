using System.Text;
using RabbitMQ.Client;

namespace ProdutorMultiWorkerChannels;

internal class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        {
            var queueName = "order-multiple-channels";

            var channel1 = CreateChannel(connection);
            var channel2 = CreateChannel(connection);

            BuildPublishers(channel1, queueName, "Produtor A");
            BuildPublishers(channel2, queueName, "Produtor B");

            Console.ReadLine();
        }
    }

    public static IModel CreateChannel(IConnection connection)
    {
        var channel = connection.CreateModel();

        return channel;
    }

    public static void BuildPublishers(IModel channel, string queue, string publisherName)
    {
        Task.Run(() =>
        {
            int count = 0;

            channel.QueueDeclare(
                queue: queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            while (true)
            {
                string message = $"OrderNumber: {count++} from {publisherName}";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish("", queue, null, body);

                Console.WriteLine($"{publisherName} - [x] Sent {count} - Thread: {Environment.CurrentManagedThreadId}", message);

                Thread.Sleep(1000);
            }
        });
    }
}
