using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MiniWoW
{
    public class AbilityBarUI : MonoBehaviour
    {
        public AbilitySystem abilitySystem;
        public Canvas canvas;
        public Font defaultFont;

        private class SlotWidgets
        {
            public Image icon;
            public Image coolOverlay;
            public Text label;
        }

        private readonly List<SlotWidgets> slots = new List<SlotWidgets>();

        private void Start()
        {
            if (!abilitySystem) abilitySystem = FindObjectOfType<AbilitySystem>();
            if (!canvas)
            {
                var go = new GameObject("AbilityBarCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = go.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = go.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }
            BuildBar();
        }

        private void BuildBar()
        {
            var root = new GameObject("AbilityBar", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 20f);
            rt.sizeDelta = new Vector2(600f, 80f);

            float slotSize = 64f;
            float gap = 10f;
            for (int i = 0; i < 5; i++)
            {
                var slotGO = new GameObject($"Slot{i+1}", typeof(Image));
                slotGO.transform.SetParent(root.transform, false);
                var srt = slotGO.GetComponent<RectTransform>();
                srt.sizeDelta = new Vector2(slotSize, slotSize);
                srt.anchoredPosition = new Vector2((i - 2) * (slotSize + gap), 0f);
                var bg = slotGO.GetComponent<Image>();
                bg.color = new Color(0f, 0f, 0f, 0.5f);

                var iconGO = new GameObject("Icon", typeof(Image));
                iconGO.transform.SetParent(slotGO.transform, false);
                var irt = iconGO.GetComponent<RectTransform>();
                irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one; irt.offsetMin = Vector2.zero; irt.offsetMax = Vector2.zero;
                var icon = iconGO.GetComponent<Image>();
                icon.color = Color.white;

                var coolGO = new GameObject("Cooldown", typeof(Image));
                coolGO.transform.SetParent(slotGO.transform, false);
                var crt = coolGO.GetComponent<RectTransform>();
                crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one; crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;
                var cool = coolGO.GetComponent<Image>();
                cool.fillMethod = Image.FillMethod.Radial360;
                cool.type = Image.Type.Filled;
                cool.fillOrigin = (int)Image.Origin360.Top;
                cool.fillClockwise = false;
                cool.fillAmount = 0f;
                cool.color = new Color(0f, 0f, 0f, 0.6f);

                var labelGO = new GameObject("Key", typeof(Text));
                labelGO.transform.SetParent(slotGO.transform, false);
                var lrt = labelGO.GetComponent<RectTransform>();
                lrt.anchorMin = new Vector2(1f, 0f); lrt.anchorMax = new Vector2(1f, 0f);
                lrt.anchoredPosition = new Vector2(10f, -10f);
                lrt.sizeDelta = new Vector2(40f, 24f);
                var txt = labelGO.GetComponent<Text>();
                txt.text = (i + 1).ToString();
                txt.alignment = TextAnchor.LowerRight;
                txt.fontSize = 16;
                txt.color = Color.white;
                if (defaultFont) txt.font = defaultFont;

                slots.Add(new SlotWidgets{ icon = icon, coolOverlay = cool, label = txt });
            }

            RefreshIcons();
        }

        private void RefreshIcons()
        {
            if (!abilitySystem) return;
            for (int i = 0; i < slots.Count; i++)
            {
                var def = (i < abilitySystem.loadout.Length) ? abilitySystem.loadout[i] : null;
                var w = slots[i];
                if (def && def.icon) w.icon.sprite = def.icon;
                else w.icon.sprite = null;
                w.icon.enabled = def != null;
                w.coolOverlay.fillAmount = 0f;
            }
        }

        private void Update()
        {
            if (!abilitySystem) return;
            for (int i = 0; i < slots.Count; i++)
            {
                var def = (i < abilitySystem.loadout.Length) ? abilitySystem.loadout[i] : null;
                var w = slots[i];
                if (!def) { w.coolOverlay.fillAmount = 0f; continue; }
                float rem = abilitySystem.GetCooldownRemaining(def);
                w.coolOverlay.fillAmount = rem > 0f ? rem / Mathf.Max(0.001f, def.cooldown) : 0f;
            }
        }
    }
}
