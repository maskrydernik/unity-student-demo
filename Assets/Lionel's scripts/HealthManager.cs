using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public Image healthBar;
    public float healthAmount = 100f;
    public float currentHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = healthAmount;
        UpdateHealthBar();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)){
            TakeDamage(5);
        }

        if(currentHealth <= 0)
        {

        }
    }

    public void SetMaxHealth(float health)
    {
        healthAmount = health;
        currentHealth = health;
        UpdateHealthBar();
    }

    public void SetHealth(float health)
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
    
    public void TakeDamage(float damage)
    {
        healthAmount -= damage;
        healthBar.fillAmount = healthAmount / 100f;
    }
}
