using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public Image healthBar;
    public int healthAmount = 100;
    public int currentHealth;

    public BasicFighter2D fighter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = fighter.GetCurrentHP();
        UpdateHealthBar();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)){
            TakeDamage(5);
            //currentHealth = fightTrack.GetCurrentHP();
            //maxHealth = fightTrack.GetMaxHP();
        }

        if(currentHealth <= 0)
        {

        }
    }

    public void SetMaxHealth(int health)
    {
        healthAmount = health;
        currentHealth = health;
        UpdateHealthBar();
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, healthAmount);
        UpdateHealthBar();
    }


    public void UpdateHealthBar()
    {
        if(healthBar != null)
        {
            healthBar.fillAmount = currentHealth / healthAmount;
        }
    }
    
    public void TakeDamage(int damage)
    {
        healthAmount -= damage;
        healthBar.fillAmount = healthAmount / 100f;
    }
}
