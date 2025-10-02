// Anthony_Hats.cs
// Billboarded Sprite hat, Tab cycles including no-hat.
using System.Collections.Generic;
using UnityEngine;

public class Anthony_Hats : MonoBehaviour
{
    public List<Sprite> hats = new List<Sprite>();
    public float yOffset = 2.1f;
    public int sortingOrder = 50;

    Transform root;
    SpriteRenderer sr;
    int idx = -1;

    void Start()
    {
        root = new GameObject("Hat").transform;
        root.SetParent(transform, false);
        root.localPosition = new Vector3(0,yOffset,0);
        sr = root.gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = sortingOrder;
        Apply(-1);
    }

    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam) root.forward = cam.transform.forward;
        if (Input.GetKeyDown(KeyCode.Tab)) Cycle();
    }

    void Cycle()
    {
        idx = (idx + 1) % (hats.Count + 1);
        Apply(idx - 1);
    }

    void Apply(int hatIdx){ sr.sprite = hatIdx < 0 ? null : hats[hatIdx]; }
}
