// GameSystems.cs
// Central glue for shop, quest, HUD, and cross-script signals.
// Keys: Q=Shop, R=Retry, Tab=Hats, H=Full-Heal, Shift+Drag=Multi-Select, RightClick=Move/Attack.
//
// Students covered (why): David(shop), Mary/HUD+weapon-choice, Christopher(raider quest cap),
// Yari(retry), Anthony(hats), Lionel(heal-charge), VictorG(gear stat HUD),
// Jordon(speed on kill), Ryan(drops), Steven(item tiers), Nicholas(auto-combat),
// Kameron(rage+exhaust), JohnSw(multi-select + group orders).

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameSystems : MonoBehaviour
{
    // ---- Economy / Shop (David) ----
    public int gold = 50;
    public int unitCost = 20;
    public int buildingCost = 30;
    public Transform spawnPoint;
    public GameObject unitPrefab;
    public GameObject buildingPrefab;
    public GameObject shopPanel;        // toggled with Q

    // ---- HUD (Mary / VictorG) ----
    public TMP_Text goldText;               // "Gold: 50"
    public TMP_Text gearText;               // "Gear: W+1 A+0 | Heal:25%"
    public TMP_Text questText;              // "Raiders: 3/7"
    public TMP_Text tipText;                // short hints

    // ---- Weapon Choice (Mary) ----
    public GameObject weaponChoicePanel;    // buttons call OnClickWeaponTier(int tier)

    // ---- Quest (Christopher) ----
    public string raiderTag = "Raider";
    public int raiderGoal = 7;
    public int raiderKills = 0;
    public GameObject raiderSpawnerRoot;    // disabled when quest completes

    // ---- Selection (JohnSw) ----
    public Manager selection;               // multi-select and orders live here

    public static GameSystems I;

    void Awake() { I = this; }

    void Start()
    {
        shopPanel.SetActive(false);
        weaponChoicePanel.SetActive(false);
        UpdateHUD();
        Hint("LClick select. Shift+drag multi. RClick move/attack. Q shop. Tab hats. H heal. R retry.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        if (Input.GetKeyDown(KeyCode.Q)) shopPanel.SetActive(!shopPanel.activeSelf);

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            foreach (Unit u in selection.SelectedUnits)
            {
                HatBillboardSwitcher h = u.GetComponent<HatBillboardSwitcher>();
                h.Cycle();
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            foreach (Unit u in selection.SelectedUnits)
            {
                UnitExtras ex = u.GetComponent<UnitExtras>();
                ex.TryFullHeal();
            }
        }
    }

    // ---- Public hooks ----

    // Ryan + Christopher: enemy death → possible drop and quest progress
    public void OnEnemyKilled(GameObject enemy)
    {
        if (enemy.CompareTag(raiderTag))
        {
            raiderKills = Mathf.Min(raiderGoal, raiderKills + 1);
            if (raiderKills >= raiderGoal) raiderSpawnerRoot.SetActive(false);
            UpdateHUD();
        }

        SimpleDrop drop = enemy.GetComponent<SimpleDrop>();
        if (drop != null)
        {
            if (drop.dropPrefab != null)
            {
                Instantiate(drop.dropPrefab, enemy.transform.position, Quaternion.identity);
            }
        }
    }

    // Mary: simple weapon selection before the first engage
    public void OnClickWeaponTier(int tier)
    {
        foreach (Unit u in selection.SelectedUnits)
        {
            UnitExtras ex = u.GetComponent<UnitExtras>();
            ex.EquipWeapon(tier);
        }
        weaponChoicePanel.SetActive(false);
        Hint("Equipped W+" + tier + " on " + selection.SelectedUnits.Count + " unit(s)");
    }

    // ---- Shop (David) ----
    public void BuyUnit()
    {
        gold -= unitCost;
        GameObject go = Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
        AutoWire(go);
        UpdateHUD();
    }

    public void BuyBuilding()
    {
        gold -= buildingCost;
        Instantiate(buildingPrefab, spawnPoint.position, Quaternion.identity);
        UpdateHUD();
    }

    // ---- HUD helpers (Mary / VictorG) ----
    public void AddGold(int delta) { gold += delta; UpdateHUD(); }
    public void UpdateGearHUD(string s) { gearText.text = s; }
    public void ShowWeaponChoicePanel() { weaponChoicePanel.SetActive(true); }

    public void Hint(string s) { tipText.text = s; }
    public void UpdateHUD()
    {
        goldText.text = "Gold: " + gold;
        questText.text = "Raiders: " + raiderKills + "/" + raiderGoal;
    }

    // Ensure spawned units participate in all systems
    void AutoWire(GameObject go)
    {
        go.AddComponent<UnitExtras>();           // HP, heal, gear, speed-on-kill
        go.AddComponent<AutoCombat>();           // target, approach, attack, rage/exhaust
        go.AddComponent<HatBillboardSwitcher>(); // hats via SpriteRenderer billboard
    }
}
