// Unit.cs
// Minimal selection highlight and movement wrapper. Works with NavMeshAgent.
// Replaces old Unit.cs. (Kept SetSelected and MoveTo API for continuity)

using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public GameObject highlight;    // child object used as selection indicator
    public NavMeshAgent agent;

    public void SetSelected(bool v)
    {
        highlight.SetActive(v);
    }

    public void MoveTo(Vector3 world)
    {
        agent.SetDestination(world);
    }
}
