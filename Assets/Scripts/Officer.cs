// GreetingOnlyNPC.cs
// Press T to talk when near. Only shows greeting.
using UnityEngine;

public class GreetingOnlyNPC : MonoBehaviour
{
    [TextArea] public string greeting = "Hello, traveler!";
    [Tooltip("Radius within which a unit can start dialog.")]
    public float talkRange = 3f;

    private bool inside = false;
    private Transform currentListener;

    void Update()
    {
        UpdateProximity();

        if (!inside)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (GameGlue.I != null)
            {
                GameGlue.I.dialogueText.gameObject.SetActive(true);
                GameGlue.I.ShowDialogue(greeting);
                GameGlue.I.Hint(""); // No options
            }
        }
    }

    void UpdateProximity()
    {
        Transform listener = FindClosestListener();
        bool wasInside = inside;
        inside = listener != null;
        currentListener = listener;

        if (inside && !wasInside)
        {
            if (GameGlue.I != null)
            {
                GameGlue.I.Hint("Press T to talk.");
            }
        }
        
        else if (!inside && wasInside)
        {
            if (GameGlue.I != null)
            {
                GameGlue.I.ShowDialogue("");
                GameGlue.I.dialogueText.gameObject.SetActive(false);
                GameGlue.I.Hint("");
            }
        }
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
