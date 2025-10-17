using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MiniWoW
{
    public class CombatSystemSetup : MonoBehaviour
    {
        [Header("Quick Setup")]
        public bool autoSetupOnStart = true;
        public bool createClassSelectionUI = true;
        public bool spawnDefaultPlayer = true;
        
        [Header("Class Templates")]
        public ClassTemplate[] defaultClasses;
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupCombatSystem();
            }
        }
        
        [ContextMenu("Setup Complete Combat System")]
        public void SetupCombatSystem()
        {
            Debug.Log("[CombatSystemSetup] Setting up complete combat system...");
            
            // 1. Setup the scene
            SetupScene();
            
            // 2. Create class selection UI if requested
            if (createClassSelectionUI)
            {
                CreateClassSelectionUI();
            }
            
            // 3. Spawn default player if requested
            if (spawnDefaultPlayer && defaultClasses != null && defaultClasses.Length > 0)
            {
                SpawnDefaultPlayer();
            }
            
            Debug.Log("[CombatSystemSetup] Combat system setup complete!");
        }
        
        private void SetupScene()
        {
            // Find or create CombatSceneSetup
            CombatSceneSetup sceneSetup = FindFirstObjectByType<CombatSceneSetup>();
            if (sceneSetup == null)
            {
                GameObject setupGO = new GameObject("Combat Scene Setup");
                sceneSetup = setupGO.AddComponent<CombatSceneSetup>();
            }
            
            // Configure available classes
            if (defaultClasses != null && defaultClasses.Length > 0)
            {
                sceneSetup.availableClasses = defaultClasses;
            }
            
            // Setup the scene
            sceneSetup.SetupScene();
        }
        
        private void CreateClassSelectionUI()
        {
            // Create main canvas if it doesn't exist
            Canvas mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                GameObject canvasGO = new GameObject("Main Canvas");
                mainCanvas = canvasGO.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                mainCanvas.sortingOrder = 100;
                
                var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            // Create class selection UI
            GameObject classSelectionGO = new GameObject("Class Selection UI");
            classSelectionGO.transform.SetParent(mainCanvas.transform, false);
            
            ClassSelectionUI classSelectionUI = classSelectionGO.AddComponent<ClassSelectionUI>();
            classSelectionUI.availableClasses = defaultClasses;
            
            // Create UI layout
            CreateClassSelectionLayout(classSelectionGO, classSelectionUI);
        }
        
        private void CreateClassSelectionLayout(GameObject parent, ClassSelectionUI classSelectionUI)
        {
            // Create main panel
            GameObject panel = new GameObject("Class Selection Panel");
            panel.transform.SetParent(parent.transform, false);
            
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            var panelImage = panel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            // Create title
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(panel.transform, false);
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.8f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            var titleText = titleGO.AddComponent<UnityEngine.UI.Text>();
            titleText.text = "Choose Your Class";
            titleText.fontSize = 32;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            // Create class buttons container
            GameObject buttonsContainer = new GameObject("Class Buttons Container");
            buttonsContainer.transform.SetParent(panel.transform, false);
            var buttonsRect = buttonsContainer.AddComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0, 0.3f);
            buttonsRect.anchorMax = new Vector2(1, 0.8f);
            buttonsRect.offsetMin = Vector2.zero;
            buttonsRect.offsetMax = Vector2.zero;
            
            classSelectionUI.classButtonContainer = buttonsContainer.transform;
            
            // Create class info panel
            GameObject infoPanel = new GameObject("Class Info Panel");
            infoPanel.transform.SetParent(panel.transform, false);
            var infoRect = infoPanel.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0, 0);
            infoRect.anchorMax = new Vector2(1, 0.3f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;
            
            var infoImage = infoPanel.AddComponent<UnityEngine.UI.Image>();
            infoImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Create class name text
            GameObject classNameGO = new GameObject("Class Name");
            classNameGO.transform.SetParent(infoPanel.transform, false);
            var classNameRect = classNameGO.AddComponent<RectTransform>();
            classNameRect.anchorMin = new Vector2(0, 0.7f);
            classNameRect.anchorMax = new Vector2(0.5f, 1);
            classNameRect.offsetMin = Vector2.zero;
            classNameRect.offsetMax = Vector2.zero;
            
            var classNameText = classNameGO.AddComponent<UnityEngine.UI.Text>();
            classNameText.text = "Select a class";
            classNameText.fontSize = 24;
            classNameText.color = Color.white;
            classNameText.alignment = TextAnchor.MiddleCenter;
            classNameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            classSelectionUI.classNameText = classNameText;
            
            // Create class description text
            GameObject descGO = new GameObject("Class Description");
            descGO.transform.SetParent(infoPanel.transform, false);
            var descRect = descGO.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(1, 0.7f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;
            
            var descText = descGO.AddComponent<UnityEngine.UI.Text>();
            descText.text = "Choose a class to see its description and stats.";
            descText.fontSize = 14;
            descText.color = Color.white;
            descText.alignment = TextAnchor.UpperLeft;
            descText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            classSelectionUI.classDescriptionText = descText;
            
            // Create spawn button
            GameObject spawnButtonGO = new GameObject("Spawn Button");
            spawnButtonGO.transform.SetParent(panel.transform, false);
            var spawnRect = spawnButtonGO.AddComponent<RectTransform>();
            spawnRect.anchorMin = new Vector2(0.7f, 0.05f);
            spawnRect.anchorMax = new Vector2(0.95f, 0.25f);
            spawnRect.offsetMin = Vector2.zero;
            spawnRect.offsetMax = Vector2.zero;
            
            var spawnButton = spawnButtonGO.AddComponent<UnityEngine.UI.Button>();
            var spawnButtonImage = spawnButtonGO.AddComponent<UnityEngine.UI.Image>();
            spawnButtonImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
            
            var spawnButtonText = spawnButtonGO.AddComponent<UnityEngine.UI.Text>();
            spawnButtonText.text = "Spawn Class";
            spawnButtonText.fontSize = 18;
            spawnButtonText.color = Color.white;
            spawnButtonText.alignment = TextAnchor.MiddleCenter;
            spawnButtonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            classSelectionUI.spawnButton = spawnButton;
            
            // Create close button
            GameObject closeButtonGO = new GameObject("Close Button");
            closeButtonGO.transform.SetParent(panel.transform, false);
            var closeRect = closeButtonGO.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.05f, 0.05f);
            closeRect.anchorMax = new Vector2(0.3f, 0.25f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            
            var closeButton = closeButtonGO.AddComponent<UnityEngine.UI.Button>();
            var closeButtonImage = closeButtonGO.AddComponent<UnityEngine.UI.Image>();
            closeButtonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            
            var closeButtonText = closeButtonGO.AddComponent<UnityEngine.UI.Text>();
            closeButtonText.text = "Close";
            closeButtonText.fontSize = 18;
            closeButtonText.color = Color.white;
            closeButtonText.alignment = TextAnchor.MiddleCenter;
            closeButtonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            classSelectionUI.closeButton = closeButton;
        }
        
        private void SpawnDefaultPlayer()
        {
            CombatSceneSetup sceneSetup = FindFirstObjectByType<CombatSceneSetup>();
            if (sceneSetup != null && defaultClasses != null && defaultClasses.Length > 0)
            {
                sceneSetup.SpawnPlayerClass(0); // Spawn first class by default
            }
        }
        
        [ContextMenu("Load Default Classes")]
        public void LoadDefaultClasses()
        {
            // Find all class templates in the project
            string[] guids = AssetDatabase.FindAssets("t:ClassTemplate");
            List<ClassTemplate> classes = new List<ClassTemplate>();
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ClassTemplate template = AssetDatabase.LoadAssetAtPath<ClassTemplate>(path);
                if (template != null)
                {
                    classes.Add(template);
                }
            }
            
            defaultClasses = classes.ToArray();
            Debug.Log($"[CombatSystemSetup] Loaded {defaultClasses.Length} class templates");
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(CombatSystemSetup))]
    public class CombatSystemSetupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            CombatSystemSetup setup = (CombatSystemSetup)target;
            
            if (GUILayout.Button("Setup Complete Combat System"))
            {
                setup.SetupCombatSystem();
            }
            
            if (GUILayout.Button("Load Default Classes"))
            {
                setup.LoadDefaultClasses();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Class Templates", EditorStyles.boldLabel);
            
            if (setup.defaultClasses != null)
            {
                for (int i = 0; i < setup.defaultClasses.Length; i++)
                {
                    if (setup.defaultClasses[i] != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{i + 1}. {setup.defaultClasses[i].className}");
                        if (GUILayout.Button("Select", GUILayout.Width(60)))
                        {
                            Selection.activeObject = setup.defaultClasses[i];
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }
    }
    #endif
}
