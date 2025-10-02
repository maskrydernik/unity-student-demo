// GameGlue.cs
// Central, neutral glue. Not credited to a student. Keeps shared counters and simple helpers.
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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

    void Awake()
    {
        I = this;
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

    public void Hint(string message)
    {
        if (tipText != null)
        {
            tipText.text = message;
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
