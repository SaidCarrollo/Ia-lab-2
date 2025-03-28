using UnityEngine;

public enum TypeSteeringBehavior
{
    Seek,
    Flee,
    Arrive,
    Pursuit,
    Evade,
    Wander,
    ObstacleAvoidance,
    Separation,
    Cohesion,
    Alignment,
    Flock
}

public class Agent : MonoBehaviour
{
    [Header("Basic Settings")]
    public TypeSteeringBehavior type;
    public Transform target;
    public float maxSpeed = 10f;
    public float maxForce = 10f;
    public float slowingRadius = 5f;

    [Header("Wander Settings")]
    public float wanderRadius = 1f;
    public float wanderDistance = 5f;
    public float wanderJitter = 0.2f;

    [Header("Obstacle Avoidance")]
    public float obstacleAvoidanceDistance = 10f;
    public LayerMask obstacleMask;

    private Vector3 _velocity;
    private Vector3 _wanderTarget;
    private float _initialY;

    // Public properties for controlled access
    public Vector3 Velocity => _velocity;
    public float MaxSpeed => maxSpeed;
    public string agentName = "Agent";
    void Start()
    {
        InitializeWander();
        _initialY = transform.position.y;
        AgentManager.Register(this);
    }

    void OnDestroy()
    {
        AgentManager.Unregister(this);
    }

    void Update()
    {
        Vector3 steeringForce = CalculateSteeringForce();
        ApplyForce(steeringForce);
    }
    public void ChangeBehavior(TypeSteeringBehavior newBehavior)
    {
        type = newBehavior;
        Debug.Log($"{agentName} cambió a: {newBehavior}");
    }
    private void InitializeWander()
    {
        _wanderTarget = Random.insideUnitSphere * wanderRadius;
        _wanderTarget.y = 0; // Ensure wander is only on horizontal plane
    }

    private Vector3 CalculateSteeringForce()
    {
        if (target == null && type != TypeSteeringBehavior.Wander &&
            type != TypeSteeringBehavior.ObstacleAvoidance &&
            type != TypeSteeringBehavior.Separation &&
            type != TypeSteeringBehavior.Cohesion &&
            type != TypeSteeringBehavior.Alignment &&
            type != TypeSteeringBehavior.Flock)
        {
            return Vector3.zero;
        }

        return type switch
        {
            TypeSteeringBehavior.Seek => CalculateSeek(target.position),
            TypeSteeringBehavior.Flee => CalculateFlee(target.position),
            TypeSteeringBehavior.Arrive => CalculateArrive(target.position),
            TypeSteeringBehavior.Pursuit => CalculatePursuit(target),
            TypeSteeringBehavior.Evade => CalculateEvade(target),
            TypeSteeringBehavior.Wander => CalculateWander(),
            TypeSteeringBehavior.ObstacleAvoidance => CalculateObstacleAvoidance(),
            TypeSteeringBehavior.Separation => AgentManager.Instance.CalculateSeparation(this),
            TypeSteeringBehavior.Cohesion => AgentManager.Instance.CalculateCohesion(this),
            TypeSteeringBehavior.Alignment => AgentManager.Instance.CalculateAlignment(this),
            TypeSteeringBehavior.Flock => CalculateFlockBehavior(),
            _ => Vector3.zero
        };
    }

    private void ApplyForce(Vector3 force)
    {
        force.y = 0; // Restrict to horizontal plane
        _velocity += Vector3.ClampMagnitude(force, maxForce) * Time.deltaTime;
        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);
        transform.position += _velocity * Time.deltaTime;

        // Maintain original Y position
        transform.position = new Vector3(transform.position.x, _initialY, transform.position.z);

        if (_velocity.magnitude > 0.1f)
        {
            transform.forward = _velocity.normalized;
        }
    }

    public Vector3 CalculateSeek(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (targetPosition - transform.position).normalized * maxSpeed;
        return desiredVelocity - _velocity;
    }

    public Vector3 CalculateFlee(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (transform.position - targetPosition).normalized * maxSpeed;
        return desiredVelocity - _velocity;
    }

    public Vector3 CalculateArrive(Vector3 targetPosition)
    {
        Vector3 toTarget = targetPosition - transform.position;
        float distance = toTarget.magnitude;

        if (distance < slowingRadius)
        {
            float rampedSpeed = maxSpeed * (distance / slowingRadius);
            Vector3 desiredVelocity = toTarget.normalized * rampedSpeed;
            return desiredVelocity - _velocity;
        }
        return CalculateSeek(targetPosition);
    }

    public Vector3 CalculatePursuit(Transform target)
    {
        Vector3 targetVelocity = target.TryGetComponent<Agent>(out var agent) ? agent.Velocity : Vector3.zero;
        Vector3 toTarget = target.position - transform.position;
        float prediction = (maxSpeed > 0) ? toTarget.magnitude / maxSpeed : 0;
        Vector3 futurePosition = target.position + targetVelocity * prediction;
        return CalculateSeek(futurePosition);
    }

    public Vector3 CalculateEvade(Transform target)
    {
        Vector3 targetVelocity = target.TryGetComponent<Agent>(out var agent) ? agent.Velocity : Vector3.zero;
        Vector3 toTarget = target.position - transform.position;
        float prediction = (maxSpeed > 0) ? toTarget.magnitude / maxSpeed : 0;
        Vector3 futurePosition = target.position + targetVelocity * prediction;
        return CalculateFlee(futurePosition);
    }

    public Vector3 CalculateWander()
    {
        // Update wander target with jitter
        _wanderTarget += new Vector3(
            Random.Range(-wanderJitter, wanderJitter),
            0,
            Random.Range(-wanderJitter, wanderJitter)
        );

        _wanderTarget = _wanderTarget.normalized * wanderRadius;
        Vector3 targetLocal = _wanderTarget + Vector3.forward * wanderDistance;
        Vector3 targetWorld = transform.TransformPoint(targetLocal);
        return CalculateSeek(targetWorld);
    }

    public Vector3 CalculateObstacleAvoidance()
    {
        if (Physics.Raycast(transform.position, _velocity.normalized, out var hit,
            obstacleAvoidanceDistance, obstacleMask))
        {
            Vector3 avoidanceDir = FindBestAvoidanceDirection(hit);
            float strength = CalculateAvoidanceStrength(hit.distance);
            Vector3 avoidanceForce = avoidanceDir * strength;

            // Emergency braking if too close
            if (hit.distance < 0.5f)
            {
                transform.position -= _velocity.normalized * 0.1f;
            }

            return avoidanceForce;
        }
        return Vector3.zero;
    }

    private Vector3 FindBestAvoidanceDirection(RaycastHit hit)
    {
        Vector3[] rayDirections = {
            transform.right,
            -transform.right,
            transform.right + transform.forward * 0.5f,
            -transform.right + transform.forward * 0.5f
        };

        foreach (Vector3 dir in rayDirections)
        {
            if (!Physics.Raycast(transform.position, dir, obstacleAvoidanceDistance * 0.5f, obstacleMask))
            {
                return dir.normalized;
            }
        }
        return -_velocity.normalized; // Fallback: brake
    }

    private float CalculateAvoidanceStrength(float distanceToObstacle)
    {
        return maxForce * (1 - (distanceToObstacle / obstacleAvoidanceDistance));
    }

    private Vector3 CalculateFlockBehavior()
    {
        Vector3 separation = AgentManager.Instance.CalculateSeparation(this);
        Vector3 cohesion = AgentManager.Instance.CalculateCohesion(this);
        Vector3 alignment = AgentManager.Instance.CalculateAlignment(this);

        return separation + cohesion + alignment;
    }
}