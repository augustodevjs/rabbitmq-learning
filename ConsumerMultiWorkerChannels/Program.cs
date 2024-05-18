using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConsumerMultiWorkerChannels;

public class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        using (var connection = factory.CreateConnection())
        {

            for (var index1 = 0; index1 < 2; index1++)
            {
                var channel = CreateChannel(connection);

                channel.QueueDeclare(
                    queue: "order-multiple-channels",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                for (var index2 = 0; index2 < 7; index2++)
                {
                    BuildAndRunWorkers(channel, $"Worker {index2}");
                }
            }

            Console.ReadLine();
        }
    }

    public static IModel CreateChannel(IConnection connection)
    {
        var channel = connection.CreateModel();

        return channel;
    }

    public static void BuildAndRunWorkers(IModel channel, string workerName)
    {
        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"{channel.ChannelNumber} {workerName} [x] Received {message} Thread: {Environment.CurrentManagedThreadId}");
        };

        channel.BasicConsume(
            queue: "order-multiple-channels",
            autoAck: false,
            consumer: consumer
        );
    }
}
