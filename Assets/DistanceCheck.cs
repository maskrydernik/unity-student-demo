using System.Runtime.InteropServices;
using UnityEngine;

public class DistanceCheck : MonoBehaviour
{
    public float talkRange = 2;
    private Transform best;
    
    void Update()
    {
        FindClosestListener();

    
    }

    Transform FindClosestListener()
    {
        float bestSqr = talkRange * talkRange;
        Transform best = null;
        foreach (var hp in FindObjectsByType<Arthur_WorldHPBar>(FindObjectsSortMode.None))
        {
            if (!hp) continue;
            float sqr = (hp.transform.position - transform.position).sqrMagnitude;
            if (sqr <= bestSqr)
            {
                bestSqr = sqr;
                best = hp.transform;
            }
        }
        return best;
    }
}
