using UnityEngine;
public enum TypeSteeringBehavior
{
    Seek,
    Flee,
    Arrive,
    Pursuit,
    Evade,
    Wander,
    ObstacleAvoidance
}

public class Agent : MonoBehaviour
{
    public TypeSteeringBehavior type;
    public Transform target;
    public float maxSpeed = 10f;
    public float maxForce = 10f;
    public float slowingRadius = 5f;
    public float wanderRadius = 1f;
    public float wanderDistance = 5f;
    public float wanderJitter = 0.2f;
    public float obstacleAvoidanceDistance = 5f;
    public LayerMask obstacleMask;

    private Vector3 velocity;
    private Vector3 wanderTarget;
    private float initialY;

    void Start()
    {
        wanderTarget = Random.insideUnitSphere * wanderRadius;
        initialY = transform.position.y;
    }

    void Update()
    {
        Vector3 steeringForce = Vector3.zero;

        switch (type)
        {
            case TypeSteeringBehavior.Seek:
                steeringForce = Seek(target.position);
                break;
            case TypeSteeringBehavior.Flee:
                steeringForce = Flee(target.position);
                break;
            case TypeSteeringBehavior.Arrive:
                steeringForce = Arrive(target.position);
                break;
            case TypeSteeringBehavior.Pursuit:
                steeringForce = Pursuit(target);
                break;
            case TypeSteeringBehavior.Evade:
                steeringForce = Evade(target);
                break;
            case TypeSteeringBehavior.Wander:
                steeringForce = Wander();
                break;
            case TypeSteeringBehavior.ObstacleAvoidance:
                steeringForce = ObstacleAvoidance();
                break;
        }

        ApplyForce(steeringForce);
    }

    void ApplyForce(Vector3 force)
    {
        force.y = 0; // Eliminar cualquier componente en el eje Y
        velocity += Vector3.ClampMagnitude(force, maxForce) * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, initialY, transform.position.z); // Mantener la posición Y constante

        if (velocity.magnitude > 0.1f)
        {
            transform.forward = velocity.normalized;
        }
    }

    Vector3 Seek(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (targetPosition - transform.position).normalized * maxSpeed;
        return desiredVelocity - velocity;
    }

    Vector3 Flee(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (transform.position - targetPosition).normalized * maxSpeed;
        return desiredVelocity - velocity;
    }

    Vector3 Arrive(Vector3 targetPosition)
    {
        Vector3 toTarget = targetPosition - transform.position;
        float distance = toTarget.magnitude;

        if (distance < slowingRadius)
        {
            float rampedSpeed = maxSpeed * (distance / slowingRadius);
            Vector3 desiredVelocity = toTarget.normalized * rampedSpeed;
            return desiredVelocity - velocity;
        }
        else
        {
            return Seek(targetPosition);
        }
    }

    Vector3 Pursuit(Transform target)
    {
        Vector3 targetVelocity = Vector3.zero;
        Agent targetAgent = target.GetComponent<Agent>();
        if (targetAgent != null)
        {
            targetVelocity = targetAgent.velocity;
        }

        Vector3 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;
        float prediction = (maxSpeed > 0) ? distance / maxSpeed : 0;
        Vector3 futurePosition = target.position + targetVelocity * prediction;
        return Seek(futurePosition);
    }

    Vector3 Evade(Transform target)
    {
        Vector3 targetVelocity = Vector3.zero;
        Agent targetAgent = target.GetComponent<Agent>();
        if (targetAgent != null)
        {
            targetVelocity = targetAgent.velocity;
        }

        Vector3 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;
        float prediction = (maxSpeed > 0) ? distance / maxSpeed : 0;
        Vector3 futurePosition = target.position + targetVelocity * prediction;
        return Flee(futurePosition);
    }

    Vector3 Wander()
    {
        wanderTarget += new Vector3(Random.Range(-wanderJitter, wanderJitter), 0, Random.Range(-wanderJitter, wanderJitter));
        wanderTarget = wanderTarget.normalized * wanderRadius;
        Vector3 targetLocal = wanderTarget + Vector3.forward * wanderDistance;
        Vector3 targetWorld = transform.TransformPoint(targetLocal);
        return Seek(targetWorld);
    }

    Vector3 ObstacleAvoidance()
    {
        RaycastHit hit;
        Vector3 ahead = transform.position + velocity.normalized * obstacleAvoidanceDistance;
        if (Physics.Raycast(transform.position, velocity.normalized, out hit, obstacleAvoidanceDistance, obstacleMask))
        {
            Vector3 avoidanceForce = ahead - hit.point;
            return avoidanceForce.normalized * maxForce;
        }
        return Vector3.zero;
    }
}