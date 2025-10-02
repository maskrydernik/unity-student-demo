// Christopher_RaiderQuest.cs
// Spawns raiders, tracks how many you defeat, and wraps up the quest.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Christopher_RaiderQuest : MonoBehaviour
{
    // Allow other scripts to find this quest easily.
    public static Christopher_RaiderQuest I;

    [Header("Quest Settings")]
    public string raiderTag = "Raider";
    public int goal = 7;
    public int count = 0;
    public string questLabel = "Raiders";
    public string completionMessage = "All raiders defeated!";

    [Header("Spawning")]
    public GameObject raiderPrefab;
    public Transform spawnPoint;
    public float secondsBetweenSpawns = 30f;

    bool questComplete = false;
    List<GameObject> activeRaiders = new List<GameObject>();

    void Awake()
    {
        I = this;
    }

    IEnumerator Start()
    {
        // The HUD may not be ready on the first frame, so wait for it.
        while (GameGlue.I == null || GameGlue.I.questText == null)
        {
            yield return null;
        }

        RegisterExistingRaiders();
        UpdateQuestText();
        StartCoroutine(SpawnLoop());
    }

    public void NotifyEnemyDeath(GameObject enemy)
    {
        if (!enemy.CompareTag(raiderTag))
        {
            return;
        }

        count = Mathf.Min(goal, count + 1);
        activeRaiders.Remove(enemy);
        UpdateQuestText();

        if (count < goal)
        {
            return;
        }

        questComplete = true;
        if (GameGlue.I != null)
        {
            GameGlue.I.Hint(completionMessage);
        }
    }

    void UpdateQuestText()
    {
        if (GameGlue.I == null || GameGlue.I.questText == null)
        {
            return;
        }

        string line = questLabel + ": " + count + "/" + goal;
        if (count >= goal)
        {
            line += "\n" + completionMessage;
        }

        GameGlue.I.questText.text = line;
    }

    IEnumerator SpawnLoop()
    {
        while (!questComplete)
        {
            yield return new WaitForSeconds(secondsBetweenSpawns);
            if (questComplete)
            {
                yield break;
            }

            RemoveNullRaiders();
            if (ShouldSpawnAnotherRaider())
            {
                SpawnRaider();
            }
        }
    }

    void SpawnRaider()
    {
        if (raiderPrefab == null)
        {
            return;
        }

        Transform spawnTransform = spawnPoint != null ? spawnPoint : transform;
        GameObject raider = Instantiate(raiderPrefab, spawnTransform.position, spawnTransform.rotation);
        activeRaiders.Add(raider);
    }

    void RegisterExistingRaiders()
    {
        activeRaiders.Clear();
        GameObject[] existingRaiders = GameObject.FindGameObjectsWithTag(raiderTag);
        foreach (GameObject raider in existingRaiders)
        {
            activeRaiders.Add(raider);
        }
    }

    bool ShouldSpawnAnotherRaider()
    {
        int aliveCount = activeRaiders.Count;
        int totalCreated = aliveCount + count;
        return totalCreated < goal;
    }

    void RemoveNullRaiders()
    {
        activeRaiders.RemoveAll(raider => raider == null);
    }
}
