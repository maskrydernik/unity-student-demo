// Unit.cs
// Minimal selection highlight and NavMeshAgent wrapper used by multiple scripts.
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour
{
    public GameObject highlight;
    public NavMeshAgent agent;
    public bool IsSelected { get; private set; }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (highlight != null)
        {
            highlight.SetActive(selected);
        }
    }

    public void MoveTo(Vector3 worldPosition)
    {
        if (agent != null)
        {
            agent.SetDestination(worldPosition);
        }
    }
}
