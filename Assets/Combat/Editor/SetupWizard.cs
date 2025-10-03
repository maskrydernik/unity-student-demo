#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

namespace MiniWoW.EditorTools
{
    public static class SetupWizard
    {
        [MenuItem("Tools/Combat/Generate Sample Content")]
        public static void Generate()
        {
            // Create folders if missing (all within Combat folder)
            EnsureFolder("Assets/Combat/GeneratedContent");
            EnsureFolder("Assets/Combat/GeneratedContent/Prefabs");
            EnsureFolder("Assets/Combat/GeneratedContent/Abilities");
            EnsureFolder("Assets/Combat/GeneratedContent/Scenes");

            // Projectile prefab
            GameObject proj = new GameObject("Projectile");
            var projSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projSphere.name = "Visual";
            projSphere.transform.SetParent(proj.transform);
            projSphere.transform.localScale = Vector3.one * 0.3f;
            Object.DestroyImmediate(projSphere.GetComponent<Collider>()); // Remove primitive collider
            var projRenderer = projSphere.GetComponent<Renderer>();
            if (projRenderer) projRenderer.sharedMaterial.color = Color.cyan; // Bright visible color
            
            proj.AddComponent<SphereCollider>().isTrigger = true;
            var rb = proj.AddComponent<Rigidbody>(); rb.useGravity = false; rb.isKinematic = true;
            proj.AddComponent<MiniWoW.Projectile>();
            string projPath = "Assets/Combat/GeneratedContent/Prefabs/Projectile.prefab";
            var projPrefab = PrefabUtility.SaveAsPrefabAsset(proj, projPath);
            GameObject.DestroyImmediate(proj);

            // Ability assets
            var fireball = CreateAbility("Fireball", TargetRule.Enemy, dmg: 40f, heal: 0f, cd: 2f, useProj: true, projPrefab);
            var smite    = CreateAbility("Smite",    TargetRule.Enemy, dmg: 25f, heal: 0f, cd: 2f, useProj: false, projPrefab);
            var heal     = CreateAbility("Heal",     TargetRule.Friendly, dmg: 0f, heal: 35f, cd: 2f, useProj: false, projPrefab);
            var bolt     = CreateAbility("Bolt",     TargetRule.Enemy, dmg: 20f, heal: 0f, cd: 2f, useProj: true, projPrefab);
            var bigHeal  = CreateAbility("Big Heal", TargetRule.Friendly, dmg: 0f, heal: 60f, cd: 4f, useProj: false, projPrefab);

            // Player prefab
            var playerPrefab = CreatePlayerPrefab("PlayerPrefab", new[] { fireball, smite, heal, bolt, bigHeal });

            // Optional other player variants
            var magePrefab = CreatePlayerPrefab("MagePrefab", new[] { fireball, heal, bolt, smite, bigHeal });
            var priestPrefab = CreatePlayerPrefab("PriestPrefab", new[] { heal, bigHeal, smite, fireball, bolt });

            // Create complete sample scene
            CreateCompleteScene(new GameObject[] { playerPrefab, magePrefab, priestPrefab });

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Combat System", "Sample content generated in Combat folder:\n- Prefabs\n- Abilities\n- Complete scene at Assets/Combat/GeneratedContent/Scenes/CombatSample.unity\n\nOpen the scene and press Play!", "OK");
        }

        private static AbilityDefinition CreateAbility(string name, TargetRule rule, float dmg, float heal, float cd, bool useProj, GameObject projPrefab)
        {
            var def = ScriptableObject.CreateInstance<AbilityDefinition>();
            def.id = name.ToLower().Replace(" ", ".");
            def.displayName = name;
            def.targetRule = rule;
            def.damage = dmg;
            def.healing = heal;
            def.cooldown = cd;
            def.useProjectile = useProj;
            def.projectileTravelTime = 0.6f;
            def.requiresTarget = rule != TargetRule.Self;
            def.range = 25f;
            if (useProj) def.projectilePrefab = projPrefab;

            string path = $"Assets/Combat/GeneratedContent/Abilities/{name}.asset";
            AssetDatabase.CreateAsset(def, path);
            return AssetDatabase.LoadAssetAtPath<AbilityDefinition>(path);
        }

