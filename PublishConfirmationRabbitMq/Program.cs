using System.Text;
using RabbitMQ.Client;

namespace PublishConfirmationRabbitMq;

public class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            try
            {
                channel.ConfirmSelect();

                channel.BasicAcks += Channel_BasicAcks;
                channel.BasicNacks += Channel_BasicNacks;
                channel.BasicReturn += Channel_BasicReturn;

                channel.QueueDeclare(queue: "order",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string message = $"{DateTime.UtcNow:o} -> Hello World!";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "", // Default Exchange
                                     routingKey: "orderasjdalksjdk",
                                     basicProperties: null,
                                     body: body,
                                     mandatory: true);

                channel.WaitForConfirms(new TimeSpan(0, 0, 5));

                Console.WriteLine(" [x] Sent {0}", message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }

    private static void Channel_BasicAcks(object sender, RabbitMQ.Client.Events.BasicAckEventArgs e)
    {
        Console.WriteLine($"{DateTime.UtcNow:o} -> Basic Ack");
    }

    private static void Channel_BasicNacks(object sender, RabbitMQ.Client.Events.BasicNackEventArgs e)
    {
        Console.WriteLine($"{DateTime.UtcNow:o} -> Basic Nack");
    }

    private static void Channel_BasicReturn(object sender, RabbitMQ.Client.Events.BasicReturnEventArgs e)
    {
        var message = Encoding.UTF8.GetString(e.Body.ToArray());

        Console.WriteLine($"{DateTime.UtcNow:o} -> Basic Return -> Original message -> {message}");
    }
}
