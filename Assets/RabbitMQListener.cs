using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Unity.VisualScripting;
using UnityEngine;

public class RabbitMQListener : MonoBehaviour
{
    private const string HostName = "kebnekaise.lmq.cloudamqp.com";
    private const string UserName = "xknvjswf";
    private const string Password = "RP_J3HdB3aAkig08yUF9abygQrNVyHwz";
    private const string VirtualHost = "xknvjswf";
    private const string QueueName = "events_queue";

    private IConnection connection;
    private IModel channel;

    void Start()
    {
        ConnectToRabbitMQ();
    }

    void ConnectToRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri("amqps://" + UserName + ":" + Password + "@" + HostName + "/" + VirtualHost),
                DispatchConsumersAsync = true
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Debug.Log("[RabbitMQ] Received: " + message);

                // Здесь можно добавить обработку команд

                await System.Threading.Tasks.Task.Yield();
            };

            channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);

            Debug.Log("[RabbitMQ] Connected and listening to " + QueueName);
        }
        catch (Exception ex)
        {
            Debug.LogError("[RabbitMQ] Connection error: " + ex.Message);
        }
    }

    void OnApplicationQuit()
    {
        channel?.Close();
        connection?.Close();
    }
}
