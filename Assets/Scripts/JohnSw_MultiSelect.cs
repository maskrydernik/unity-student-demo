// JohnSw_MultiSelect.cs
// Minimal working selection: Click to select, Shift+drag for multi-select, Right-click to move.
// DISABLE Manager.cs if you use this script!
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JohnSw_MultiSelect : MonoBehaviour
{
    public Image dragRectImage;
    public List<Unit> selection = new List<Unit>();

    Camera mainCamera;
    Canvas dragCanvas;
    RectTransform dragRectTransform;
    RectTransform dragRectParent;
    Vector2 dragStart;
    Vector2 dragStartLocal;
    bool dragging;
    bool dragAdditive;
    readonly List<Unit> dragResults = new List<Unit>();

    void Start()
    {
        mainCamera = Camera.main;

        if (dragRectImage != null)
        {
            // Setup drag rect properly
            dragRectImage.gameObject.SetActive(false);
            dragRectTransform = dragRectImage.rectTransform;
            dragRectParent = dragRectTransform.parent as RectTransform;
            dragCanvas = dragRectImage.canvas;
            if (dragCanvas == null)
            {
                dragCanvas = dragRectImage.GetComponentInParent<Canvas>();
            }
            dragRectTransform.pivot = new Vector2(0.5f, 0.5f);
            dragRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            dragRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            dragRectTransform.anchoredPosition = Vector2.zero;
            dragRectTransform.sizeDelta = Vector2.zero;
            dragRectTransform.SetAsLastSibling();
            dragRectImage.raycastTarget = false;
        }
    }

    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        HandleSelection();
        HandleMovement();
    }

    void HandleSelection()
    {
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (dragging)
        {
            UpdateDragVisual(Input.mousePosition);

            if (Input.GetMouseButtonUp(0))
            {
                CompleteDrag(Input.mousePosition);
            }
            return;
        }

        // Start drag when Shift is held
        if (shift && Input.GetMouseButtonDown(0))
        {
            BeginDrag(true);
            return;
        }

        // Allow drag without shift as a fresh selection
        if (!shift && Input.GetMouseButtonDown(0))
        {
            BeginDrag(false);
            return;
        }
    }

    void BeginDrag(bool additive)
    {
        dragging = true;
        dragAdditive = additive;
        dragStart = Input.mousePosition;
        if (!additive)
        {
            ClearSelection();
        }
        else
        {
            selection.RemoveAll(u => u == null);
        }

        if (dragRectImage != null)
        {
            dragRectImage.gameObject.SetActive(true);
            dragStartLocal = ScreenToLocal(dragStart);
            UpdateDragVisual(dragStart);
        }
    }

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

        // Treat tiny drags as clicks
        if (delta.sqrMagnitude < 4f)
        {
            SelectByRaycast(dragAdditive);
            return;
        }

        dragResults.Clear();
        foreach (Unit u in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            if (!u) continue;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(u.transform.position);
            if (screenPos.z <= 0) continue;
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

        foreach (Unit u in dragResults)
        {
            if (selection.Contains(u)) continue;
            selection.Add(u);
            u.SetSelected(true);
        }

        if (GameGlue.I) GameGlue.I.Hint("Selected " + selection.Count + " units");
    }

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
                if (GameGlue.I) GameGlue.I.Hint("Selected " + selection.Count + " unit" + (selection.Count == 1 ? "" : "s"));
                break;
            }
        }
    }

    void ClearSelection()
    {
        foreach (Unit u in selection)
        {
            if (u) u.SetSelected(false);
        }
        selection.Clear();
    }

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
