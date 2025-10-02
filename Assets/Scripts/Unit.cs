// Unit.cs
// Minimal selection highlight and NavMeshAgent wrapper used by multiple scripts.
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public GameObject highlight;
    public NavMeshAgent agent;
    public bool IsSelected{ get; private set; }

    public void SetSelected(bool v){ IsSelected = v; if (highlight) highlight.SetActive(v); }
    public void MoveTo(Vector3 world){ if(agent) agent.SetDestination(world); }
}
