// JohnSw_MultiSelect.cs
// Shift-drag to select many. Right-click to move or attack target.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JohnSw_MultiSelect : MonoBehaviour
{
    public Image dragRectImage; // screen-space Image
    public LayerMask groundMask;

    public List<Unit> selection = new List<Unit>();
    Camera cam;
    Vector2 dragStart; bool dragging;

    void Start(){ cam = Camera.main; if (dragRectImage) dragRectImage.gameObject.SetActive(false); }

    void Update(){ HandleSelection(); HandleOrders(); }

    void HandleSelection()
    {
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (shift && Input.GetMouseButtonDown(0))
        {
            dragging = true;
            dragStart = Input.mousePosition;
            if (dragRectImage) dragRectImage.gameObject.SetActive(true);
        }

        if (dragging)
        {
            Vector2 cur = Input.mousePosition;
            Vector2 min = Vector2.Min(dragStart, cur);
            Vector2 size = Vector2.Max(dragStart, cur) - min;
            var rt = dragRectImage.rectTransform;
            rt.anchoredPosition = min;
            rt.sizeDelta = size;
        }

        if (shift && Input.GetMouseButtonUp(0))
        {
            dragging = false;
            if (dragRectImage) dragRectImage.gameObject.SetActive(false);
            RectSelect(dragStart, Input.mousePosition);
        }

        if (!shift && Input.GetMouseButtonDown(0)) Clear();
    }

    void RectSelect(Vector2 a, Vector2 b)
    {
        Vector2 min = Vector2.Min(a,b);
        Vector2 max = Vector2.Max(a,b);

        foreach (var u in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            Vector3 p = cam.WorldToScreenPoint(u.transform.position);
            bool inside = p.z > 0 && p.x >= min.x && p.x <= max.x && p.y >= min.y && p.y <= max.y;
            if (inside){ if (!selection.Contains(u)) { selection.Add(u); u.SetSelected(true); } }
        }
        GameGlue.I.Hint("Selected " + selection.Count + " unit(s)");
    }

    void HandleOrders()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f, groundMask))
            {
                foreach (var u in selection) u.MoveTo(hit.point);
            }
            else if (Physics.Raycast(ray, out hit, 1000f))
            {
                var enemy = hit.collider.GetComponentInParent<Nicholas_AutoCombat>();
                if (enemy != null && enemy.team != Nicholas_AutoCombat.Team.Player)
                {
                    foreach (var u in selection)
                    {
                        var ac = u.GetComponent<Nicholas_AutoCombat>();
                        if (ac) u.MoveTo(enemy.transform.position); // simple: converge
                    }
                }
            }
        }
    }

    public void Clear(){ foreach (var u in selection) u.SetSelected(false); selection.Clear(); }
}
