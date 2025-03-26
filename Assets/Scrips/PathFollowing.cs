using UnityEngine;

public class PathFollowing : MonoBehaviour
{
    public Transform[] pathPoints; // Puntos que conforman la ruta
    public float speed = 5f;       // Velocidad de movimiento
    public float reachThreshold = 0.5f; // Distancia mínima para considerar que se alcanzó un punto

    private int currentPointIndex = 0;

    void Update()
    {
        if (pathPoints.Length == 0)
            return;

        Vector3 targetPosition = pathPoints[currentPointIndex].position;
        targetPosition.y = transform.position.y; // Mantener el movimiento en el plano XZ

        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rotar gradualmente hacia la dirección del movimiento
        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, speed * Time.deltaTime * 100);
        }

        if (Vector3.Distance(transform.position, targetPosition) < reachThreshold)
        {
            currentPointIndex = (currentPointIndex + 1) % pathPoints.Length;
        }
    }
}
