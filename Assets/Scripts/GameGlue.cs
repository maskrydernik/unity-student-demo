// GameGlue.cs
// Central, neutral glue. Not credited to a student. Keeps shared counters and simple helpers.
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // needed for coroutines

public class GameGlue : MonoBehaviour
{
    public static GameGlue I;

    [Header("Economy and Meta")]
    public int gold = 50;
    public int keys = 0;
    public int houses = 0;

    [Header("HUD refs (wired by Mary_HUD)")]
    public TMP_Text goldText;
    public TMP_Text gearText;
    public TMP_Text questText;
    public TMP_Text tipText;

    [Header("Dialogue ref")] // by Joshua
    public TMP_Text dialogueText;

    void Awake()
    {
        I = this;
    }

    void Start()
    {
        // Show a tip at startup
        if (tipText != null)
        {
            tipText.text = "Hi. Use WASD to move the camera. You see that little guy with a health bar over his head? Way back there? Click on him, then right-click where you want him to move. Get going.";
            StartCoroutine(ClearTipAfterDelay(30f));
        }
    }

    // Coroutine to clear tip text after a delay
    IEnumerator ClearTipAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (tipText != null)
            tipText.text = "";
    }

    // Call this to update the tip and cancel the startup coroutine if needed
    public void Hint(string message)
    {
        if (tipText != null)
        {
            tipText.text = message;
        }
    }

    public void ShowDialogue(string message)
    {
        if (dialogueText != null)
        {
            dialogueText.text = message;
        }
    }

    public void AddGold(int delta)
    {
        gold += delta;
        RefreshHUD();
    }

    public void AddKeys(int delta)
    {
        keys += delta;
        RefreshHUD();
    }

    public void AddHouse(int delta)
    {
        houses += delta;
        RefreshHUD();
    }

    public void SetGearText(string value)
    {
        if (gearText != null)
        {
            gearText.text = value;
        }
    }

    public void RefreshHUD()
    {
        if (goldText == null)
        {
            return;
        }

        goldText.text = "Gold: " + gold + " | Houses: " + houses + " | Keys: " + keys;
    }
}
