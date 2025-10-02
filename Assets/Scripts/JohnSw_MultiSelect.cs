// JohnSw_MultiSelect.cs
// Minimal working selection: Click to select, Shift+drag for multi-select, Right-click to move.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JohnSw_MultiSelect : MonoBehaviour
{
    public Image dragRectImage;
    public List<Unit> selection = new List<Unit>();
    
    Camera cam;
    Vector2 dragStart;
    bool dragging;

    void Start()
    {
        cam = Camera.main;
        if (dragRectImage) dragRectImage.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleSelection();
        HandleMovement();
    }

    void HandleSelection()
    {
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Start drag with Shift
        if (shift && Input.GetMouseButtonDown(0))
        {
            dragging = true;
            dragStart = Input.mousePosition;
            if (dragRectImage) dragRectImage.gameObject.SetActive(true);
        }

        // Update drag rectangle
        if (dragging)
        {
            Vector2 cur = Input.mousePosition;
            Vector2 min = Vector2.Min(dragStart, cur);
            Vector2 max = Vector2.Max(dragStart, cur);
            
            if (dragRectImage)
            {
                var rt = dragRectImage.rectTransform;
                rt.anchoredPosition = min;
                rt.sizeDelta = max - min;
            }
        }

        // End drag - select units in rectangle
        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;
            if (dragRectImage) dragRectImage.gameObject.SetActive(false);
            
            Vector2 min = Vector2.Min(dragStart, Input.mousePosition);
            Vector2 max = Vector2.Max(dragStart, Input.mousePosition);
            
            foreach (Unit u in FindObjectsByType<Unit>(FindObjectsSortMode.None))
            {
                Vector3 screenPos = cam.WorldToScreenPoint(u.transform.position);
                if (screenPos.z > 0 && 
                    screenPos.x >= min.x && screenPos.x <= max.x && 
                    screenPos.y >= min.y && screenPos.y <= max.y)
                {
                    if (!selection.Contains(u))
                    {
                        selection.Add(u);
                        u.SetSelected(true);
                    }
                }
            }
            
            if (GameGlue.I) GameGlue.I.Hint("Selected " + selection.Count + " units");
        }

        // Single click without shift
        if (!shift && Input.GetMouseButtonDown(0))
        {
            // Clear old selection
            foreach (Unit u in selection) u.SetSelected(false);
            selection.Clear();
            
            // Try to select clicked unit
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);
            
            foreach (RaycastHit hit in hits)
            {
                Unit u = hit.collider.GetComponent<Unit>();
                if (u == null) u = hit.collider.GetComponentInParent<Unit>();
                
                if (u != null)
                {
                    selection.Add(u);
                    u.SetSelected(true);
                    if (GameGlue.I) GameGlue.I.Hint("Selected 1 unit");
                    break;
                }
            }
        }
    }

    void HandleMovement()
    {
        if (Input.GetMouseButtonDown(1) && selection.Count > 0)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                foreach (Unit u in selection)
                {
                    u.MoveTo(hit.point);
                }
                if (GameGlue.I) GameGlue.I.Hint("Moving units");
            }
        }
    }
}
