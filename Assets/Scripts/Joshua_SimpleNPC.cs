// Joshua_SimpleNPC.cs
// Press T to talk when near. 1/2 to choose.
using UnityEngine;

public class Joshua_SimpleNPC : MonoBehaviour
{
    [TextArea] public string greeting = "Hello.";
    [TextArea] public string option1 = "Help (+10 gold)";
    [TextArea] public string option2 = "Decline";
    [Tooltip("Radius within which a unit can start dialog.")]
    public float talkRange = 3f;

    bool inside = false;
    bool dialog = false;
    Transform currentListener;

    void Update()
    {
        UpdateProximity();
        if (!inside) return;
        if (!dialog && Input.GetKeyDown(KeyCode.T))
        {
            dialog = true;
            GameGlue.I.Hint(greeting + " [1] " + option1 + "  [2] " + option2);
        }
        else if (dialog && Input.GetKeyDown(KeyCode.Alpha1))
        {
            dialog = false;
            GameGlue.I.AddGold(10);
            GameGlue.I.Hint("You gained 10 gold.");
        }
        else if (dialog && Input.GetKeyDown(KeyCode.Alpha2))
        {
            dialog = false;
            GameGlue.I.Hint("You walk away.");
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
            if (GameGlue.I) GameGlue.I.Hint("Press T to talk.");
        }
        else if (!inside && wasInside)
        {
            if (dialog)
            {
                dialog = false;
            }
            if (GameGlue.I) GameGlue.I.Hint("");
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
