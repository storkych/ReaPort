using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UnityEngine;

public class RabbitMQListener : MonoBehaviour
{
    private const string QueueName = "events_queue";

    private IConnection connection;
    private IModel channel;

    private readonly Queue<Action> mainThreadActions = new Queue<Action>();

    void Start()
    {
        ConnectToRabbitMQ();
    }

    void Update()
    {
        // ¬ыполн€ем все действи€, добавленные в главный поток
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
            {
                mainThreadActions.Dequeue()?.Invoke();
            }
        }
    }

    void ConnectToRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri("amqp://jonh:123@rabbitmq.reaport.ru"),
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

                lock (mainThreadActions)
                {
                    mainThreadActions.Enqueue(() =>
                    {
                        Debug.Log("[RabbitMQ] Received: " + message);
                        UiManager.Instance.LogMessage(message);
                    });

                    mainThreadActions.Enqueue(() => ProcessMessage(message));
                }

                await System.Threading.Tasks.Task.Yield();
            };

            channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);

            lock (mainThreadActions)
            {
                mainThreadActions.Enqueue(() =>
                {
                    Debug.Log("[RabbitMQ] Connected and listening to " + QueueName);
                    UiManager.Instance.LogMessage("[RabbitMQ] Connected and listening to " + QueueName);
                });
            }
        }
        catch (Exception ex)
        {
            lock (mainThreadActions)
            {
                mainThreadActions.Enqueue(() =>
                {
                    UiManager.Instance.LogMessage("[RabbitMQ] Connection error: " + ex.Message);
                    Debug.LogError("[RabbitMQ] Connection error: " + ex.Message);
                });
            }
        }
    }

    void ProcessMessage(string message)
    {
        try
        {
            var json = JObject.Parse(message);
            var type = json["type"]?.ToString();
            var data = json["data"];

            if (type == "vehicle_registered")
            {
                string nodeId = data["garrage_node_id"]?.ToString();
                string vehicleId = data["vehicle_id"]?.ToString();
                string vehicleType = data["vehicle_type"]?.ToString();

                Debug.Log($"Spawning vehicle {vehicleId} ({vehicleType}) at node {nodeId}");

                VehicleManager.Instance.SpawnVehicle(nodeId, vehicleId, vehicleType);
            }
            else if (type == "vehicle_left_node")
            {
                string fromNode = data["from"]?.ToString();
                string toNode = data["to"]?.ToString();
                string vehicleId = data["vehicle_id"]?.ToString();
                float distance = float.Parse(data["distance"]?.ToString());

                Debug.Log($"Moving vehicle {vehicleId} from {fromNode} to {toNode}");

                VehicleManager.Instance.MoveVehicle(vehicleId, fromNode, toNode, distance);
            }
            else if (type == "map_refreshed")
            {
                VehicleManager.Instance.RefreshMap();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[RabbitMQ] Error processing message: " + ex.Message);
        }
    }

    void OnApplicationQuit()
    {
        channel?.Close();
        connection?.Close();
    }
}
