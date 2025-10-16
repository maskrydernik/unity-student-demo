using UnityEngine;
using UnityEngine.UI;

public class Nicholas_Healthbar : MonoBehaviour
{




    public BasicFighter2D fightTrack;
    public Image healthBar; 

    private int maxHealth = 1000;
    private int currentHealth = 1000;


    void Start()
    {


        currentHealth = fightTrack.GetCurrentHP();
        maxHealth = fightTrack.GetMaxHP();

        print(currentHealth);
        print(maxHealth);


    }

    private void Update()
    {
        
    }
}
