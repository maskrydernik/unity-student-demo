using UnityEngine;
using UnityEngine.UI;

public class HealthTrack : MonoBehaviour
{
    public BasicFighter2D fightTrack;
    public Image healthBar;

    private int maxHealth = 0;
    private int currentHealth = 0;

    private void Awake()
    {
        if (fightTrack == null)
        {
            fightTrack = GetComponent<BasicFighter2D>();
        }
    }

    private void Start()
    {
        if (fightTrack != null)
        {
            maxHealth = fightTrack.GetMaxHP();
            currentHealth = fightTrack.GetCurrentHP();
            SetupHealthBar();
        }
    }

    private void Update()
    {
        if (fightTrack != null)
        {
            int newHP = fightTrack.GetCurrentHP();
            if (newHP != currentHealth)
            {
                currentHealth = newHP;
            }
        }
    }

    private void SetupHealthBar()
    {
        {
            if (healthBar != null)
                healthBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }

}