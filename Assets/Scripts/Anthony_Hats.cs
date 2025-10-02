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
        if (Input.GetKeyDown(KeyCode.Tab) && IsSelected()) Cycle();
    }

    void Cycle()
    {
        if (hats.Count == 0)
        {
            Apply(-1);
            idx = -1;
            return;
        }

        int next = idx + 1;
        if (next >= hats.Count) next = -1;

        Apply(next);
        idx = next;
    }

    void Apply(int hatIdx)
    {
        sr.sprite = hatIdx < 0 ? null : hats[hatIdx];
    }

    bool IsSelected()
    {
        var unit = GetComponent<Unit>();
        if (unit) return unit.IsSelected;
        return true;
    }
}
