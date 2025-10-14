using UnityEngine;
using UnityEngine.UI;

namespace MiniWoW
{
    /// <summary>
    /// Simple player spawner that shows selection UI and spawns the chosen player
    /// </summary>
    public class SimplePlayerSpawner : MonoBehaviour
    {
        public GameObject[] playerPrefabs;
        private Canvas selectionCanvas;

        private void Start()
        {
            Debug.Log($"[SimplePlayerSpawner] Starting... Player prefabs count: {(playerPrefabs != null ? playerPrefabs.Length : 0)}");
            if (playerPrefabs == null || playerPrefabs.Length == 0)
            {
                Debug.LogError("[SimplePlayerSpawner] No player prefabs assigned!");
                return;
            }
            BuildPlayerSelectionUI();
            Debug.Log("[SimplePlayerSpawner] Selection UI built!");
        }

        private void BuildPlayerSelectionUI()
        {
            // Create selection canvas
            var go = new GameObject("SelectionCanvas");
            go.transform.SetParent(transform);
            selectionCanvas = go.AddComponent<Canvas>();
            selectionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            selectionCanvas.sortingOrder = 500; // On top of everything
            
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            go.AddComponent<GraphicRaycaster>();

            // Panel background
            var panel = new GameObject("Panel");
            panel.transform.SetParent(go.transform, false);
            var prt = panel.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.5f);
            prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(600f, 200f);
            prt.anchoredPosition = Vector2.zero;
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.8f);

            // Title
            var title = new GameObject("Title");
            title.transform.SetParent(panel.transform, false);
            var trt = title.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 1f);
            trt.anchorMax = new Vector2(0.5f, 1f);
            trt.pivot = new Vector2(0.5f, 1f);
            trt.anchoredPosition = new Vector2(0f, -10f);
            trt.sizeDelta = new Vector2(500f, 40f);
            var titleText = title.AddComponent<Text>();
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontSize = 24;
            titleText.text = "Choose Your Player";
            titleText.color = Color.white;
            titleText.fontStyle = FontStyle.Bold;

            // Create buttons
            int count = playerPrefabs != null ? playerPrefabs.Length : 0;
            for (int i = 0; i < count; i++)
            {
                CreateButton(panel.transform, i, count, playerPrefabs[i].name);
            }
        }

        private void CreateButton(Transform parent, int index, int total, string prefabName)
        {
            var btnGO = new GameObject($"Button_{prefabName}");
            btnGO.transform.SetParent(parent, false);
            
            var brt = btnGO.AddComponent<RectTransform>();
            brt.sizeDelta = new Vector2(160f, 60f);
            float spacing = 180f;
            float offset = (index - (total - 1) / 2f) * spacing;
            brt.anchoredPosition = new Vector2(offset, -90f);
            
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.4f, 0.8f, 1f);
            
            var button = btnGO.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.4f, 0.8f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.5f, 1f, 1f);
            colors.pressedColor = new Color(0.1f, 0.3f, 0.6f, 1f);
            button.colors = colors;

            // Button label
            var label = new GameObject("Label");
            label.transform.SetParent(btnGO.transform, false);
            var lrt = label.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            
            var labelText = label.AddComponent<Text>();
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.text = prefabName;
            labelText.fontStyle = FontStyle.Bold;

            // Button click handler
            int idx = index;
            button.onClick.AddListener(() => SpawnPlayer(idx));
        }

        private void SpawnPlayer(int index)
        {
            if (playerPrefabs == null || index >= playerPrefabs.Length) return;

            Debug.Log($"[SimplePlayerSpawner] Spawning {playerPrefabs[index].name}");

            // Spawn player
            var player = Instantiate(playerPrefabs[index], new Vector3(0f, 1f, 0f), Quaternion.identity);
            player.name = "Player";

            // Destroy old camera if it exists
            var oldCam = GameObject.Find("Main Camera");
            if (oldCam)
            {
                Debug.Log("[SimplePlayerSpawner] Destroying old Main Camera");
                Destroy(oldCam);
            }

            // Create new camera
            var camGO = new GameObject("MainCamera");
            var camera = camGO.AddComponent<Camera>();
            camera.tag = "MainCamera";
            var audioListener = camGO.AddComponent<AudioListener>();
            var camCtrl = camGO.AddComponent<CameraController>();
            
            Debug.Log("[SimplePlayerSpawner] Camera created and tagged as MainCamera");

            // Setup player motor
            var motor = player.GetComponent<PlayerMotor>();
            if (motor)
            {
                motor.cameraController = camCtrl;
                camCtrl.SetTarget(player.transform, motor);
                Debug.Log("[SimplePlayerSpawner] Camera controller linked to player motor");
            }
            else
            {
                Debug.LogWarning("[SimplePlayerSpawner] Player prefab missing PlayerMotor component!");
                // Still set camera to follow player
                camCtrl.SetTarget(player.transform, null);
            }

            // Destroy selection UI
            if (selectionCanvas != null)
            {
                Debug.Log("[SimplePlayerSpawner] Destroying selection UI canvas");
                Destroy(selectionCanvas.gameObject);
            }

            Debug.Log("[SimplePlayerSpawner] Player spawned! UI should now find player components automatically.");
            Debug.Log($"[SimplePlayerSpawner] Camera active: {camera.enabled}, AudioListener active: {audioListener.enabled}");
        }
    }
}
