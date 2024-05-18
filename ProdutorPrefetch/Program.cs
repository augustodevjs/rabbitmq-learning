using RabbitMQ.Client;
using System.Text;

namespace ProdutorPrefetch;

public class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var manualResetEvent = new ManualResetEvent(false);

        manualResetEvent.Reset();

        using (var connection = factory.CreateConnection())
        {
            var queueName = "order-prefetch";

            var channel1 = CreateChannel(connection, queueName);

            BuildPublishers(channel1, queueName, "Produtor A", manualResetEvent);

            manualResetEvent.WaitOne();
        }
    }

    public static IModel CreateChannel(IConnection connection, string queueName)
    {
        var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        return channel;
    }

    public static void BuildPublishers(IModel channel, string queue, string publisherName, ManualResetEvent manualResetEvent)
    {
        Task.Run(() =>
        {
            int count = 0;

            while (true)
            {
                try
                {
                    Console.WriteLine("Pressione qualquer tecla para produzir 100 msg");
                    Console.ReadLine();

                    for (var index = 0; index < 10; index++)
                    {
                        string message = $"OrderNumber: {count++} from {publisherName}";
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish("", queue, null, body);
                        Console.WriteLine($"{publisherName} - [x] Sent {count}", message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                    manualResetEvent.Set();
                }
            }
        });
    }
}
