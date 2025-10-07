// Anthony_Hats.cs
// Billboarded Sprite hat, Tab cycles including no-hat.
using System.Collections.Generic;
using UnityEngine;

public class Anthony_Hats : MonoBehaviour
{
    public List<Sprite> hats = new List<Sprite>();
    public float yOffset = 2.1f;
    public int sortingOrder = 50;

    private Transform hatRoot;
    private SpriteRenderer hatRenderer;
    private int currentHatIndex = -1;

    void Start()
    {
        CreateHatRenderer();
        ApplyHat(-1);
    }

    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            hatRoot.forward = cam.transform.forward;
        }
    }

    void CreateHatRenderer()
    {
        hatRoot = new GameObject("Hat").transform;
        hatRoot.SetParent(transform, false);
        hatRoot.localPosition = new Vector3(0f, yOffset, 0f);

        hatRenderer = hatRoot.gameObject.AddComponent<SpriteRenderer>();
        hatRenderer.sortingOrder = sortingOrder;
    }

    public void SelectHat (int hatIndex)
    {
        if (hatIndex >= 0 && hatIndex < hats.Count)
        {
            ApplyHat(hatIndex);
            currentHatIndex = hatIndex;
        }
        else
        {
            ApplyHat(-1);
            currentHatIndex = -1;
        }
    }

    void ApplyHat(int hatIndex)
    {
        hatRenderer.sprite = hatIndex < 0 ? null : hats[hatIndex];
    }
}
