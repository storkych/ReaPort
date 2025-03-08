using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public string NodeId;
    public Color GizmoColor = Color.green;
    public List<Node> connectedNodes = new List<Node>(); // Связанные узлы

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawSphere(transform.position, 1f); // Визуализация узлов

        Gizmos.color = Color.yellow;
        foreach (Node node in connectedNodes)
        {
            if (node != null)
            {
                Gizmos.DrawLine(transform.position, node.transform.position); // Линии между нодами
            }
        }
    }
}
