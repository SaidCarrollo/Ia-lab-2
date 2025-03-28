using UnityEngine;
using System.Collections.Generic;

public class AgentManager : MonoBehaviour
{
    private static AgentManager _instance;
    public static AgentManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("AgentManager").AddComponent<AgentManager>();
                Debug.Log("AgentManager creado automáticamente");
            }
            return _instance;
        }
    }

    private readonly List<Agent> agents = new List<Agent>();

    [Header("Configuración de Comportamientos")]
    public float separationRadius = 2f;
    public float cohesionRadius = 5f;
    public float alignmentRadius = 5f;
    public float separationWeight = 1.5f;
    public float cohesionWeight = 1f;
    public float alignmentWeight = 1f;

    // Métodos de registro optimizados
    public static void Register(Agent agent)
    {
        Instance.agents.Add(agent);
    }

    public static void Unregister(Agent agent)
    {
        if (_instance != null)
            Instance.agents.Remove(agent);
    }

    // Métodos de comportamientos optimizados
    public Vector3 CalculateSeparation(Agent currentAgent)
    {
        Vector3 separationForce = Vector3.zero;
        int neighborsCount = 0;

        foreach (Agent other in agents)
        {
            if (other != currentAgent && other != null)
            {
                float distance = Vector3.Distance(currentAgent.transform.position, other.transform.position);
                if (distance < separationRadius)
                {
                    Vector3 awayVector = currentAgent.transform.position - other.transform.position;
                    separationForce += awayVector.normalized / Mathf.Max(distance, 0.1f);
                    neighborsCount++;
                }
            }
        }

        if (neighborsCount > 0)
        {
            separationForce /= neighborsCount;
            separationForce = separationForce.normalized * currentAgent.MaxSpeed;
            separationForce -= currentAgent.Velocity;
        }

        return separationForce * separationWeight;
    }

    public Vector3 CalculateCohesion(Agent currentAgent)
    {
        Vector3 centerOfMass = Vector3.zero;
        int neighborsCount = 0;

        foreach (Agent other in agents)
        {
            if (other != currentAgent && other != null)
            {
                float distance = Vector3.Distance(currentAgent.transform.position, other.transform.position);
                if (distance < cohesionRadius)
                {
                    centerOfMass += other.transform.position;
                    neighborsCount++;
                }
            }
        }

        if (neighborsCount > 0)
        {
            centerOfMass /= neighborsCount;
            return currentAgent.CalculateSeek(centerOfMass) * cohesionWeight;
        }

        return Vector3.zero;
    }

    public Vector3 CalculateAlignment(Agent currentAgent)
    {
        Vector3 averageVelocity = Vector3.zero;
        int neighborsCount = 0;

        foreach (Agent other in agents)
        {
            if (other != currentAgent && other != null)
            {
                float distance = Vector3.Distance(currentAgent.transform.position, other.transform.position);
                if (distance < alignmentRadius)
                {
                    averageVelocity += other.Velocity;
                    neighborsCount++;
                }
            }
        }

        if (neighborsCount > 0)
        {
            averageVelocity /= neighborsCount;
            averageVelocity = averageVelocity.normalized * currentAgent.MaxSpeed;
            return (averageVelocity - currentAgent.Velocity) * alignmentWeight;
        }

        return Vector3.zero;
    }
}