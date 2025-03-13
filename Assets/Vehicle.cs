using System.Collections;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public string VehicleId { get; private set; }
    public string VehicleType { get; private set; }
    public Node CurrentNode { get; private set; }

    public Animator animator;

    public void Initialize(string vehicleId, string vehicleType, Node startNode)
    {
        VehicleId = vehicleId;
        VehicleType = vehicleType;
        CurrentNode = startNode;
        animator = GetComponent<Animator>();
    }

    public IEnumerator MoveTo(Node targetNode, float timeToMove)
    {
        if (targetNode == null) yield break;

        if (CurrentNode.isActionNode)
        {
            if (animator != null)
            {
                animator.SetTrigger("finished");
            }
        }

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = targetNode.transform.position;
        float elapsedTime = 0f;

        // Разворачиваем машинку мгновенно в сторону цели
        transform.forward = (targetPosition - startPosition).normalized;

        while (elapsedTime < timeToMove)
        {
            elapsedTime += Time.deltaTime;

            // Двигаем машинку прямо к точке назначения
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / timeToMove);

            yield return null;
        }

        // Устанавливаем финальную позицию 
        transform.position = targetPosition;
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        // Обновляем ноду
        CurrentNode.RemoveVehicle(this);
        CurrentNode = targetNode;
        CurrentNode.AddVehicle(this);
        if (CurrentNode.isActionNode)
        {
            if (animator != null)
            {
                animator.SetTrigger("started");
            }
        }
    }
}
