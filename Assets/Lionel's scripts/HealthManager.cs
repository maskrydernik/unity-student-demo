using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public Image healthBar;
    public int healthAmount; //MaxHp for fighter
    public int currentHealth;

    public BasicFighter2D fighter;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(fighter == null)
        {
            fighter = GetComponentInParent<BasicFighter2D>();
        }
        
        //currentHealth = fighter.GetCurrentHP();
        //healthAmount = fighter.GetMaxHP();

        if(fighter != null)
        {
            healthAmount = fighter.GetMaxHP();
            currentHealth = fighter.GetCurrentHP();
            
        }

    }

    // Update is called once per frame
    void Update()
    {
        

            //if (Input.GetKeyDown(KeyCode.Space))
            //{
                if (fighter != null)
                {
                    int updateHP = fighter.GetCurrentHP();
                    if (updateHP != currentHealth)
                    {
                        
                        currentHealth = updateHP;
                        TakeDamage(10);
    
                    }
                }

            //}
 
    }
    public void TakeDamage(int damage)
    {
        //if (healthBar == null || healthAmount <= 0) return;

        currentHealth -= damage;
        healthBar.fillAmount = currentHealth / 100f;
    }

 

}
