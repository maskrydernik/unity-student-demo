using UnityEngine;
using UnityEngine.UI;

namespace MiniWoW
{
    public class PlayerFrameUI : MonoBehaviour
    {
        public Health playerHealth;
        public Canvas canvas;
        public Font defaultFont;

        private Text nameText;
        private Text hpText;
        private Image hpBG;
        private Image hpFill;

        private void Start()
        {
            if (!playerHealth) playerHealth = FindFirstObjectByType<Health>();
            if (!canvas)
            {
                var go = new GameObject("PlayerFrameCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = go.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                var scaler = go.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }
            Build();
            Debug.Log("[PlayerFrameUI] Initialized");
        }

        private void Build()
        {
            var root = new GameObject("PlayerFrame", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(20f, -20f);
            rt.sizeDelta = new Vector2(250f, 70f);

            // Background panel
            var bgGO = new GameObject("Background", typeof(Image));
            bgGO.transform.SetParent(root.transform, false);
            var bgrt = bgGO.GetComponent<RectTransform>();
            bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one;
            bgrt.offsetMin = Vector2.zero; bgrt.offsetMax = Vector2.zero;
            var bgImg = bgGO.GetComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.6f);

            var nameGO = new GameObject("Name", typeof(Text));
            nameGO.transform.SetParent(root.transform, false);
            var nrt = nameGO.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0f, 1f); nrt.anchorMax = new Vector2(1f, 1f);
            nrt.anchoredPosition = new Vector2(0f, -5f); nrt.sizeDelta = new Vector2(-10f, 20f);
            nameText = nameGO.GetComponent<Text>();
            nameText.alignment = TextAnchor.UpperLeft; nameText.fontSize = 16; nameText.color = Color.white;
            nameText.text = "Player";
            if (defaultFont) nameText.font = defaultFont;

            var hpTextGO = new GameObject("HPText", typeof(Text));
            hpTextGO.transform.SetParent(root.transform, false);
            var htrt = hpTextGO.GetComponent<RectTransform>();
            htrt.anchorMin = new Vector2(0f, 0f); htrt.anchorMax = new Vector2(1f, 0f);
            htrt.anchoredPosition = new Vector2(0f, 5f); htrt.sizeDelta = new Vector2(-10f, 20f);
            hpText = hpTextGO.GetComponent<Text>();
            hpText.alignment = TextAnchor.LowerRight; hpText.fontSize = 14; hpText.color = Color.white;
            if (defaultFont) hpText.font = defaultFont;

            var barBG = new GameObject("HPBG", typeof(Image));
            barBG.transform.SetParent(root.transform, false);
            var bgrt2 = barBG.GetComponent<RectTransform>();
            bgrt2.anchorMin = new Vector2(0f, 0f); bgrt2.anchorMax = new Vector2(1f, 0f);
            bgrt2.anchoredPosition = new Vector2(0f, 25f); bgrt2.sizeDelta = new Vector2(-10f, 18f);
            hpBG = barBG.GetComponent<Image>();
            hpBG.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var barFill = new GameObject("HPFill", typeof(Image));
            barFill.transform.SetParent(barBG.transform, false);
            var frt = barFill.GetComponent<RectTransform>();
            frt.anchorMin = new Vector2(0f, 0f); frt.anchorMax = new Vector2(0f, 1f);
            frt.offsetMin = Vector2.zero; frt.offsetMax = new Vector2(0f, 0f);
            hpFill = barFill.GetComponent<Image>();
            hpFill.color = Color.green;
        }

        private void Update()
        {
            if (!playerHealth) return;
            
            float current = playerHealth.Current;
            float max = playerHealth.Max;
            float percent = Mathf.Approximately(max, 0f) ? 0f : current / max;
            
            hpText.text = $"{current:F0} / {max:F0}";
            SetFill(percent);
        }

        private void SetFill(float f)
        {
            f = Mathf.Clamp01(f);
            var rt = hpFill.rectTransform;
            rt.anchorMax = new Vector2(f, 1f);
        }
    }
}
