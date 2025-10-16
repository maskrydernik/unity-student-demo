using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MiniWoW;


public class Mehran_HealthScript : MonoBehaviour
{
    public Image healthBar;
    public float HealthAmount = 100f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (HealthAmount <= 0)
        {
            Application.LoadLevel(Application.loadedLevel);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            TakeDamage(20);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Heal(5);
        }
    }

    public void TakeDamage(float damage)
    {
        HealthAmount -= damage;
        healthBar.fillAmount = HealthAmount / 100f;
    }
    public void Heal(float healingAmount)
    {
        HealthAmount += healingAmount;
        HealthAmount = Mathf.Clamp(HealthAmount, 0, 100);

        healthBar.fillAmount = HealthAmount / 100f;
    }

}