        private static GameObject CreatePlayerPrefab(string name, AbilityDefinition[] loadout)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            var cc = go.AddComponent<CharacterController>(); cc.height = 2f; cc.center = new Vector3(0f, 1f, 0f);

            var health = go.AddComponent<MiniWoW.Health>(); health.SetFaction(Faction.Player); health.SetMax(500f, true);
            var targ = go.AddComponent<MiniWoW.Targetable>();
            var field = typeof(MiniWoW.Targetable).GetField("displayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(targ, name);

            go.AddComponent<MiniWoW.TargetingSystem>();
            var abs = go.AddComponent<MiniWoW.AbilitySystem>();
            abs.loadout = loadout;
            go.AddComponent<MiniWoW.PlayerMotor>();

            string path = $"Assets/Combat/GeneratedContent/Prefabs/{name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            GameObject.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = Path.GetDirectoryName(path).Replace("\\", "/");
                var leaf = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }

        private static void CreateCompleteScene(GameObject[] playerPrefabs)
        {
            // Create new scene with default objects (Main Camera, Directional Light)
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Keep default camera but position it to see the selection UI
            var defaultCam = GameObject.Find("Main Camera");
            if (defaultCam)
            {
                defaultCam.transform.position = new Vector3(0f, 5f, -10f);
                defaultCam.transform.LookAt(Vector3.zero);
                // This camera will be destroyed when player spawns
            }

            // 1. Create EventSystem
            var eventSys = new GameObject("EventSystem");
            eventSys.AddComponent<EventSystem>();
            eventSys.AddComponent<StandaloneInputModule>();

            // 2. Create Ground
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = Vector3.one * 2f;

            // 3. Create Training Dummies
            CreateTrainingDummy("EnemyDummy", Faction.Enemy, new Vector3(6f, 0f, 10f), Color.red);
            CreateTrainingDummy("FriendlyDummy", Faction.Friendly, new Vector3(-6f, 0f, 10f), Color.green);

            // 4. Create UI Canvases (these will persist and find the player when it spawns)
            CreatePersistentUICanvases();

            // 5. Create GameManager with simple player spawner
            var gmGO = new GameObject("GameManager");
            var spawner = gmGO.AddComponent<SimplePlayerSpawner>();
            spawner.playerPrefabs = playerPrefabs;

            // Save scene
            EditorSceneManager.SaveScene(scene, "Assets/Combat/GeneratedContent/Scenes/CombatSample.unity");
        }

        private static void CreateTrainingDummy(string name, Faction faction, Vector3 position, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.position = position;
            
            // Color the dummy
            var renderer = go.GetComponent<Renderer>();
            if (renderer)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                renderer.sharedMaterial = mat;
            }

            var health = go.AddComponent<Health>();
            health.SetFaction(faction);
            health.SetMax(2000f, true);

            var targetable = go.AddComponent<Targetable>();
            
            // Use SerializedObject to properly set the private fields in Edit Mode
            var so = new UnityEditor.SerializedObject(targetable);
            so.FindProperty("displayName").stringValue = faction == Faction.Enemy ? "Training Dummy" : "Friendly Dummy";
            so.FindProperty("health").objectReferenceValue = health;
            so.ApplyModifiedProperties();

            go.AddComponent<TrainingDummy>();
            
            Debug.Log($"[SetupWizard] Created {name} with Health (Faction: {faction}) and Targetable components");
        }

        private static void CreatePersistentUICanvases()
        {
            // Main UI Canvas
            var canvasGO = new GameObject("UI_Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasGO.AddComponent<GraphicRaycaster>();

            // Player Frame (Top-Left)
            var pfGO = new GameObject("PlayerFrame_UI");
            pfGO.transform.SetParent(canvasGO.transform, false);
            var pf = pfGO.AddComponent<PlayerFrameUI>();
            pf.canvas = canvas;

            // Target Frame (Top-Center)
            var tfGO = new GameObject("TargetFrame_UI");
            tfGO.transform.SetParent(canvasGO.transform, false);
            var tf = tfGO.AddComponent<TargetFrameUI>();
            tf.canvas = canvas;

            // Ability Bar (Bottom-Center)
            var abGO = new GameObject("AbilityBar_UI");
            abGO.transform.SetParent(canvasGO.transform, false);
            var ab = abGO.AddComponent<AbilityBarUI>();
            ab.canvas = canvas;
        }
    }
}
#endif
