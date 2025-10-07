using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Anthony_HatMenu : MonoBehaviour
{
    public GameObject hatPanel;
    public Button toggleButton;

    private List<Anthony_Hats> allPlayers = new List<Anthony_Hats>();
    private bool isOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hatPanel.SetActive(false);

        toggleButton.onClick.AddListener(ToggleHatPanel);

        FindAllPlayers();
    }

    void FindAllPlayers()
    {
        allPlayers.Clear();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        foreach (GameObject player in players)
        {
            Anthony_Hats hats = player.GetComponent<Anthony_Hats>();
            if (hats != null)
            {
                allPlayers.Add(hats);
            }
        }
    }

    public void ToggleHatPanel()
    {
        isOpen = !isOpen;
        hatPanel.SetActive(isOpen);
    }

    public void OnHatButtonClicked(int hatIndex)
    {
        Anthony_Hats currentTarget = GetSelectedCharacter();

        if (currentTarget == null) return;

        if (hatIndex == 0)
        {
            currentTarget.SelectHat(-1);
        }
        else
        {
            int adjustedIndex = hatIndex - 1;
            currentTarget.SelectHat(adjustedIndex);
        }
    }

    private Anthony_Hats GetSelectedCharacter()
    {
        foreach (Anthony_Hats hats in allPlayers)
        {
            Unit unit = hats.GetComponent<Unit>();
            if (unit != null && unit.IsSelected)
            {
                return hats;
            }
        }
        return null;
    }
}
