using UnityEngine;
using UnityEngine.UI;

public class Nicholas_Healthbar : MonoBehaviour
{
    [Header("References")]
    public BasicFighter2D fightTrack;
    public Image healthFill; // Assign this in the Inspector

    private float maxHealth;
    private float currentHealth;

    [Header("Visual Settings")]
    public float smoothSpeed = 10f; // Controls smooth transition
    private float targetFill;

    void Start()
    {
        if (fightTrack == null)
            fightTrack = GetComponent<BasicFighter2D>();

        maxHealth = fightTrack.GetMaxHP();
        currentHealth = fightTrack.GetCurrentHP();

        UpdateHealthInstant();
    }

    void Update()
    {
        // Keep reading current health every frame
        currentHealth = fightTrack.GetCurrentHP();
        targetFill = currentHealth / maxHealth;

        // Smoothly animate the bar
        if (healthFill != null)
        {
            healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, targetFill, Time.deltaTime * smoothSpeed);
        }
    }

    private void UpdateHealthInstant()
    {
        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;
    }
}
