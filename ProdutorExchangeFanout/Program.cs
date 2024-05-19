using System.Text;
using RabbitMQ.Client;

namespace ProdutorExchangeFanout;

public class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var manualResetEvent = new ManualResetEvent(false);

        manualResetEvent.Reset();

        using (var connection = factory.CreateConnection())
        {
            var channel1 = SetupChannel(connection);

            BuildPublishers(channel1, "Produtor A", manualResetEvent);

            manualResetEvent.WaitOne();
        }
    }

    public static IModel SetupChannel(IConnection connection)
    {
        var channel = connection.CreateModel();

        var logsQueue = "logs-exchange-fanout";
        var orderQueue = "order-exchange-fanout";
        var financeOrdersQueues = "finance-orders_exchange-fanout";

        channel.QueueDeclare(
            queue: orderQueue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        channel.QueueDeclare(
            queue: logsQueue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        channel.QueueDeclare(
            queue: financeOrdersQueues,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        channel.ExchangeDeclare("order", "fanout");

        channel.QueueBind(orderQueue, "order", string.Empty);
        channel.QueueBind(logsQueue, "order", string.Empty);
        channel.QueueBind(financeOrdersQueues, "order", string.Empty);

        return channel;
    }

    public static void BuildPublishers(IModel channel, string publisherName, ManualResetEvent manualResetEvent)
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

                    for (var index = 0; index < 100; index++)
                    {
                        string message = $"OrderNumber: {count++} from {publisherName}";
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish("order", string.Empty, null, body);
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
