using System.Text;
using RabbitMQ.Client;
using ProdutorExchangeDirect;
using RabbitMQ.Client.Events;

namespace ConsumidorExchangeDirect;

public class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        {
            var channel = connection.CreateModel();

            var queueName = "order";

            channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            BuildAndRunWorker(channel, $"Worker A1", "order");
            BuildAndRunWorker(channel, $"Worker A2", "order");
            BuildAndRunWorker(channel, $"Worker A3", "order");
            BuildAndRunWorker(channel, $"Worker B4", "finance_orders");
            BuildAndRunWorker(channel, $"Worker B5", "finance_orders");
            BuildAndRunWorker(channel, $"Worker B6", "finance_orders");

            Console.ReadLine();
        }
    }

    public static void BuildAndRunWorker(IModel channel, string workerName, string queueName)
    {
        channel.BasicQos(0, 7, false);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                var order = System.Text.Json.JsonSerializer.Deserialize<Order>(message);

                Console.WriteLine(
                    $"{channel.ChannelNumber} - {queueName}- {workerName}: [x] Order Id {order.Id} | {order.Amount} | {order.CreateDate:s} | {order.LastUpdated:s}");

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }
}
