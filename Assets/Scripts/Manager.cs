// Manager.cs
// Multi-select rectangle + group orders. Keeps your original spirit, but now supports RTS-style control.
// Left click = clear or start drag (with Shift). Right click = move or attack target.
//
// Replaces old Manager.cs. (Original single-select mouse control)

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [Header("Screen-UI")]
    public Image dragRectImage;          // screen-space rect image, disabled by default

    [Header("Layers")]
    public LayerMask unitMask;           // "Unit"
    public LayerMask groundMask;         // "Ground"

    public List<Unit> SelectedUnits = new List<Unit>();

    Camera cam;
    Vector2 dragStart;
    bool dragging;

    void Start()
    {
        cam = Camera.main;
        dragRectImage.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleSelectionInput();
        HandleRightClickOrder();
    }

    void HandleSelectionInput()
    {
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (shift && Input.GetMouseButtonDown(0))
        {
            dragging = true;
            dragStart = Input.mousePosition;
            dragRectImage.gameObject.SetActive(true);
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
            dragRectImage.gameObject.SetActive(false);
            RectSelect(dragStart, Input.mousePosition);
            GameSystems.I.Hint("Selected " + SelectedUnits.Count + " unit(s).");
        }

        if (!shift && Input.GetMouseButtonDown(0))
        {
            ClearSelection();
        }
    }

    void RectSelect(Vector2 a, Vector2 b)
    {
        Vector2 min = Vector2.Min(a, b);
        Vector2 max = Vector2.Max(a, b);

        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            Vector3 p = cam.WorldToScreenPoint(u.transform.position);
            bool inside = p.z > 0 && p.x >= min.x && p.x <= max.x && p.y >= min.y && p.y <= max.y;
            if (inside)
            {
                if (!SelectedUnits.Contains(u))
                {
                    SelectedUnits.Add(u);
                    u.SetSelected(true);
                }
            }
        }
    }

    void HandleRightClickOrder()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            bool hitGround = Physics.Raycast(ray, out hit, 1000f, groundMask);
            if (hitGround)
            {
                foreach (Unit u in SelectedUnits) u.MoveTo(hit.point);
            }
            else
            {
                bool hitAnything = Physics.Raycast(ray, out hit, 1000f);
                if (hitAnything)
                {
                    AutoCombat enemy = hit.collider.GetComponentInParent<AutoCombat>();
                    if (enemy != null && enemy.team != AutoCombat.Team.Player)
                    {
                        GameSystems.I.ShowWeaponChoicePanel(); // Mary
                        foreach (Unit u in SelectedUnits)
                        {
                            AutoCombat ac = u.GetComponent<AutoCombat>();
                            ac.SetForcedTarget(enemy);
                        }
                    }
                }
            }
        }
    }

    public void ClearSelection()
    {
        foreach (Unit u in SelectedUnits) u.SetSelected(false);
        SelectedUnits.Clear();
    }
}
