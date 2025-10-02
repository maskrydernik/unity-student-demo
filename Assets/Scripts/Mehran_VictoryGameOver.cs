// Mehran_VictoryGameOver.cs
// Victory if no enemies. Game over if no player units.
using UnityEngine;

public class Mehran_VictoryGameOver : MonoBehaviour
{
    public GameObject victoryPanel;
    public GameObject gameOverPanel;
    public float checkInterval = 1f;

    float t; bool ended;

    void Update()
    {
        if (ended) return;
        t -= Time.deltaTime; if (t > 0) return; t = checkInterval;

        int player=0, enemy=0;
        foreach (var ac in FindObjectsByType<Nicholas_AutoCombat>(FindObjectsSortMode.None))
        {
            if (ac.team == Nicholas_AutoCombat.Team.Player) player++; else enemy++;
        }

        if (player <= 0){ ended = true; if (gameOverPanel) gameOverPanel.SetActive(true); GameGlue.I.Hint("Game Over. Press R."); }
        else if (enemy <= 0){ ended = true; if (victoryPanel) victoryPanel.SetActive(true); GameGlue.I.Hint("Victory. Press R."); }
    }
}
