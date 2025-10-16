using UnityEngine;
using UnityEngine.UI;

public class ArthurBar : MonoBehaviour
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
            UpdateHealthBar();
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
                UpdateHealthBar();
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

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }
}