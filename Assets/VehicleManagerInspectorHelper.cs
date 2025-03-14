using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleManagerInspectorHelper : MonoBehaviour
{
    public string vehicleId; // ID ������������� ��������
    public string vehicleType; // ��� ������������� ��������
    public string spawnNodeId; // ���� ��� ������
    public string moveFromNodeId; // ���� ��� ������ ��������
    public string moveToNodeId; // ���� ��� ��������� ��������
    public float moveDistance; // ��������� ��������
    public string withAirplaneId; // ID ������������� ��������, � ������� ������������� �������

    public void SpawnVehicle()
    {
        if (string.IsNullOrEmpty(spawnNodeId) || string.IsNullOrEmpty(vehicleId) || string.IsNullOrEmpty(vehicleType))
        {
            Debug.LogError("Please provide valid NodeId, VehicleId, and VehicleType.");
            return;
        }

        // ����� ������ ������ ������������� ��������
        VehicleManager.Instance.SpawnVehicle(spawnNodeId, vehicleId, vehicleType);
    }

    public void MoveVehicle()
    {
        if (string.IsNullOrEmpty(moveFromNodeId) || string.IsNullOrEmpty(moveToNodeId) || string.IsNullOrEmpty(vehicleId))
        {
            Debug.LogError("Please provide valid FromNodeId, ToNodeId, and VehicleId.");
            return;
        }

        // ����� ������ �������� ������������� ��������
        VehicleManager.Instance.MoveVehicle(vehicleId, moveFromNodeId, moveToNodeId, moveDistance, withAirplaneId);
    }

    // � ���������� ����� ������ ��� ������ � ��������
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
