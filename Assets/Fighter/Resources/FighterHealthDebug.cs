using UnityEngine;

public class FighterHealthDebug : MonoBehaviour
{
    [Tooltip("Amount of damage to deal when pressing space")]
    public int damageAmount = 10;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DamageAllFighters();
        }
    }
    
    void DamageAllFighters()
    {
        BasicFighter2D[] allFighters = Object.FindObjectsByType<BasicFighter2D>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        int damagedCount = 0;
        foreach (var fighter in allFighters)
        {
            if (fighter.GetCurrentHP() > 0)
            {
                fighter.DebugReduceHP(damageAmount);
                damagedCount++;
            }
        }
        
        Debug.Log($"[Space Debug] Reduced HP of {damagedCount} fighters by {damageAmount} HP each");
    }
}
