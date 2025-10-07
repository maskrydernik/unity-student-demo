// Yari_Retry.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Yari_Retry : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverScreen; // Assign in Inspector
    public TMP_Text gameOverMessage;  // Optional TMP text for Game Over text

    [Header("Player Reference")]
    public GameObject player; // Assign your player object in Inspector

    private bool isGameOver = false;

    void Update()
    {
        // Check if player has been destroyed or deactivated
        if (!isGameOver && player == null)
        {
            TriggerGameOverInternal();
        }

        // Only allow restart after game over
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            // Resume time before reloading
            Time.timeScale = 1f;

            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadSceneAsync(currentScene.buildIndex);
        }
    }

    // Internal-only method — cannot be called by other scripts
    private void TriggerGameOverInternal()
    {
        isGameOver = true;

        // Show Game Over UI
        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);

        if (gameOverMessage != null)
            gameOverMessage.text = "Game Over";

        // Pause the game
        Time.timeScale = 0f;
    }
}