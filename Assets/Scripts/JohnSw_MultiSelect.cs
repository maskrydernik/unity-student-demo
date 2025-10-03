// JohnSw_MultiSelect.cs
// Minimal working RTS-style selection system:
// - Left-click: select a single unit
// - Shift + drag: additive multi-select
// - Drag without shift: box-select, clears old selection
// - Right-click: move selected units

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JohnSw_MultiSelect : MonoBehaviour
{
    // UI image used to draw the drag selection rectangle
    public Image dragRectImage;

    // Currently selected units
    public List<Unit> selection = new List<Unit>();

    Camera mainCamera;
    Canvas dragCanvas;                   // canvas containing the dragRectImage
    RectTransform dragRectTransform;     // rect transform of drag rect
    RectTransform dragRectParent;        // parent RectTransform for coordinate conversion
    Vector2 dragStart;                   // drag start position (screen space)
    Vector2 dragStartLocal;              // drag start position (local space for UI)
    bool dragging;                       // whether we are currently dragging
    bool dragAdditive;                   // whether drag adds to selection instead of replacing
    readonly List<Unit> dragResults = new List<Unit>(); // units found inside drag box

    void Start()
    {
        mainCamera = Camera.main;

        if (dragRectImage != null)
        {
            // Initialize drag rect settings (hidden by default)
            dragRectImage.gameObject.SetActive(false);
            dragRectTransform = dragRectImage.rectTransform;
            dragRectParent = dragRectTransform.parent as RectTransform;
            dragCanvas = dragRectImage.canvas;
            if (dragCanvas == null)
            {
                dragCanvas = dragRectImage.GetComponentInParent<Canvas>();
            }

            // Force pivot/anchors to center for easier resizing
            dragRectTransform.pivot = new Vector2(0.5f, 0.5f);
            dragRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            dragRectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            // Reset rect size/position
            dragRectTransform.anchoredPosition = Vector2.zero;
            dragRectTransform.sizeDelta = Vector2.zero;

            // Ensure it draws on top of UI
            dragRectTransform.SetAsLastSibling();

            // Disable blocking UI raycasts
            dragRectImage.raycastTarget = false;
        }
    }

    void Update()
    {
        // Cache main camera if lost (e.g. new scene reload)
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        HandleSelection();
        HandleMovement();
    }

    // Handle selection input logic (dragging vs clicks)
    void HandleSelection()
    {
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // If we are already dragging, update the drag box
        if (dragging)
        {
            UpdateDragVisual(Input.mousePosition);

            // Release left mouse ends drag
            if (Input.GetMouseButtonUp(0))
            {
                CompleteDrag(Input.mousePosition);
            }
            return;
        }

        // Shift + left click drag = additive multi-select
        if (shift && Input.GetMouseButtonDown(0))
        {
            BeginDrag(true);
            return;
        }

        // Left click drag without shift = new selection
        if (!shift && Input.GetMouseButtonDown(0))
        {
            BeginDrag(false);
            return;
        }
    }

    // Begin a drag operation
    void BeginDrag(bool additive)
    {
        dragging = true;
        dragAdditive = additive;
        dragStart = Input.mousePosition;

        // Clear selection if not additive
        if (!additive)
        {
            ClearSelection();
        }
        else
        {
            // Prune destroyed/null units
            selection.RemoveAll(u => u == null);
        }

        // Activate drag rectangle UI
        if (dragRectImage != null)
        {
            dragRectImage.gameObject.SetActive(true);
            dragStartLocal = ScreenToLocal(dragStart);
            UpdateDragVisual(dragStart);
        }
    }

    // Update the drag box UI while dragging
    void UpdateDragVisual(Vector2 currentScreenPosition)
    {
        if (dragRectTransform == null || dragRectParent == null)
        {
            return;
        }

        Vector2 startLocal = dragStartLocal;
        Vector2 currentLocal = ScreenToLocal(currentScreenPosition);
        Vector2 min = Vector2.Min(startLocal, currentLocal);
        Vector2 max = Vector2.Max(startLocal, currentLocal);
        Vector2 center = (min + max) * 0.5f;
        Vector2 size = max - min;

        dragRectTransform.anchoredPosition = center;
        dragRectTransform.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
    }

    // Convert screen coordinates to local UI coordinates for drag rectangle
    Vector2 ScreenToLocal(Vector2 screenPos)
    {
        if (dragRectParent == null)
        {
            return screenPos;
        }
        Camera uiCam = null;
        if (dragCanvas)
        {
            if (dragCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                uiCam = dragCanvas.worldCamera;
            else if (dragCanvas.renderMode == RenderMode.WorldSpace)
                uiCam = dragCanvas.worldCamera ? dragCanvas.worldCamera : mainCamera;
        }

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragRectParent, screenPos, uiCam, out local);
        return local;
    }

    // Finish drag, select all units inside box
    void CompleteDrag(Vector2 endScreenPosition)
    {
        dragging = false;
        if (dragRectImage != null)
        {
            dragRectImage.gameObject.SetActive(false);
        }

        Vector2 min = Vector2.Min(dragStart, endScreenPosition);
        Vector2 max = Vector2.Max(dragStart, endScreenPosition);
        Vector2 delta = max - min;

        // If user barely dragged, treat as a click
        if (delta.sqrMagnitude < 4f)
        {
            SelectByRaycast(dragAdditive);
            return;
        }

        dragResults.Clear();

        // Find all Units in scene and test if they fall inside selection box
        foreach (Unit u in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            if (!u) continue;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(u.transform.position);

            // Skip behind camera
            if (screenPos.z <= 0) continue;

            // Skip if outside selection bounds
            if (screenPos.x < min.x || screenPos.x > max.x) continue;
            if (screenPos.y < min.y || screenPos.y > max.y) continue;

            dragResults.Add(u);
        }

        if (!dragAdditive)
        {
            ClearSelection();
        }
        else
        {
            selection.RemoveAll(u => u == null);
        }

        // Mark selected
        foreach (Unit u in dragResults)
        {
            if (selection.Contains(u)) continue;
            selection.Add(u);
            u.SetSelected(true);
        }

        if (GameGlue.I) GameGlue.I.Hint("Selected " + selection.Count + " units");
    }

    // Single-click raycast select
    void SelectByRaycast(bool additive)
    {
        if (!additive)
        {
            ClearSelection();
        }
        else
        {
            selection.RemoveAll(u => u == null);
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);

        foreach (RaycastHit hit in hits)
        {
            Unit u = hit.collider.GetComponent<Unit>();
            if (u == null) u = hit.collider.GetComponentInParent<Unit>();

            if (u != null)
            {
                if (!selection.Contains(u))
                {
                    selection.Add(u);
                    u.SetSelected(true);
                }
                if (GameGlue.I)
                    GameGlue.I.Hint("Selected " + selection.Count + " unit" + (selection.Count == 1 ? "" : "s"));

                // Stop after first hit (closest)
                break;
            }
        }
    }

    // Clear all current selections
    void ClearSelection()
    {
        foreach (Unit u in selection)
        {
            if (u) u.SetSelected(false);
        }
        selection.Clear();
    }

    // Handle right-click movement command
    void HandleMovement()
    {
        if (Input.GetMouseButtonDown(1) && selection.Count > 0)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                selection.RemoveAll(u => u == null);
                foreach (Unit u in selection)
                {
                    u.MoveTo(hit.point);
                }
                if (GameGlue.I) GameGlue.I.Hint("Moving " + selection.Count + " units");
            }
        }
    }
}
