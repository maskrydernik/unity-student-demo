using UnityEngine;

public class HealthBar_RyanAguiar : MonoBehaviour
{
    public BasicFighter2D fightTrack;
    public SpriteRenderer healthBarSprite; // Your HealthBar_0 sprite

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

        // scale only X axis (shrinks bar)
        Vector3 newScale = fullScale;
        newScale.x = fullScale.x * healthPercent;
        healthBarSprite.transform.localScale = newScale;

        // move the sprite so it shrinks from right to left, not middle
        Vector3 newPos = startPosition;
        newPos.x = startPosition.x - (spriteWidth * (1 - healthPercent) / 2f);
        healthBarSprite.transform.localPosition = newPos;
    }
}