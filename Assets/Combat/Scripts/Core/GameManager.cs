using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniWoW
{
    public class GameManager : MonoBehaviour
    {
        [Header("Player Prefabs (choose at runtime)")]
        public GameObject[] playerPrefabs; // optional; SetupWizard can fill. If empty, will create default ones.

        [Header("Training Dummies")]
        public Vector3 enemyDummyPos = new Vector3(6f, 0f, 10f);
        public Vector3 friendlyDummyPos = new Vector3(-6f, 0f, 10f);

        private GameObject activePlayer;

        private void Awake()
        {
            EnsureEventSystem();
            EnsureGround();
            SpawnDummies();
            BuildPlayerSelectionUI();
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>()) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private void EnsureGround()
        {
            if (GameObject.Find("Ground")) return;
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = Vector3.one * 2f;
        }

        private void SpawnDummies()
        {
            SpawnDummy("EnemyDummy", Faction.Enemy, enemyDummyPos);
            SpawnDummy("FriendlyDummy", Faction.Friendly, friendlyDummyPos);
        }

        private void SpawnDummy(string name, Faction f, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.position = pos;
            var h = go.AddComponent<Health>();
            h.SetFaction(f);
            h.SetMax(2000f, true);
            var t = go.AddComponent<Targetable>();
            var field = typeof(Targetable).GetField("displayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(t, f == Faction.Enemy ? "Training Dummy" : "Friendly Dummy");
            go.AddComponent<TrainingDummy>();
        }

        private void BuildPlayerSelectionUI()
        {
            // Canvas
            var go = new GameObject("SelectionCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var panel = new GameObject("Panel", typeof(Image));
            panel.transform.SetParent(go.transform, false);
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.5f);
            prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(600f, 200f);
            prt.anchoredPosition = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            var title = new GameObject("Title", typeof(Text));
            title.transform.SetParent(panel.transform, false);
            var trt = title.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 1f); trt.anchorMax = new Vector2(0.5f, 1f);
            trt.pivot = new Vector2(0.5f, 1f); trt.anchoredPosition = new Vector2(0f, -10f); trt.sizeDelta = new Vector2(500f, 30f);
            var t = title.GetComponent<Text>();
            t.alignment = TextAnchor.MiddleCenter; t.fontSize = 20; t.text = "Choose Player"; t.color = Color.white;

            // Buttons
            int count = (playerPrefabs != null && playerPrefabs.Length > 0) ? playerPrefabs.Length : 3;
            for (int i = 0; i < count; i++)
            {
                var btnGO = new GameObject($"Option{i+1}", typeof(Image), typeof(Button));
                btnGO.transform.SetParent(panel.transform, false);
                var brt = btnGO.GetComponent<RectTransform>();
                brt.sizeDelta = new Vector2(160f, 48f);
                brt.anchoredPosition = new Vector2((i - (count - 1) / 2f) * 180f, -80f);

                var lblGO = new GameObject("Label", typeof(Text));
                lblGO.transform.SetParent(btnGO.transform, false);
                var lrt = lblGO.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
                var txt = lblGO.GetComponent<Text>();
                txt.alignment = TextAnchor.MiddleCenter; txt.fontSize = 16; txt.color = Color.white;
                txt.text = playerPrefabs != null && playerPrefabs.Length > i && playerPrefabs[i] ? playerPrefabs[i].name : $"Prototype {i+1}";

                int idx = i;
                btnGO.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Destroy(go);
                    SpawnPlayer(idx);
                });
            }
        }

        private void SpawnPlayer(int index)
        {
            GameObject prefab = null;
            if (playerPrefabs != null && playerPrefabs.Length > index) prefab = playerPrefabs[index];

            if (!prefab)
            {
                prefab = CreateDefaultPlayerPrototype(index);
            }

            activePlayer = Instantiate(prefab, new Vector3(0f, 1f, 0f), Quaternion.identity);
            activePlayer.name = "Player";

            // Camera
            var camGO = new GameObject("MainCamera");
            var camera = camGO.AddComponent<Camera>();
            camera.tag = "MainCamera";
            var camCtrl = camGO.AddComponent<CameraController>();

            // Player motor
            var motor = activePlayer.GetComponent<PlayerMotor>();
            if (!motor) motor = activePlayer.AddComponent<PlayerMotor>();
            motor.cameraController = camCtrl;
            motor.moveSpeed = 6f;

            // Targeting
            var tgt = activePlayer.GetComponent<TargetingSystem>();
            if (!tgt) tgt = activePlayer.AddComponent<TargetingSystem>();

            // Health + targetable
            var h = activePlayer.GetComponent<Health>();
            if (!h) h = activePlayer.AddComponent<Health>();
            h.SetFaction(Faction.Player);
            h.SetMax(500f, true);

            var tar = activePlayer.GetComponent<Targetable>();
            if (!tar) tar = activePlayer.AddComponent<Targetable>();
            var field = typeof(Targetable).GetField("displayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(tar, "Player");

            // Ability System + UI
            var abs = activePlayer.GetComponent<AbilitySystem>();
            if (!abs) abs = activePlayer.AddComponent<AbilitySystem>();
            abs.targeting = tgt;

            // Attach UI helpers
            var bar = new GameObject("AbilityBarUI").AddComponent<AbilityBarUI>();
            bar.abilitySystem = abs;

            var tf = new GameObject("TargetFrameUI").AddComponent<TargetFrameUI>();
            tf.targeting = tgt;

            var pf = new GameObject("PlayerFrameUI").AddComponent<PlayerFrameUI>();
            pf.playerHealth = h;

            // Basic loadout fallback if prefab had none
            EnsureFallbackAbilities(abs);
        }

        private GameObject CreateDefaultPlayerPrototype(int index)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "PrototypePlayer";
            var cc = go.AddComponent<CharacterController>();
            cc.height = 2f; cc.center = new Vector3(0f, 1f, 0f);
            return go;
        }

        private void EnsureFallbackAbilities(AbilitySystem abs)
        {
            bool empty = true;
            for (int i = 0; i < abs.loadout.Length; i++)
            {
                if (abs.loadout[i]) { empty = false; break; }
            }
            if (!empty) return;

            // Create runtime ability defs as ScriptableObjects in memory
            abs.loadout[0] = MakeSimpleAbility("Fireball", TargetRule.Enemy, dmg: 40f, heal: 0f, projectile: true, travelTime: 0.6f);
            abs.loadout[1] = MakeSimpleAbility("Smite", TargetRule.Enemy, dmg: 25f, heal: 0f, projectile: false, travelTime: 0.0f);
            abs.loadout[2] = MakeSimpleAbility("Heal", TargetRule.Friendly, dmg: 0f, heal: 35f, projectile: false, travelTime: 0.0f);
            abs.loadout[3] = MakeSimpleAbility("Bolt", TargetRule.Enemy, dmg: 20f, heal: 0f, projectile: true, travelTime: 0.4f);
            abs.loadout[4] = MakeSimpleAbility("Big Heal", TargetRule.Friendly, dmg: 0f, heal: 60f, projectile: false, travelTime: 0.0f);

            // Provide projectile prefab
            var projPrefab = new GameObject("Projectile");
            var projSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projSphere.name = "Visual";
            projSphere.transform.SetParent(projPrefab.transform);
            projSphere.transform.localScale = Vector3.one * 0.3f;
            Destroy(projSphere.GetComponent<Collider>()); // Remove primitive collider
            var projRenderer = projSphere.GetComponent<Renderer>();
            if (projRenderer) projRenderer.material.color = Color.cyan; // Bright visible color
            
            projPrefab.AddComponent<SphereCollider>().isTrigger = true;
            var rb = projPrefab.AddComponent<Rigidbody>(); rb.useGravity = false; rb.isKinematic = true;
            var proj = projPrefab.AddComponent<Projectile>();
            // assign to projectile abilities
            abs.loadout[0].useProjectile = true; abs.loadout[0].projectilePrefab = projPrefab;
            abs.loadout[3].useProjectile = true; abs.loadout[3].projectilePrefab = projPrefab;
        }

        private AbilityDefinition MakeSimpleAbility(string name, TargetRule rule, float dmg, float heal, bool projectile, float travelTime)
        {
            var def = ScriptableObject.CreateInstance<AbilityDefinition>();
            def.id = name.ToLower().Replace(" ", ".");
            def.displayName = name;
            def.targetRule = rule;
            def.damage = dmg;
            def.healing = heal;
            def.useProjectile = projectile;
            def.projectileTravelTime = travelTime;
            def.cooldown = 2f;
            def.requiresTarget = rule != TargetRule.Self;
            def.range = 25f;
            return def;
        }
    }
}
