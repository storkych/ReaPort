using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public string NodeId;
    public bool isActionNode;
    public Color GizmoColor = Color.green;
    public List<Node> connectedNodes = new List<Node>(); // Связанные узлы
    private List<Vehicle> occupyingVehicles = new List<Vehicle>(); // Машины в этой ноде

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawSphere(transform.position, 1f); // Визуализация узлов

        foreach (Node node in connectedNodes)
        {
            if (node != null)
            {
                Gizmos.color = GizmoColor; // Устанавливаем цвет линии как у текущего узла
                Gizmos.DrawLine(transform.position, node.transform.position);
            }
        }
    }

    // Добавление транспорта в ноду
    public void AddVehicle(Vehicle vehicle)
    {
        if (!occupyingVehicles.Contains(vehicle))
        {
            occupyingVehicles.Add(vehicle);
        }
    }

    // Удаление транспорта из ноды
    public void RemoveVehicle(Vehicle vehicle)
    {
        if (occupyingVehicles.Contains(vehicle))
        {
            occupyingVehicles.Remove(vehicle);
        }
    }

    // Проверка, есть ли место для нового транспорта
    public bool IsSlotAvailable()
    {
        return occupyingVehicles.Count == 0; 
    }

    // Получение списка транспорта в ноде
    public List<Vehicle> GetOccupyingVehicles()
    {
        return occupyingVehicles;
    }

    public void ClearOccupyingVehicles()
    {
        occupyingVehicles.Clear();
    }
}
