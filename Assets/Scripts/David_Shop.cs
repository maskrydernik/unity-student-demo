// David_Shop.cs
// Simple shop for units/buildings. Q toggles a panel if assigned.
using UnityEngine;

public class David_Shop : MonoBehaviour
{
    public GameObject shopPanel;
    public Transform spawnPoint;
    public GameObject unitPrefab;
    public GameObject buildingPrefab;
    public int unitCost = 20;
    public int buildingCost = 30;

    void Update()
    {
        if (shopPanel && Input.GetKeyDown(KeyCode.Q))
            shopPanel.SetActive(!shopPanel.activeSelf);
    }

    public void BuyUnit()
    {
        if (GameGlue.I.gold < unitCost) { GameGlue.I.Hint("Need " + unitCost + " gold"); return; }
        GameGlue.I.AddGold(-unitCost);
        var go = Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
        // Minimal auto-wire
        if (!go.GetComponent<Nicholas_AutoCombat>()) go.AddComponent<Nicholas_AutoCombat>();
        if (!go.GetComponent<Steven_GearStats>()) go.AddComponent<Steven_GearStats>();
        if (!go.GetComponent<Arthur_WorldHPBar>()) go.AddComponent<Arthur_WorldHPBar>();
        if (!go.GetComponent<Lionel_HealCharge>()) go.AddComponent<Lionel_HealCharge>();
        if (!go.GetComponent<Anthony_Hats>()) go.AddComponent<Anthony_Hats>();
        if (!go.GetComponent<Jordon_SpeedOnKill>()) go.AddComponent<Jordon_SpeedOnKill>();
        if (!go.GetComponent<UnityEngine.AI.NavMeshAgent>()) go.AddComponent<UnityEngine.AI.NavMeshAgent>();
        if (!go.GetComponent<Unit>()) go.AddComponent<Unit>();
    }

    public void BuyBuilding()
    {
        if (GameGlue.I.gold < buildingCost) { GameGlue.I.Hint("Need " + buildingCost + " gold"); return; }
        GameGlue.I.AddGold(-buildingCost);
        Instantiate(buildingPrefab, spawnPoint.position, Quaternion.identity);
        GameGlue.I.AddHouse(1);
    }
}
