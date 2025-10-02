// Mehran_VictoryGameOver.cs
// Victory if no enemies. Game over if no player units.
using UnityEngine;

public class Mehran_VictoryGameOver : MonoBehaviour
{
    public GameObject victoryPanel;
    public GameObject gameOverPanel;
    public float checkInterval = 1f;

    float checkTimer;
    bool ended;

    void Update()
    {
        if (ended)
        {
            return;
        }

        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f)
        {
            return;
        }

        checkTimer = checkInterval;

        int playerCount = 0;
        int enemyCount = 0;
        foreach (var autoCombat in FindObjectsByType<Nicholas_AutoCombat>(FindObjectsSortMode.None))
        {
            if (autoCombat.team == Nicholas_AutoCombat.Team.Player)
            {
                playerCount++;
            }
            else
            {
                enemyCount++;
            }
        }

        if (playerCount <= 0)
        {
            ended = true;
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            GameGlue.I.Hint("Game Over. Press R.");
        }
        else if (enemyCount <= 0)
        {
            ended = true;
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
            GameGlue.I.Hint("Victory. Press R.");
        }
    }
}
