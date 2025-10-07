using UnityEngine;
using UnityEngine.UI;

namespace MiniWoW
{
    /// <summary>
    /// Floating combat text that displays damage/healing numbers
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        public float lifetime = 1.5f;
        public float moveSpeed = 2f;
        public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private Text textComponent;
        private float elapsed = 0f;
        private Canvas canvas;

        public static void Create(Vector3 worldPosition, string text, Color color)
        {
            // Find or create canvas
            var canvas = FindFirstObjectByType<Canvas>();
            if (!canvas)
            {
                var go = new GameObject("FloatingTextCanvas", typeof(Canvas), typeof(CanvasScaler));
                canvas = go.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 200; // On top of everything
                var scaler = go.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            // Create floating text
            var textGO = new GameObject("FloatingText", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(canvas.transform, false);
            
            var ft = textGO.AddComponent<FloatingText>();
            ft.canvas = canvas;
            ft.textComponent = textGO.GetComponent<Text>();
            ft.textComponent.text = text;
            ft.textComponent.color = color;
            ft.textComponent.fontSize = 24;
            ft.textComponent.alignment = TextAnchor.MiddleCenter;
            ft.textComponent.fontStyle = FontStyle.Bold;

            // Add outline for readability
            var outline = textGO.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, 2);

            var rt = textGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 50);
            
            // Convert world position to screen position
            Camera cam = Camera.main;
            if (cam)
            {
                Vector3 screenPos = cam.WorldToScreenPoint(worldPosition + Vector3.up * 2f);
                rt.position = screenPos;
            }
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            
            // Move upward
            transform.position += Vector3.up * moveSpeed * 60f * Time.deltaTime;
            
            // Fade out
            float alpha = fadeCurve.Evaluate(elapsed / lifetime);
            Color c = textComponent.color;
            c.a = alpha;
            textComponent.color = c;
            
            // Destroy when done
            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
