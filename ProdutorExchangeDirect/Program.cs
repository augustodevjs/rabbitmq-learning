using System.Text;
using RabbitMQ.Client;
using System.Text.Json;

namespace ProdutorExchangeDirect;

class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var manualResetEvent = new ManualResetEvent(false);

        manualResetEvent.Reset();

        using (var connection = factory.CreateConnection())
        {
            var queueName = "order";

            var channel1 = SetupChannel(connection);

            BuildAndRunPublishers(channel1, queueName, manualResetEvent);

            manualResetEvent.WaitOne();
        }
    }

    public static IModel SetupChannel(IConnection connection)
    {
        var channel = connection.CreateModel();

        var queueOrder = "order";
        var queueFinanceOrders = "finance_orders";

        channel.QueueDeclare(queue: queueOrder, durable: false, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueDeclare(queue: queueFinanceOrders, durable: false, exclusive: false, autoDelete: false, arguments: null);

        channel.ExchangeDeclare("order", "direct");

        channel.QueueBind(queueOrder, "order", "order_new");
        channel.QueueBind(queueOrder, "order", "order_upd");
        channel.QueueBind(queueFinanceOrders, "order", "order_new");

        return channel;
    }

    public static void BuildAndRunPublishers(IModel channel, string queue, ManualResetEvent manualResetEvent)
    {
        Task.Run(() =>
        {
            var idIndex = 1;
            var randon = new Random(DateTime.UtcNow.Millisecond * DateTime.UtcNow.Second);

            while (true)
            {
                try
                {
                    Console.WriteLine("Pressione qualquer tecla para produzir mais mansagens");
                    Console.ReadLine();

                    var order = new Order(idIndex++, randon.Next(1000, 9999));
                    var message1 = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(order));

                    channel.BasicPublish(queue, "order_new", null, message1);

                    Console.WriteLine($"New order Id {order.Id}: Amount {order.Amount} | Created: {order.CreateDate:o}");

                    order.UpdateOrder(randon.Next(100, 999));
                    var message2 = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(order));

                    channel.BasicPublish(queue, "order_upd", null, message2);
                    Console.WriteLine($"Upd Id {order.Id}: Amount {order.Amount} | LastUpdated: {order.LastUpdated:o}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    manualResetEvent.Set();
                }
            }
        });
    }
}