#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
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

            // Create sample scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var gmGO = new GameObject("GameManager");
            var gm = gmGO.AddComponent<MiniWoW.GameManager>();
            gm.playerPrefabs = new GameObject[] { playerPrefab, magePrefab, priestPrefab };

            EditorSceneManager.SaveScene(scene, "Assets/Combat/GeneratedContent/Scenes/CombatSample.unity");
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Combat System", "Sample content generated in Combat folder:\n- Prefabs\n- Abilities\n- Sample scene at Assets/Combat/GeneratedContent/Scenes/CombatSample.unity", "OK");
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
    }
}
#endif
