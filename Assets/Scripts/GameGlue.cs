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

    void Awake(){ I = this; }

    public void AddGold(int d){ gold += d; RefreshHUD(); }
    public void AddKeys(int d){ keys += d; RefreshHUD(); }
    public void AddHouse(int d){ houses += d; RefreshHUD(); }

    public void SetGearText(string s){ if (gearText) gearText.text = s; }
    public void Hint(string s){ if (tipText) tipText.text = s; }

    public void RefreshHUD()
    {
        if (goldText) goldText.text = "Gold: " + gold + " | Houses: " + houses + " | Keys: " + keys;
    }
}
