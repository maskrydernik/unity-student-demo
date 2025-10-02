// Christopher_RaiderQuest.cs
// Count raider kills up to a goal. Then disable a spawner object.
using UnityEngine;

public class Christopher_RaiderQuest : MonoBehaviour
{
    public static Christopher_RaiderQuest I;
    public string raiderTag = "Raider";
    public int goal = 7;
    public int count = 0;
    public GameObject spawnerRoot;

    void Awake(){ I = this; }

    public void NotifyEnemyDeath(GameObject enemy)
    {
        if (enemy.CompareTag(raiderTag))
        {
            count = Mathf.Min(goal, count + 1);
            if (GameGlue.I.questText) GameGlue.I.questText.text = "Raiders: " + count + "/" + goal;
            if (count >= goal && spawnerRoot) spawnerRoot.SetActive(false);
        }
    }
}
