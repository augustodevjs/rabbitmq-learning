using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConsumerPrefetch;

public class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        using (var connection = factory.CreateConnection())
        {
            var channel = connection.CreateModel();

            var queueName = "order-prefetch";

            channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            BuildAndRunWorker(channel, "Worker A", queueName);
            BuildAndRunWorker(channel, "Worker B", queueName);

            Console.ReadLine();
        }
    }

    public static void BuildAndRunWorker(IModel channel, string workerName, string queueName)
    {
        //  BasicQos é  usado para controlar o fluxo de mensagens em um sistema de filas RabbitMQ, limitando o
        //  número de mensagens que um consumidor pode receber antes de confirmar o processamento (ack). Evita
        //  sobrecarga do consumidor e realiza o balenceamento de carga dos consumidores.

        channel.BasicQos(0, 3, false); // global false per consume | global true per channel

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
