// Arthur_WorldHPBar.cs
// Tracks HP and draws a simple world-space bar. Optional Rage fill as child Image named "Rage".
using UnityEngine;
using UnityEngine.UI;

public class Arthur_WorldHPBar : MonoBehaviour
{
    public Canvas worldCanvasPrefab;
    public float maxHP = 100f;
    public float hp = 100f;
    public float uiHeight = 2f;

    Image healthFillImage;
    Image rageFillImage;
    Transform canvasTransform;

    void Start()
    {
        if (worldCanvasPrefab == null)
        {
            return;
        }

    Canvas canvasInstance = Instantiate(worldCanvasPrefab, transform);
    canvasTransform = canvasInstance.transform;
    canvasTransform.localPosition = new Vector3(0f, uiHeight, 0f);

        foreach (Image image in canvasInstance.GetComponentsInChildren<Image>())
        {
            if (image.name == "HP")
            {
                healthFillImage = image;
            }

            if (image.name == "Rage")
            {
                rageFillImage = image;
            }
        }

        Sync();
    }

    public void ApplyDamage(float dmg)
    {
        hp = Mathf.Max(0f, hp - dmg);
        Sync();

        if (hp <= 0f)
        {
            Die();
        }
    }

    public void Sync()
    {
        if (healthFillImage == null)
        {
            return;
        }

        float fillAmount = maxHP > 0f ? hp / maxHP : 0f;
        healthFillImage.fillAmount = fillAmount;
    }

    public void SetRageFill(float value)
    {
        if (rageFillImage == null)
        {
            return;
        }

        rageFillImage.fillAmount = Mathf.Clamp01(value);
    }

    void Die()
    {
        // Quest notify and simple drop are handled by other scripts on this object, if present.
        var autoCombat = GetComponent<Nicholas_AutoCombat>();
        if (autoCombat != null && autoCombat.team == Nicholas_AutoCombat.Team.Enemy)
        {
            if (Christopher_RaiderQuest.I != null)
            {
                Christopher_RaiderQuest.I.NotifyEnemyDeath(gameObject);
            }

            var drop = GetComponent<Ryan_SimpleDrop>();
            if (drop != null && drop.dropPrefab != null)
            {
                Instantiate(drop.dropPrefab, transform.position, Quaternion.identity);
            }
        }
        Destroy(gameObject);
    }

    void LateUpdate()
    {
        if (canvasTransform == null)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraUp = mainCamera.transform.up;
        canvasTransform.rotation = Quaternion.LookRotation(cameraForward, cameraUp);
        // transform.LookAt(position);

    }
}
