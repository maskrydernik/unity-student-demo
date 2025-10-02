// Anthony_Hats.cs
// Billboarded Sprite hat, Tab cycles including no-hat.
using System.Collections.Generic;
using UnityEngine;

public class Anthony_Hats : MonoBehaviour
{
    public List<Sprite> hats = new List<Sprite>();
    public float yOffset = 2.1f;
    public int sortingOrder = 50;

    Transform hatRoot;
    SpriteRenderer hatRenderer;
    int currentHatIndex = -1;

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

        if (Input.GetKeyDown(KeyCode.Tab) && IsSelected())
        {
            CycleHat();
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

    void CycleHat()
    {
        if (hats.Count == 0)
        {
            ApplyHat(-1);
            currentHatIndex = -1;
            return;
        }

        int nextIndex = currentHatIndex + 1;
        if (nextIndex >= hats.Count)
        {
            nextIndex = -1; // wrap around to no hat
        }

        ApplyHat(nextIndex);
        currentHatIndex = nextIndex;
    }

    bool IsSelected()
    {
        var unit = GetComponent<Unit>();
        if (unit != null)
        {
            return unit.IsSelected;
        }

        return true;
    }

    void ApplyHat(int hatIndex)
    {
        hatRenderer.sprite = hatIndex < 0 ? null : hats[hatIndex];
    }
}
