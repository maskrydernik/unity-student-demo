using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MiniWoW
{
    public class CombatSceneSetup : MonoBehaviour
    {
        [Header("Scene Configuration")]
        public ClassTemplate[] availableClasses;
        public Transform playerSpawnPoint;
        public Transform[] enemySpawnPoints;
        public GameObject trainingDummyPrefab;
        
        [Header("UI Configuration")]
        public Canvas mainCanvas;
        public AbilityBarUI abilityBarUI;
        public PlayerFrameUI playerFrameUI;
        public TargetFrameUI targetFrameUI;
        
        [Header("Camera Configuration")]
        public Camera mainCamera;
        public CameraController cameraController;
        
        private void Start()
        {
            SetupScene();
        }
        
        [ContextMenu("Setup Combat Scene")]
        public void SetupScene()
        {
            Debug.Log("[CombatSceneSetup] Setting up combat scene...");
            
            // Find or create main camera
            SetupCamera();
            
            // Find or create player spawn point
            SetupPlayerSpawn();
            
            // Setup enemy spawn points
            SetupEnemySpawns();
            
            // Setup UI
            SetupUI();
            
            // Create training dummies
            CreateTrainingDummies();
            
            Debug.Log("[CombatSceneSetup] Scene setup complete!");
        }
        
        private void SetupCamera()
        {
            if (!mainCamera)
            {
                mainCamera = Camera.main;
                if (!mainCamera)
                {
                    GameObject cameraGO = new GameObject("Main Camera");
                    mainCamera = cameraGO.AddComponent<Camera>();
                    mainCamera.tag = "MainCamera";
                }
            }
            
            if (!cameraController)
            {
                cameraController = mainCamera.GetComponent<CameraController>();
                if (!cameraController)
                {
                    cameraController = mainCamera.gameObject.AddComponent<CameraController>();
                }
            }
            
            // Position camera for good combat view
            mainCamera.transform.position = new Vector3(0, 8, -10);
            mainCamera.transform.rotation = Quaternion.Euler(30, 0, 0);
        }
        
        private void SetupPlayerSpawn()
        {
            if (!playerSpawnPoint)
            {
                GameObject spawnGO = new GameObject("Player Spawn Point");
                playerSpawnPoint = spawnGO.transform;
                playerSpawnPoint.position = Vector3.zero;
            }
        }
        
        private void SetupEnemySpawns()
        {
            if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
            {
                enemySpawnPoints = new Transform[3];
                for (int i = 0; i < 3; i++)
                {
                    GameObject spawnGO = new GameObject($"Enemy Spawn Point {i + 1}");
                    enemySpawnPoints[i] = spawnGO.transform;
                    enemySpawnPoints[i].position = new Vector3((i - 1) * 5, 0, 8);
                }
            }
        }
        
        private void SetupUI()
        {
            // Create main canvas if it doesn't exist
            if (!mainCanvas)
            {
                GameObject canvasGO = new GameObject("Main Canvas");
                mainCanvas = canvasGO.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                mainCanvas.sortingOrder = 0;
                
                var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            // Setup ability bar
            if (!abilityBarUI)
            {
                GameObject abilityBarGO = new GameObject("Ability Bar UI");
                abilityBarUI = abilityBarGO.AddComponent<AbilityBarUI>();
                abilityBarGO.transform.SetParent(mainCanvas.transform, false);
            }
            
            // Setup player frame
            if (!playerFrameUI)
            {
                GameObject playerFrameGO = new GameObject("Player Frame UI");
                playerFrameUI = playerFrameGO.AddComponent<PlayerFrameUI>();
                playerFrameGO.transform.SetParent(mainCanvas.transform, false);
            }
            
            // Setup target frame
            if (!targetFrameUI)
            {
                GameObject targetFrameGO = new GameObject("Target Frame UI");
                targetFrameUI = targetFrameGO.AddComponent<TargetFrameUI>();
                targetFrameGO.transform.SetParent(mainCanvas.transform, false);
            }
        }
        
        private void CreateTrainingDummies()
        {
            if (!trainingDummyPrefab)
            {
                // Create a simple training dummy
                GameObject dummyGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                dummyGO.name = "Training Dummy";
                dummyGO.transform.position = new Vector3(0, 1, 5);
                dummyGO.transform.localScale = new Vector3(1, 2, 1);
                
                // Add required components
                var health = dummyGO.AddComponent<Health>();
                var targetable = dummyGO.AddComponent<Targetable>();
                var trainingDummy = dummyGO.AddComponent<TrainingDummy>();
                
                // Configure health
                health.SetMax(200f, true);
                health.SetFaction(Faction.Enemy);
                
                // Configure targetable
                targetable.name = "Training Dummy";
                
                trainingDummyPrefab = dummyGO;
            }
            
            // Spawn training dummies at enemy spawn points
            for (int i = 0; i < enemySpawnPoints.Length; i++)
            {
                if (enemySpawnPoints[i] != null)
                {
                    GameObject dummy = Instantiate(trainingDummyPrefab, enemySpawnPoints[i].position, enemySpawnPoints[i].rotation);
                    dummy.name = $"Training Dummy {i + 1}";
                }
            }
        }
        
        [ContextMenu("Spawn Player Class")]
        public void SpawnPlayerClass(int classIndex = 0)
        {
            if (availableClasses == null || availableClasses.Length == 0)
            {
                Debug.LogWarning("[CombatSceneSetup] No available classes configured!");
                return;
            }
            
            if (classIndex < 0 || classIndex >= availableClasses.Length)
            {
                Debug.LogWarning($"[CombatSceneSetup] Invalid class index: {classIndex}");
                return;
            }
            
            ClassTemplate selectedClass = availableClasses[classIndex];
            if (selectedClass == null)
            {
                Debug.LogWarning($"[CombatSceneSetup] Class at index {classIndex} is null!");
                return;
            }
            
            // Generate prefab from template
            GameObject playerPrefab = ClassPrefabGenerator.GenerateClassPrefab(selectedClass);
            if (playerPrefab == null)
            {
                Debug.LogError($"[CombatSceneSetup] Failed to generate prefab for {selectedClass.className}!");
                return;
            }
            
            // Spawn player
            GameObject player = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            player.name = $"{selectedClass.className} Player";
            
            // Configure player components
            var health = player.GetComponent<Health>();
            var targetable = player.GetComponent<Targetable>();
            var abilitySystem = player.GetComponent<AbilitySystem>();
            var targetingSystem = player.GetComponent<TargetingSystem>();
            
            if (health) health.SetFaction(Faction.Player);
            if (targetable) targetable.name = $"{selectedClass.className} Player";
            
            // Connect UI to player
            if (abilityBarUI) abilityBarUI.Bind(abilitySystem);
            if (playerFrameUI) playerFrameUI.playerHealth = health;
            
            // Set camera to follow player
            if (cameraController) cameraController.SetTarget(player.transform, player.GetComponent<PlayerMotor>());
            
            Debug.Log($"[CombatSceneSetup] Spawned {selectedClass.className} player!");
        }
        
        [ContextMenu("Spawn All Classes")]
        public void SpawnAllClasses()
        {
            if (availableClasses == null || availableClasses.Length == 0)
            {
                Debug.LogWarning("[CombatSceneSetup] No available classes configured!");
                return;
            }
            
            for (int i = 0; i < availableClasses.Length; i++)
            {
                SpawnPlayerClass(i);
            }
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(CombatSceneSetup))]
    public class CombatSceneSetupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scene Setup Actions", EditorStyles.boldLabel);
            
            CombatSceneSetup setup = (CombatSceneSetup)target;
            
            if (GUILayout.Button("Setup Combat Scene"))
            {
                setup.SetupScene();
            }
            
            if (setup.availableClasses != null && setup.availableClasses.Length > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Spawn Classes", EditorStyles.boldLabel);
                
                for (int i = 0; i < setup.availableClasses.Length; i++)
                {
                    if (setup.availableClasses[i] != null)
                    {
                        if (GUILayout.Button($"Spawn {setup.availableClasses[i].className}"))
                        {
                            setup.SpawnPlayerClass(i);
                        }
                    }
                }
                
                if (GUILayout.Button("Spawn All Classes"))
                {
                    setup.SpawnAllClasses();
                }
            }
        }
    }
    #endif
}
