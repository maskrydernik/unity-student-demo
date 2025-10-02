// Christopher_RaiderQuest.cs
// Count raider kills up to a goal. Then disable a spawner object.
using System.Collections;
using UnityEngine;

public class Christopher_RaiderQuest : MonoBehaviour
{
    public static Christopher_RaiderQuest I;

    [Header("Quest Settings")]
    public string raiderTag = "Raider";
    public int goal = 7;
    public int count = 0;
    public string questLabel = "Raiders";
    public string completionMessage = "All raiders defeated!";
    public GameObject spawnerRoot;

    void Awake()
    {
        I = this;
    }

    IEnumerator Start()
    {
        // Wait until HUD wiring provides a quest text field.
        while (GameGlue.I == null || GameGlue.I.questText == null)
        {
            yield return null;
        }
        RefreshQuestText();
    }

    public void NotifyEnemyDeath(GameObject enemy)
    {
        if (enemy.CompareTag(raiderTag))
        {
            count = Mathf.Min(goal, count + 1);
            RefreshQuestText();

            if (count >= goal)
            {
                if (spawnerRoot) spawnerRoot.SetActive(false);
                if (GameGlue.I) GameGlue.I.Hint(completionMessage);
            }
        }
    }

    void RefreshQuestText()
    {
        if (GameGlue.I == null || GameGlue.I.questText == null)
        {
            return;
        }

        string questLine = questLabel + ": " + count + "/" + goal;
        if (count >= goal)
        {
            questLine += "\n" + completionMessage;
        }

        GameGlue.I.questText.text = questLine;
    }
}
