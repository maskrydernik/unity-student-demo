// David_Shop.cs
// Simple shop for units/buildings. Q toggles a panel if assigned.
using UnityEngine;

public class David_Shop : MonoBehaviour
{
    public GameObject shopPanel;
    public Transform spawnPoint;
    public GameObject unitPrefab;
    public GameObject buildingPrefab;
    public GameObject baricadePrefab;
    public GameObject archertowerPrefab;
    public int unitCost = 20;
    public int buildingCost = 30;
    public int baricadeCost = 15;
    public int archerCost = 35;

    void Update()
    {
        if (shopPanel != null && Input.GetKeyDown(KeyCode.Q))
        {
            bool isActive = shopPanel.activeSelf;
            shopPanel.SetActive(!isActive);
        }
    }

    public void BuyUnit()
    {
        if (!ValidateSpawn(unitPrefab, unitCost))
        {
            return;
        }

        GameGlue.I.AddGold(-unitCost);
        Transform point = spawnPoint != null ? spawnPoint : transform;

        /*
        Transform point;
        if (spawnPoint != null)
            point = spawnPoint;
        else
            point = transform;
        */
        Instantiate(unitPrefab, point.position, point.rotation);
        GameGlue.I.Hint("Recruited a unit");
    }

    public void BuyBuilding()
    {
        if (!ValidateSpawn(buildingPrefab, buildingCost))
        {
            return;
        }

        GameGlue.I.AddGold(-buildingCost);
        Transform point = spawnPoint != null ? spawnPoint : transform;
        Instantiate(buildingPrefab, point.position, point.rotation);
        GameGlue.I.AddHouse(1);
        GameGlue.I.Hint("Constructed a building");
    }

    public void BuyBaricade()
    {
        if (!ValidateSpawn(baricadePrefab, baricadeCost))
        {
            return;
        }

        GameGlue.I.AddGold(-baricadeCost);
        Transform point = spawnPoint != null ? spawnPoint : transform;
        Instantiate(baricadePrefab, point.position, point.rotation);
        GameGlue.I.AddHouse(1);
        GameGlue.I.Hint("Constructed a baricade");
    }

    public void BuyArcherTower()
    {
        if (!ValidateSpawn(archertowerPrefab, archerCost))
        {
            return;
        }

        GameGlue.I.AddGold(-archerCost);
        Transform point = spawnPoint != null ? spawnPoint : transform;
        Instantiate(archertowerPrefab, point.position, point.rotation);
        GameGlue.I.AddHouse(1);
        GameGlue.I.Hint("Constructed a baricade");
    }

    // Checks prefab, gold, and spawn validity before purchase
    public bool ValidateSpawn(GameObject prefab, int cost)
    {
        if (GameGlue.I == null)
        {
            Debug.LogWarning("GameGlue instance missing; cannot process purchase");
            return false;
        }

        if (prefab == null)
        {
            GameGlue.I.Hint("No prefab assigned");
            return false;
        }

        if (GameGlue.I.gold < cost)
        {
            GameGlue.I.Hint("Need " + cost + " gold");
            return false;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning(name + " missing spawn point; using self position");
        }

        return true;
    }
}
