using UnityEngine;
using UnityEngine.UI;

namespace MiniWoW
{
    public class TargetFrameUI : MonoBehaviour
    {
        public TargetingSystem targeting;
        public Canvas canvas;
        public Font defaultFont;

        private Text nameText;
        private Image hpBG;
        private Image hpFill;

        private void Start()
        {
            if (!targeting) targeting = FindFirstObjectByType<TargetingSystem>();
            if (!canvas)
            {
                var go = new GameObject("TargetFrameCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = go.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100; // Ensure it's on top
                var scaler = go.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }
            Build();
            Debug.Log("[TargetFrameUI] Initialized");
        }

        private void Build()
        {
            var root = new GameObject("TargetFrame", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -20f);
            rt.sizeDelta = new Vector2(250f, 60f);

            var nameGO = new GameObject("Name", typeof(Text));
            nameGO.transform.SetParent(root.transform, false);
            var nrt = nameGO.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0f, 1f); nrt.anchorMax = new Vector2(1f, 1f);
            nrt.anchoredPosition = new Vector2(0f, -10f); nrt.sizeDelta = new Vector2(0f, 24f);
            nameText = nameGO.GetComponent<Text>();
            nameText.alignment = TextAnchor.UpperCenter; nameText.fontSize = 18; nameText.color = Color.white;
            if (defaultFont) nameText.font = defaultFont;

            var barBG = new GameObject("HPBG", typeof(Image));
            barBG.transform.SetParent(root.transform, false);
            var bgrt = barBG.GetComponent<RectTransform>();
            bgrt.anchorMin = new Vector2(0f, 0f); bgrt.anchorMax = new Vector2(1f, 0f);
            bgrt.anchoredPosition = new Vector2(0f, 10f); bgrt.sizeDelta = new Vector2(0f, 16f);
            hpBG = barBG.GetComponent<Image>();
            hpBG.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // More visible background

            var barFill = new GameObject("HPFill", typeof(Image));
            barFill.transform.SetParent(barBG.transform, false);
            var frt = barFill.GetComponent<RectTransform>();
            frt.anchorMin = new Vector2(0f, 0f); frt.anchorMax = new Vector2(0f, 1f);
            frt.offsetMin = Vector2.zero; frt.offsetMax = new Vector2(0f, 0f);
            hpFill = barFill.GetComponent<Image>();
            hpFill.color = Color.green; // Green for health
        }

        private void Update()
        {
            var t = targeting ? targeting.Current : null;
            if (!t)
            {
                nameText.text = "";
                SetFill(0f);
                return;
            }
            nameText.text = $"{t.DisplayName} [{t.Faction}]";
            if (t.Health) SetFill(Mathf.Approximately(t.Health.Max,0f) ? 0f : t.Health.Current / t.Health.Max);
        }

        private void SetFill(float f)
        {
            f = Mathf.Clamp01(f);
            var rt = hpFill.rectTransform;
            rt.anchorMax = new Vector2(f, 1f);
        }
    }
}
