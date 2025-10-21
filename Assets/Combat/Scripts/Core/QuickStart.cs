using UnityEngine;
using UnityEditor;

namespace MiniWoW
{
    public class QuickStart : MonoBehaviour
    {
        [Header("Quick Start Configuration")]
        public bool setupOnAwake = true;
        public bool showClassSelection = true;
        public bool spawnTrainingDummies = true;
        
        private void Awake()
        {
            if (setupOnAwake)
            {
                SetupQuickStart();
            }
        }
        
        [ContextMenu("Setup Quick Start")]
        public void SetupQuickStart()
        {
            Debug.Log("[QuickStart] Setting up quick start...");
            
            // 1. Create CombatSystemSetup if it doesn't exist
            CombatSystemSetup systemSetup = FindFirstObjectByType<CombatSystemSetup>();
            if (systemSetup == null)
            {
                GameObject setupGO = new GameObject("Combat System Setup");
                systemSetup = setupGO.AddComponent<CombatSystemSetup>();
            }
            
            // 2. Load default classes
            systemSetup.LoadDefaultClasses();
            
            // 3. Setup the complete system
            systemSetup.SetupCombatSystem();
            
            // 4. Show class selection if requested
            if (showClassSelection)
            {
                ClassSelectionUI classSelection = FindFirstObjectByType<ClassSelectionUI>();
                if (classSelection != null)
                {
                    classSelection.ShowClassSelection();
                }
            }
            
            Debug.Log("[QuickStart] Quick start setup complete!");
            Debug.Log("[QuickStart] Use the class selection UI to choose and spawn a class.");
            Debug.Log("[QuickStart] Controls: 1-5 for abilities, click to target, ESC to clear target.");
        }
        
        [ContextMenu("Create Example Classes")]
        public void CreateExampleClasses()
        {
            Debug.Log("[QuickStart] Creating example classes...");
            
            // This would create the example classes if they don't exist
            // For now, just log the available classes
            ClassTemplate[] classes = Resources.FindObjectsOfTypeAll<ClassTemplate>();
            Debug.Log($"[QuickStart] Found {classes.Length} class templates:");
            
            foreach (var classTemplate in classes)
            {
                Debug.Log($"- {classTemplate.className} ({classTemplate.classRole})");
            }
        }
        
        [ContextMenu("Test All Classes")]
        public void TestAllClasses()
        {
            Debug.Log("[QuickStart] Testing all classes...");
            
            CombatSceneSetup sceneSetup = FindFirstObjectByType<CombatSceneSetup>();
            if (sceneSetup != null && sceneSetup.availableClasses != null)
            {
                for (int i = 0; i < sceneSetup.availableClasses.Length; i++)
                {
                    if (sceneSetup.availableClasses[i] != null)
                    {
                        Debug.Log($"Spawning {sceneSetup.availableClasses[i].className}...");
                        sceneSetup.SpawnPlayerClass(i);
                    }
                }
            }
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(QuickStart))]
    public class QuickStartEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            QuickStart quickStart = (QuickStart)target;
            
            if (GUILayout.Button("Setup Quick Start"))
            {
                quickStart.SetupQuickStart();
            }
            
            if (GUILayout.Button("Create Example Classes"))
            {
                quickStart.CreateExampleClasses();
            }
            
            if (GUILayout.Button("Test All Classes"))
            {
                quickStart.TestAllClasses();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Instructions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Click 'Setup Quick Start' to initialize the system\n" +
                "2. Use the class selection UI to choose a class\n" +
                "3. Click 'Spawn Class' to create your character\n" +
                "4. Use keys 1-5 to cast abilities\n" +
                "5. Click on enemies to target them",
                MessageType.Info
            );
        }
    }
    #endif
}
