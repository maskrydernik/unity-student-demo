using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    public BasicFighter2D fightTrack;
    public Image healthBar;
    public TextMeshProUGUI healthText;

    private int maxHealth = 0;
    private int currentHealth = 0;

    private void Awake()
    {
        if (fightTrack == null)
            fightTrack = GetComponent<BasicFighter2D>();
    }

    private void Start()
    {
        if (fightTrack != null)
        {
            maxHealth = fightTrack.GetMaxHP();
            currentHealth = fightTrack.GetCurrentHP();
            SetupHealthBar();
            UpdateHealthBar();
            UpdateHealthText();
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
                UpdateHealthText();
            }
        }
    }

    private void SetupHealthBar()
    {
        if (healthBar != null)
            healthBar.fillAmount = (float)currentHealth / maxHealth;
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.fillAmount = (float)currentHealth / maxHealth;
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
            healthText.text = currentHealth.ToString() + "/" + maxHealth.ToString();
    }
}
