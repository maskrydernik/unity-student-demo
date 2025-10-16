using UnityEngine;

public class Healthbar : MonoBehaviour
{
   



    public BasicFighter2D fightTrack;

    private int maxHealth = 250;
    private int currentHealth = 250;


    void Start()
    {


        currentHealth = fightTrack.GetCurrentHP();
        maxHealth = fightTrack.GetMaxHP();

        print(currentHealth);
        print(maxHealth);


    }

    void Awake()
    {
        GetComponent<BasicFighter2D>();
    }
}
