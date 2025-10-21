using UnityEngine;

public class VictorG_HealthBar : MonoBehaviour
{
    public BasicFighter2D fightTrack;
    public SpriteRenderer healthBarSprite;

    private int maxHealth;
    private int currentHealth;
    private Vector3 fullScale;
    private Vector3 startPosition;
    private float spriteWidth;

    private void Start()
    {
        if (fightTrack == null)
            fightTrack = GetComponentInParent<BasicFighter2D>();

        if (healthBarSprite != null)
        {
            fullScale = healthBarSprite.transform.localScale;
            startPosition = healthBarSprite.transform.localPosition;
            spriteWidth = healthBarSprite.bounds.size.x;
        }

        if (fightTrack != null)
        {
            maxHealth = fightTrack.GetMaxHP();
            currentHealth = fightTrack.GetCurrentHP();
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

    private void UpdateHealthBar()
    {
        if (healthBarSprite == null || maxHealth <= 0) return;

        float healthPercent = Mathf.Clamp01((float)currentHealth / maxHealth);

        
        Vector3 newScale = fullScale;
        newScale.x = fullScale.x * healthPercent;
        healthBarSprite.transform.localScale = newScale;

        
        Vector3 newPos = startPosition;
        newPos.x = startPosition.x - (spriteWidth * (1 - healthPercent) / 2f);
        healthBarSprite.transform.localPosition = newPos;
    }
}