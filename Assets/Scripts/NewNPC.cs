// NewNPC.cs
// Press T to talk when near. 1/2 to choose.
using UnityEngine;

public class NewNPC : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea] public string greeting = "Hello, traveler!";
    [TextArea] public string option1 = "Ask for help";
    [TextArea] public string option2 = "Say goodbye";
    [TextArea] public string option1Result = "The NPC helps you.";
    [TextArea] public string option2Result = "You part ways.";

    [Header("Settings")]
    [Tooltip("Radius within which a unit can start dialog.")]
    public float talkRange = 3f;

    // Internal state
    private bool inside = false;
    private bool dialog = false;
    private Transform currentListener;

    void Update()
    {
        UpdateProximity();

        if (!inside)
            return;

        // Start dialogue
        if (!dialog && Input.GetKeyDown(KeyCode.T))
        {
            dialog = true;
            if (GameGlue.I != null)
            {
                GameGlue.I.dialogueText.gameObject.SetActive(true);
                GameGlue.I.ShowDialogue(greeting);
                GameGlue.I.Hint("[1] " + option1 + "  [2] " + option2);
            }
        }

        // Option 1
        else if (dialog && Input.GetKeyDown(KeyCode.Alpha1))
        {
            dialog = false;
            if (GameGlue.I != null)
            {
                GameGlue.I.ShowDialogue(option1Result);
                GameGlue.I.dialogueText.gameObject.SetActive(true);
                GameGlue.I.Hint("");
            }
        }

        // Option 2
        else if (dialog && Input.GetKeyDown(KeyCode.Alpha2))
        {
            dialog = false;
            if (GameGlue.I != null)
            {
                GameGlue.I.ShowDialogue(option2Result);
                GameGlue.I.dialogueText.gameObject.SetActive(true);
                GameGlue.I.Hint("");
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
                GameGlue.I.Hint("Press T to talk.");
        }
        else if (!inside && wasInside)
        {
            // Always clear dialogue and hint
            dialog = false;
            if (GameGlue.I != null)
            {
                GameGlue.I.ShowDialogue("");                // clear dialogue text
                GameGlue.I.dialogueText.gameObject.SetActive(false); // hide dialogue field
                GameGlue.I.Hint("");                        // clear tip/hint
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
