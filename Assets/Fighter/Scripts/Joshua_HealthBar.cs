using UnityEngine;
using UnityEngine.UI;

public class CustomHealthBar : MonoBehaviour
{
    public BasicFighter2D fighter; // Reference to the fighter this bar tracks
    public Image fillImage;        // UI image for fill
    public Vector3 worldOffset = new Vector3(0, 2f, 0); // Offset above fighter
    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (fighter == null || fillImage == null) return;

        // Update health fill
        float ratio = (float)fighter.GetCurrentHP() / fighter.GetMaxHP();
        fillImage.fillAmount = ratio;

        // Optional: change color based on health
        fillImage.color = Color.Lerp(Color.red, Color.green, ratio);

        // If using world-space Canvas, follow fighter position
        if (GetComponentInParent<Canvas>().renderMode == RenderMode.WorldSpace)
        {
            transform.position = fighter.transform.position + worldOffset;
        }
    }
}
