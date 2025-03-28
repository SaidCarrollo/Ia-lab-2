using UnityEngine;

public class AgentKeyboardController : MonoBehaviour
{
    [Header("Teclas para Comportamientos")]
    public KeyCode seekKey = KeyCode.Alpha1;
    public KeyCode fleeKey = KeyCode.Alpha2;
    public KeyCode flockKey = KeyCode.Alpha3;
    public KeyCode wanderKey = KeyCode.Alpha4;
    public KeyCode obstacleAvoidanceKey = KeyCode.Alpha5;

    [Header("Modo de Control")]
    public bool controlIndividual = false;
    public KeyCode selectAgentKey = KeyCode.Tab;

    private Agent selectedAgent;

    void Update()
    {
        // Seleccionar agente individual (opcional)
        if (controlIndividual && Input.GetKeyDown(selectAgentKey))
        {
            SelectNextAgent();
        }

        // Cambiar comportamientos
        if (Input.GetKeyDown(seekKey))
        {
            SetBehavior(TypeSteeringBehavior.Seek);
        }
        else if (Input.GetKeyDown(fleeKey))
        {
            SetBehavior(TypeSteeringBehavior.Flee);
        }
        else if (Input.GetKeyDown(flockKey))
        {
            SetBehavior(TypeSteeringBehavior.Flock);
        }
        else if (Input.GetKeyDown(wanderKey))
        {
            SetBehavior(TypeSteeringBehavior.Wander);
        }
        else if (Input.GetKeyDown(obstacleAvoidanceKey))
        {
            SetBehavior(TypeSteeringBehavior.ObstacleAvoidance);
        }
    }

    private void SelectNextAgent()
    {
        Agent[] agents = FindObjectsOfType<Agent>();
        if (agents.Length == 0) return;

        if (selectedAgent == null)
        {
            selectedAgent = agents[0];
        }
        else
        {
            int currentIndex = System.Array.IndexOf(agents, selectedAgent);
            int nextIndex = (currentIndex + 1) % agents.Length;
            selectedAgent = agents[nextIndex];
        }
        Debug.Log($"Agente seleccionado: {selectedAgent.agentName}");
    }

    private void SetBehavior(TypeSteeringBehavior behavior)
    {
        if (controlIndividual && selectedAgent != null)
        {
            // Cambiar solo el agente seleccionado
            selectedAgent.ChangeBehavior(behavior);
        }
        else
        {
            // Cambiar todos los agentes
            Agent[] agents = FindObjectsOfType<Agent>();
            foreach (Agent agent in agents)
            {
                agent.ChangeBehavior(behavior);
            }
            Debug.Log($"Todos los agentes cambiaron a: {behavior}");
        }
    }
}