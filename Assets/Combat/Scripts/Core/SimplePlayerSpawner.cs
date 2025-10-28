using UnityEngine;
using UnityEngine.UI;

namespace MiniWoW
{
    /// <summary>
    /// Simple player spawner that shows selection UI and spawns the chosen player
    /// </summary>
    public class SimplePlayerSpawner : MonoBehaviour
    {
		public ClassTemplate[] classTemplates; // auto-populated at runtime
        private Canvas selectionCanvas;
		private Font defaultFont;

		private void Start()
        {
			LoadAllClassTemplates();
			Debug.Log($"[SimplePlayerSpawner] Starting... Found class templates: {(classTemplates != null ? classTemplates.Length : 0)}");
            BuildPlayerSelectionUI();
            Debug.Log("[SimplePlayerSpawner] Selection UI built!");
        }

		private void LoadAllClassTemplates()
		{
			Debug.Log("[SimplePlayerSpawner] Loading class templates...");
#if UNITY_EDITOR
			// Editor: use AssetDatabase to find all templates in the project
			var guids = UnityEditor.AssetDatabase.FindAssets("t:ClassTemplate");
			Debug.Log($"[SimplePlayerSpawner] Found {guids.Length} ClassTemplate assets");
			var list = new System.Collections.Generic.List<ClassTemplate>();
			foreach (var guid in guids)
			{
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ClassTemplate>(path);
				if (asset) 
				{
					list.Add(asset);
					Debug.Log($"[SimplePlayerSpawner] Loaded template: {asset.className}");
				}
			}
			classTemplates = list.ToArray();
			Debug.Log($"[SimplePlayerSpawner] Loaded {classTemplates.Length} templates total");
			
			if (classTemplates.Length == 0)
			{
				// Try to create example templates, then reload
				Debug.Log("[SimplePlayerSpawner] No class templates found. Creating examples...");
				ClassPrefabGenerator.CreateExampleClassTemplates();
				guids = UnityEditor.AssetDatabase.FindAssets("t:ClassTemplate");
				Debug.Log($"[SimplePlayerSpawner] After creating examples, found {guids.Length} assets");
				list.Clear();
				foreach (var guid in guids)
				{
					var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
					var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ClassTemplate>(path);
					if (asset) 
					{
						list.Add(asset);
						Debug.Log($"[SimplePlayerSpawner] Reloaded template: {asset.className}");
					}
				}
				classTemplates = list.ToArray();
				Debug.Log($"[SimplePlayerSpawner] After reload, have {classTemplates.Length} templates");
			}
#else
			// Runtime/Build: load from Resources if present
			classTemplates = Resources.LoadAll<ClassTemplate>("");
			Debug.Log($"[SimplePlayerSpawner] Loaded {classTemplates.Length} templates from Resources");
#endif
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

            // Main panel
            var panel = new GameObject("MainPanel");
            panel.transform.SetParent(go.transform, false);
            var prt = panel.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.5f);
            prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(400f, 500f);
            prt.anchoredPosition = Vector2.zero;
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

			// Load a default font
			defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

			// Title
            var title = new GameObject("Title");
            title.transform.SetParent(panel.transform, false);
            var trt = title.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0f, 1f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.sizeDelta = new Vector2(0f, 60f);
            trt.anchoredPosition = new Vector2(0f, -30f);
			var titleText = title.AddComponent<Text>();
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontSize = 24;
            titleText.text = "Choose Your Class";
            titleText.color = Color.white;
            titleText.fontStyle = FontStyle.Bold;
			if (defaultFont) titleText.font = defaultFont;

            // Button container with vertical layout
            var buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(panel.transform, false);
            var brt = buttonContainer.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0f, 0f);
            brt.anchorMax = new Vector2(1f, 1f);
            brt.offsetMin = new Vector2(20f, 80f);
            brt.offsetMax = new Vector2(-20f, -20f);
            
            var layoutGroup = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;

			// Create buttons
			Debug.Log($"[SimplePlayerSpawner] About to create buttons. classTemplates is {(classTemplates != null ? "not null" : "NULL")}");
			if (classTemplates != null)
			{
				Debug.Log($"[SimplePlayerSpawner] classTemplates.Length = {classTemplates.Length}");
			}
			
