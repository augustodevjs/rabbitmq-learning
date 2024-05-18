using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumidor_Ack_Nack_AutoAck;

public class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        using (var connection = factory.CreateConnection())
        {

            var channel = connection.CreateModel();

            var queueName = "order-ack-nack-autoack";

            channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            BuildAndRunWorker(channel, "Worker A", queueName);

            Console.ReadLine();
        }
    }

    public static void BuildAndRunWorker(IModel channel, string workerName, string queueName)
    {
        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"{channel.ChannelNumber} - {workerName} [x] Received {message}");

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer
        );
    }
}
