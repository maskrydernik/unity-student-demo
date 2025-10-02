// Ryan_ItemsAndKeys.cs
// One file: enemy drops -> key pickup -> locked door.
using UnityEngine;

public class Ryan_ItemsAndKeys : MonoBehaviour
{
    // Empty; this script is a carrier so the file is attributable to Ryan.
}

// Put this on enemies.
public class Ryan_SimpleDrop : MonoBehaviour
{
    public GameObject dropPrefab;
}

// Put this on key items.
public class Ryan_KeyPickup : MonoBehaviour
{
    public int amount = 1;
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Arthur_WorldHPBar>() != null)
        {
            GameGlue.I.AddKeys(amount);
            GameGlue.I.Hint("Picked up key x" + amount);
            Destroy(gameObject);
        }
    }
}

// Put this on doors.
public class Ryan_LockedDoor : MonoBehaviour
{
    public int keysRequired = 1;
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Arthur_WorldHPBar>() != null)
        {
            if (GameGlue.I.keys >= keysRequired)
            {
                GameGlue.I.AddKeys(-keysRequired);
                GameGlue.I.Hint("Unlocked");
                Destroy(gameObject);
            }
            else GameGlue.I.Hint("Need " + keysRequired + " key(s).");
        }
    }
}
