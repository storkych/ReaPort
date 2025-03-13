using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public string NodeId;
    public bool isActionNode;
    public Color GizmoColor = Color.green;
    public List<Node> connectedNodes = new List<Node>(); // ��������� ����
    private List<Vehicle> occupyingVehicles = new List<Vehicle>(); // ������ � ���� ����

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawSphere(transform.position, 1f); // ������������ �����

        foreach (Node node in connectedNodes)
        {
            if (node != null)
            {
                Gizmos.color = GizmoColor; // ������������� ���� ����� ��� � �������� ����
                Gizmos.DrawLine(transform.position, node.transform.position);
            }
        }
    }

    // ���������� ���������� � ����
    public void AddVehicle(Vehicle vehicle)
    {
        if (!occupyingVehicles.Contains(vehicle))
        {
            occupyingVehicles.Add(vehicle);
        }
    }

    // �������� ���������� �� ����
    public void RemoveVehicle(Vehicle vehicle)
    {
        if (occupyingVehicles.Contains(vehicle))
        {
            occupyingVehicles.Remove(vehicle);
        }
    }

    // ��������, ���� �� ����� ��� ������ ����������
    public bool IsSlotAvailable()
    {
        return occupyingVehicles.Count == 0; 
    }

    // ��������� ������ ���������� � ����
    public List<Vehicle> GetOccupyingVehicles()
    {
        return occupyingVehicles;
    }

    public void ClearOccupyingVehicles()
    {
        occupyingVehicles.Clear();
    }
}
