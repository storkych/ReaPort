using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    public static VehicleManager Instance { get; private set; }

    // ������� ��� ������ ����� ����������
    public GameObject airplanePrefab; // ������ ��� �������
    public GameObject followMePrefab; // ������ ��� follow-me
    public GameObject cateringPrefab; // ������ ��� catering
    public GameObject refuelingPrefab; // ������ ��� refueling
    public GameObject cleaningPrefab; // ������ ��� cleaning
    public GameObject baggagePrefab; // ������ ��� baggage
    public GameObject chargingPrefab; // ������ ��� charging
    public GameObject busPrefab; // ������ ��� bus
    public GameObject rampPrefab; // ������ ��� ramp

    [SerializeField] private List<Node> nodeList = new List<Node>(); // ������ ���, ������� ����� ������������ � ����������

    private Dictionary<string, Vehicle> vehicles = new Dictionary<string, Vehicle>();
    private Dictionary<string, Node> nodes = new Dictionary<string, Node>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        RegisterNodes(nodeList); // ����������� ��� ����� �������������
    }

    public void RefreshMap()
    {
        foreach (Node node in nodeList)
        {
            node.ClearOccupyingVehicles();
        }
        foreach (var vehicle in vehicles.Values)
        {
            Destroy(vehicle.gameObject);
        }
        vehicles.Clear();
    }

    public void RegisterNodes(List<Node> nodeList)
    {
        nodes.Clear(); // �������� ������ ���� ����� ����������� �����

        foreach (var node in nodeList)
        {
            nodes[node.NodeId] = node;
        }
    }

    // ����� ����� ��� ��������� ���������� id ���
    private string GetCorrectNodeId(string nodeId, string vehicleType)
    {
        if (nodeId.StartsWith("garrage") && !nodeId.StartsWith("garrage_to") && !nodeId.StartsWith("garrage_from"))
        {
            return "garrage"; // ���, ��� ���������� � "garage", ����� ������������ �� �����
        }

        if (nodeId.StartsWith("parking"))
        {
            // ��������� nodeId �� �����, ����� �������� ��������� ������
            string[] parts = nodeId.Split('_');
            if (parts.Length < 3) return nodeId; // ���� ��������� �����������, ���������� ��� ����

            string baseParkingId = string.Join("_", parts.Take(parts.Length - 1)); // parking_1_catering
            string firstSlot = baseParkingId + "_1";
            string secondSlot = baseParkingId + "_2";

            // ��������� ��������� ������
            if (nodes.ContainsKey(firstSlot) && nodes[firstSlot].IsSlotAvailable())
            {
                return firstSlot; // ������ ���� ��������
            }
            else if (nodes.ContainsKey(secondSlot) && nodes[secondSlot].IsSlotAvailable())
            {
                return secondSlot; // ������ �����, �������� ������
            }
        }

        return nodeId; // ��� ������ ��� ���������� �� ��� ����
    }

    // ����� ��� ������ ������� �� ���� ������������� ��������
    private GameObject GetVehiclePrefab(string vehicleType)
    {
        switch (vehicleType)
        {
            case "airplane":
                return airplanePrefab;
            case "follow-me":
                return followMePrefab;
            case "catering":
                return cateringPrefab;
            case "refueling":
                return refuelingPrefab;
            case "cleaning":
                return cleaningPrefab;
            case "baggage":
                return baggagePrefab;
            case "charging":
                return chargingPrefab;
            case "bus":
                return busPrefab;
            case "ramp":
                return rampPrefab;
            default:
                Debug.LogError($"Unknown vehicle type: {vehicleType}");
                return null;
        }
    }

    public void SpawnVehicle(string nodeId, string vehicleId, string vehicleType)
    {
        nodeId = GetCorrectNodeId(nodeId, vehicleType); // �������� � ������������� nodeId ��� ������
        bool airstrip = false;
        if (nodeId == "airstrip")
        {
            nodeId += "_spawn";
            airstrip = true;
        }

        if (!nodes.ContainsKey(nodeId))
        {
            Debug.LogError($"Node {nodeId} not found!");
            return;
        }

        GameObject prefab = GetVehiclePrefab(vehicleType); // �������� ������ �� ���� ����������

        if (prefab == null)
        {
            return; // ���� ������ �� ������, ������� �� ������
        }

        GameObject vehicleObj = Instantiate(prefab, nodes[nodeId].transform.position, Quaternion.identity);

        var vehicle = vehicleObj.GetComponent<Vehicle>();
        if (vehicle != null)
        {
            vehicle.Initialize(vehicleId, vehicleType, nodes[nodeId]);
            vehicles[vehicleId] = vehicle;
            nodes[nodeId].AddVehicle(vehicle);
        }
        if (airstrip)
        {
            MoveVehicle(vehicleId, nodeId, "airstrip", 25f);
        }
    }

    public void MoveVehicle(string vehicleId, string fromNode, string toNode, float distance, string withAirplane = "")
    {
        toNode = GetCorrectNodeId(toNode, vehicles[vehicleId].VehicleType);

        if (toNode == "airstrip")
        {
            if (fromNode == "airplane_from_parking_1")
            {
                toNode = "airstrip_1";
            }
            else if (fromNode == "airplane_from_parking_2")
            {
                toNode = "airstrip_2";
            }
        }

        if (!vehicles.ContainsKey(vehicleId))
        {
            Debug.LogError($"Vehicle {vehicleId} not found!");
            return;
        }

        if (!nodes.ContainsKey(toNode))
        {
            Debug.LogError($"Target node {toNode} not found!");
            return;
        }

        var vehicle = vehicles[vehicleId];
        float timeToMove = distance / 25f; // ����� ��������� �� �������� (25 ��/���)

        vehicle.StartCoroutine(vehicle.MoveTo(nodes[toNode], timeToMove));
        if (!string.IsNullOrEmpty(withAirplane) && vehicles.ContainsKey(withAirplane))
        {
            var airplane = vehicles[withAirplane];
            airplane.StartCoroutine(airplane.DelayedMove(nodes[toNode], timeToMove));
        }
    }

    public void HandleTakeoff(string vehicleId)
    {
        if (!vehicles.ContainsKey(vehicleId))
        {
            Debug.LogError($"Vehicle {vehicleId} not found!");
            return;
        }

        var vehicle = vehicles[vehicleId];
        string currentNodeId = vehicle.CurrentNode.NodeId;
        string takeoffNodeId = "";

        if (currentNodeId == "airstrip_1")
        {
            takeoffNodeId = "takeoff_1";
        }
        else if (currentNodeId == "airstrip_2")
        {
            takeoffNodeId = "takeoff_2";
        }
        else
        {
            Debug.LogError($"Vehicle {vehicleId} is not at a valid airstrip node for takeoff!");
            return;
        }

        if (!nodes.ContainsKey(takeoffNodeId))
        {
            Debug.LogError($"Takeoff node {takeoffNodeId} not found!");
            return;
        }

        float timeToMove = 5f; // ����� ����������� � ������� ������
        vehicle.StartCoroutine(vehicle.MoveTo(nodes[takeoffNodeId], timeToMove));
        StartCoroutine(RemoveVehicleAfterDelay(vehicleId, 5f));
    }

    private IEnumerator RemoveVehicleAfterDelay(string vehicleId, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (vehicles.ContainsKey(vehicleId))
        {
            Destroy(vehicles[vehicleId].gameObject);
            vehicles.Remove(vehicleId);
            Debug.Log($"Vehicle {vehicleId} has taken off and been removed.");
        }
    }

}
