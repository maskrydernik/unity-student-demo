// Joshua_SimpleNPC.cs
// Press T to talk when near. 1/2 to choose.
using UnityEngine;

public class Joshua_SimpleNPC : MonoBehaviour
{
    [TextArea] public string greeting = "Hello.";
    [TextArea] public string option1 = "Help (+10 gold)";
    [TextArea] public string option2 = "Decline";

    bool inside = false;
    bool dialog = false;

    void Update()
    {
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

    void OnTriggerEnter(Collider other){ if (other.GetComponent<Arthur_WorldHPBar>()) { inside = true; GameGlue.I.Hint("Press T to talk."); } }
    void OnTriggerExit(Collider other){ if (other.GetComponent<Arthur_WorldHPBar>()) { inside = false; if (dialog){ dialog=false; GameGlue.I.Hint(""); } } }
}
