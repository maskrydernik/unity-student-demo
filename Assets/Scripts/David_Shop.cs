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
        if (!ValidateSpawn(unitPrefab, unitCost)) return;

        GameGlue.I.AddGold(-unitCost);
        Transform point = spawnPoint ? spawnPoint : transform;
        Instantiate(unitPrefab, point.position, point.rotation);
        GameGlue.I.Hint("Recruited a unit");
    }

    public void BuyBuilding()
    {
        if (!ValidateSpawn(buildingPrefab, buildingCost)) return;

        GameGlue.I.AddGold(-buildingCost);
        Transform point = spawnPoint ? spawnPoint : transform;
        Instantiate(buildingPrefab, point.position, point.rotation);
        GameGlue.I.AddHouse(1);
        GameGlue.I.Hint("Constructed a building");
    }

    bool ValidateSpawn(GameObject prefab, int cost)
    {
        if (!GameGlue.I)
        {
            Debug.LogWarning("GameGlue instance missing; cannot process purchase");
            return false;
        }

        if (!prefab)
        {
            GameGlue.I.Hint("No prefab assigned");
            return false;
        }

        if (GameGlue.I.gold < cost)
        {
            GameGlue.I.Hint("Need " + cost + " gold");
            return false;
        }

        if (!spawnPoint)
        {
            Debug.LogWarning(name + " missing spawn point; using self position");
        }

        return true;
    }
}
