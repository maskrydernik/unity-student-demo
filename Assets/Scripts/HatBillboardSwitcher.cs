// HatBillboardSwitcher.cs
// Anthony: hats as SpriteRenderers that billboard toward camera.
// Exactly one sprite visible. Tab cycles; includes "no hat" state.

using System.Collections.Generic;
using UnityEngine;

public class HatBillboardSwitcher : MonoBehaviour
{
    public List<Sprite> hats = new List<Sprite>();
    public float yOffset = 2.1f;
    public int sortingOrder = 50;

    Transform hatRoot;
    SpriteRenderer sr;
    int idx = -1;

    void Start()
    {
        hatRoot = new GameObject("HatSprite").transform;
        hatRoot.SetParent(transform, false);
        hatRoot.localPosition = new Vector3(0, yOffset, 0);

        sr = hatRoot.gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = sortingOrder;

        Apply(-1); // start with no hat
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        hatRoot.forward = cam.transform.forward; // billboard
    }

    public void Cycle()
    {
        idx = (idx + 1) % (hats.Count + 1); // include "no hat"
        Apply(idx - 1);
    }

    void Apply(int hatIdx)
    {
        if (hatIdx < 0) sr.sprite = null;
        else sr.sprite = hats[hatIdx];
    }
}
