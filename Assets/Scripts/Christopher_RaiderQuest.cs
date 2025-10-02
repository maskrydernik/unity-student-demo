// Christopher_RaiderQuest.cs
// Count raider kills up to a goal. Then disable a spawner object.
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Christopher_RaiderQuest : MonoBehaviour
{
    public static Christopher_RaiderQuest I;

    [Header("Quest Settings")]
    public string raiderTag = "Raider";
    public int goal = 7;
    public int count = 0;
    public string questLabel = "Raiders";
    public string completionMessage = "All raiders defeated!";
    public GameObject raiderPrefab;
    public Transform spawnPoint;
    public float secondsBetweenSpawns = 30f;
    public Transform evacuationTarget;

    bool questComplete;
    readonly System.Collections.Generic.List<GameObject> activeRaiders = new System.Collections.Generic.List<GameObject>();

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
        StartCoroutine(SpawnRaiders());
    }

    public void NotifyEnemyDeath(GameObject enemy)
    {
        if (enemy.CompareTag(raiderTag))
        {
            count = Mathf.Min(goal, count + 1);
            activeRaiders.Remove(enemy);
            RefreshQuestText();

            if (count >= goal)
            {
                questComplete = true;
                if (GameGlue.I) GameGlue.I.Hint(completionMessage);
                SendRemainingRaidersToExit();
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

    IEnumerator SpawnRaiders()
    {
        while (!questComplete)
        {
            yield return new WaitForSeconds(secondsBetweenSpawns);
            if (questComplete)
            {
                yield break;
            }
            SpawnOneRaider();
        }
    }

    void SpawnOneRaider()
    {
        if (raiderPrefab == null)
        {
            return;
        }

        Transform spawnTransform = spawnPoint != null ? spawnPoint : transform;
        GameObject newRaider = Instantiate(raiderPrefab, spawnTransform.position, spawnTransform.rotation);
        activeRaiders.Add(newRaider);
    }

    void SendRemainingRaidersToExit()
    {
        if (evacuationTarget == null)
        {
            return;
        }

        foreach (GameObject raider in activeRaiders.ToArray())
        {
            if (raider == null)
            {
                continue;
            }

            Vector3 destination = evacuationTarget.position;

            Unit unit = raider.GetComponent<Unit>();
            if (unit != null)
            {
                unit.MoveTo(destination);
            }
            else
            {
                NavMeshAgent agent = raider.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.SetDestination(destination);
                }
            }

            StartCoroutine(DestroyRaiderWhenArrived(raider, destination));
        }

        activeRaiders.Clear();
    }

    IEnumerator DestroyRaiderWhenArrived(GameObject raider, Vector3 destination)
    {
        if (raider == null)
        {
            yield break;
        }

        while (raider != null)
        {
            float distance = Vector3.Distance(raider.transform.position, destination);
            if (distance <= 1f)
            {
                Destroy(raider);
                yield break;
            }
            yield return null;
        }
    }
}