			if (classTemplates != null && classTemplates.Length > 0)
			{
				Debug.Log($"[SimplePlayerSpawner] Creating {classTemplates.Length} class buttons");
				for (int i = 0; i < classTemplates.Length; i++)
				{
					string className = classTemplates[i] ? classTemplates[i].className : $"Class {i+1}";
					Debug.Log($"[SimplePlayerSpawner] Creating button for: {className}");
					CreateButton(buttonContainer.transform, i, className);
				}
				Debug.Log($"[SimplePlayerSpawner] Finished creating buttons");
			}
			else
			{
				Debug.LogError("[SimplePlayerSpawner] No class templates found! Place ClassTemplate assets in the project or under Resources.");
			}
			
			Debug.Log($"[SimplePlayerSpawner] UI creation complete. Canvas active: {selectionCanvas.gameObject.activeInHierarchy}");
        }

		private void CreateButton(Transform parent, int index, string labelTextValue)
        {
			Debug.Log($"[SimplePlayerSpawner] Creating button {index}: {labelTextValue}");
			var btnGO = new GameObject($"Button_{labelTextValue}");
            btnGO.transform.SetParent(parent, false);
            
            var brt = btnGO.AddComponent<RectTransform>();
            brt.sizeDelta = new Vector2(0f, 50f); // Height only, width controlled by layout
            brt.anchorMin = new Vector2(0f, 0f);
            brt.anchorMax = new Vector2(1f, 0f);
            brt.anchoredPosition = Vector2.zero;
            
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
			labelText.text = labelTextValue;
			labelText.fontStyle = FontStyle.Bold;
			if (defaultFont) labelText.font = defaultFont;

            // Button click handler
            int idx = index;
            button.onClick.AddListener(() => SpawnPlayerFromTemplate(idx));
            Debug.Log($"[SimplePlayerSpawner] Button {labelTextValue} created");
        }

		private void SpawnPlayerFromTemplate(int index)
		{
			if (classTemplates == null || index >= classTemplates.Length) return;
			var template = classTemplates[index];
			if (!template)
			{
				Debug.LogWarning($"[SimplePlayerSpawner] Template at index {index} is null");
				return;
			}

			Debug.Log($"[SimplePlayerSpawner] Spawning from template {template.className}");
			var generated = ClassPrefabGenerator.GenerateClassPrefab(template);
			if (!generated)
			{
				Debug.LogError("[SimplePlayerSpawner] Failed to generate class prefab");
				return;
			}

			// Use the generated object directly (avoid double-spawn)
			var player = generated;
			player.transform.position = new Vector3(0f, 1f, 0f);
			player.transform.rotation = Quaternion.identity;
			player.name = $"{template.className} Player";

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

			PostSpawnSetup(player, camCtrl);

			// Destroy selection UI
			if (selectionCanvas != null)
			{
				Debug.Log("[SimplePlayerSpawner] Destroying selection UI canvas");
				Destroy(selectionCanvas.gameObject);
			}

			Debug.Log("[SimplePlayerSpawner] Player spawned from template!");
		}

		private void PostSpawnSetup(GameObject player, CameraController camCtrl)
		{
			// Ensure TargetingSystem exists
			var targeting = player.GetComponent<TargetingSystem>();
			if (!targeting) targeting = player.AddComponent<TargetingSystem>();

			// Setup player motor and camera follow
			var motor = player.GetComponent<PlayerMotor>();
			if (motor)
			{
				motor.cameraController = camCtrl;
				camCtrl.SetTarget(player.transform, motor);
				Debug.Log("[SimplePlayerSpawner] Camera controller linked to player motor");
			}
			else
			{
				camCtrl.SetTarget(player.transform, null);
			}

			// Bind UI
			var abilitySystem = player.GetComponent<AbilitySystem>();
			var abilityBar = FindFirstObjectByType<AbilityBarUI>();
			if (abilitySystem && abilityBar)
			{
				abilityBar.Bind(abilitySystem);
			}
			var health = player.GetComponent<Health>();
			var playerFrame = FindFirstObjectByType<PlayerFrameUI>();
			if (health && playerFrame)
			{
				playerFrame.playerHealth = health;
			}
		}
    }
}
