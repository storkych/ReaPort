using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleManagerInspectorHelper : MonoBehaviour
{
    public string vehicleId; // ID транспортного средства
    public string vehicleType; // Тип транспортного средства
    public string spawnNodeId; // Узел для спавна
    public string moveFromNodeId; // Узел для старта движения
    public string moveToNodeId; // Узел для окончания движения
    public float moveDistance; // Дистанция движения
    public string withAirplaneId; // ID транспортного средства, с которым передвигается самолет

    public void SpawnVehicle()
    {
        if (string.IsNullOrEmpty(spawnNodeId) || string.IsNullOrEmpty(vehicleId) || string.IsNullOrEmpty(vehicleType))
        {
            Debug.LogError("Please provide valid NodeId, VehicleId, and VehicleType.");
            return;
        }

        // Вызов метода спавна транспортного средства
        VehicleManager.Instance.SpawnVehicle(spawnNodeId, vehicleId, vehicleType);
    }

    public void MoveVehicle()
    {
        if (string.IsNullOrEmpty(moveFromNodeId) || string.IsNullOrEmpty(moveToNodeId) || string.IsNullOrEmpty(vehicleId))
        {
            Debug.LogError("Please provide valid FromNodeId, ToNodeId, and VehicleId.");
            return;
        }

        // Вызов метода движения транспортного средства
        VehicleManager.Instance.MoveVehicle(vehicleId, moveFromNodeId, moveToNodeId, moveDistance, withAirplaneId);
    }

    // В инспекторе будут кнопки для спавна и движения
    [ContextMenu("Spawn Vehicle")]
    void SpawnVehicleContext()
    {
        SpawnVehicle();
    }

    [ContextMenu("Move Vehicle")]
    void MoveVehicleContext()
    {
        MoveVehicle();
    }
}
