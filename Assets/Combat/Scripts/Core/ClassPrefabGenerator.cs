using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MiniWoW
{
    public static class ClassPrefabGenerator
    {
        public static GameObject GenerateClassPrefab(ClassTemplate template)
        {
            if (template == null)
            {
                Debug.LogError("[ClassPrefabGenerator] Template is null!");
                return null;
            }
            
            // Create root GameObject
            GameObject classObject = new GameObject(template.className + " Prefab");
            
            // Add core components
            var health = classObject.AddComponent<Health>();
            var targetable = classObject.AddComponent<Targetable>();
            var abilitySystem = classObject.AddComponent<AbilitySystem>();
            var targetingSystem = classObject.AddComponent<TargetingSystem>();
            var playerMotor = classObject.AddComponent<PlayerMotor>();
            
            // Configure Health component
            health.SetMax(template.GetHealthAtLevel(1), true);
            health.SetFaction(Faction.Player);
            
            // Configure Targetable component
            targetable.name = template.className;
            
            // Configure AbilitySystem
            abilitySystem.loadout = new AbilityDefinition[5];
            foreach (var startingAbility in template.startingAbilities)
            {
                if (startingAbility.abilityDefinition != null && 
                    startingAbility.slotIndex >= 0 && 
                    startingAbility.slotIndex < 5)
                {
                    abilitySystem.loadout[startingAbility.slotIndex] = startingAbility.abilityDefinition;
                }
            }
            
            // Create projectile spawn point
            GameObject projectileSpawn = new GameObject("ProjectileSpawn");
            projectileSpawn.transform.SetParent(classObject.transform);
            projectileSpawn.transform.localPosition = new Vector3(0, 1.2f, 0.5f);
            abilitySystem.projectileSpawn = projectileSpawn.transform;
            
            // Add visual representation
            CreateVisualRepresentation(classObject, template);
            
            // Add class-specific components
            AddClassSpecificComponents(classObject, template);
            
            return classObject;
        }
        
        private static void CreateVisualRepresentation(GameObject parent, ClassTemplate template)
        {
            // Create main visual
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.transform.SetParent(parent.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.name = "Visual";
            
            // Apply class color
            var renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = template.classColor;
                renderer.material = mat;
            }
            
            // Add class-specific visual elements based on role
            switch (template.classRole)
            {
                case ClassTemplate.ClassRole.Tank:
                    AddTankVisuals(visual, template);
                    break;
                case ClassTemplate.ClassRole.Healer:
                    AddHealerVisuals(visual, template);
                    break;
                case ClassTemplate.ClassRole.Damage:
                    AddDamageVisuals(visual, template);
                    break;
                case ClassTemplate.ClassRole.Hybrid:
                    AddHybridVisuals(visual, template);
                    break;
            }
        }
        
        private static void AddTankVisuals(GameObject visual, ClassTemplate template)
        {
            // Add shield visual
            GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shield.transform.SetParent(visual.transform);
            shield.transform.localPosition = new Vector3(0.8f, 0, 0);
            shield.transform.localScale = new Vector3(0.1f, 1.2f, 0.6f);
            shield.name = "Shield";
            
            // Make shield slightly transparent
            var shieldRenderer = shield.GetComponent<Renderer>();
            if (shieldRenderer != null)
            {
                Material shieldMat = new Material(Shader.Find("Standard"));
                shieldMat.color = new Color(0.7f, 0.7f, 0.9f, 0.8f);
                shieldMat.SetFloat("_Mode", 3); // Transparent mode
                shieldMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                shieldMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                shieldMat.SetInt("_ZWrite", 0);
                shieldMat.DisableKeyword("_ALPHATEST_ON");
                shieldMat.EnableKeyword("_ALPHABLEND_ON");
                shieldMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                shieldRenderer.material = shieldMat;
            }
        }
        
        private static void AddHealerVisuals(GameObject visual, ClassTemplate template)
        {
            // Add staff visual
            GameObject staff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            staff.transform.SetParent(visual.transform);
            staff.transform.localPosition = new Vector3(0, 0, 0.8f);
            staff.transform.localScale = new Vector3(0.1f, 0.8f, 0.1f);
            staff.transform.localRotation = Quaternion.Euler(90, 0, 0);
            staff.name = "Staff";
            
            // Add glowing orb at top
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.transform.SetParent(staff.transform);
            orb.transform.localPosition = new Vector3(0, 0.4f, 0);
            orb.transform.localScale = Vector3.one * 0.3f;
            orb.name = "Orb";
            
            // Make orb glow
            var orbRenderer = orb.GetComponent<Renderer>();
            if (orbRenderer != null)
            {
                Material orbMat = new Material(Shader.Find("Standard"));
                orbMat.color = new Color(0.2f, 0.8f, 1f, 0.9f);
                orbMat.EnableKeyword("_EMISSION");
                orbMat.SetColor("_EmissionColor", new Color(0.1f, 0.4f, 0.5f));
                orbRenderer.material = orbMat;
            }
        }
        
        private static void AddDamageVisuals(GameObject visual, ClassTemplate template)
        {
            // Add weapon based on weapon types
            if (template.weaponTypes.Contains(ClassTemplate.WeaponType.Sword))
            {
                AddSwordVisual(visual);
            }
            else if (template.weaponTypes.Contains(ClassTemplate.WeaponType.Bow))
            {
                AddBowVisual(visual);
            }
            else if (template.weaponTypes.Contains(ClassTemplate.WeaponType.Staff))
            {
                AddStaffVisual(visual);
            }
        }
        
        private static void AddHybridVisuals(GameObject visual, ClassTemplate template)
        {
            // Hybrid gets both weapon and utility elements
            AddSwordVisual(visual);
            AddUtilityVisual(visual);
        }
        
        private static void AddSwordVisual(GameObject visual)
        {
            GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sword.transform.SetParent(visual.transform);
            sword.transform.localPosition = new Vector3(0, 0, 0.8f);
            sword.transform.localScale = new Vector3(0.1f, 0.1f, 0.8f);
            sword.name = "Sword";
        }
        
        private static void AddBowVisual(GameObject visual)
        {
            GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bow.transform.SetParent(visual.transform);
            bow.transform.localPosition = new Vector3(0, 0, 0.8f);
            bow.transform.localScale = new Vector3(0.1f, 0.1f, 0.6f);
            bow.transform.localRotation = Quaternion.Euler(0, 0, 90);
            bow.name = "Bow";
        }
        
        private static void AddStaffVisual(GameObject visual)
        {
            GameObject staff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            staff.transform.SetParent(visual.transform);
            staff.transform.localPosition = new Vector3(0, 0, 0.8f);
            staff.transform.localScale = new Vector3(0.1f, 0.1f, 0.8f);
            staff.name = "Staff";
        }
        
        private static void AddUtilityVisual(GameObject visual)
        {
            GameObject utility = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            utility.transform.SetParent(visual.transform);
            utility.transform.localPosition = new Vector3(0, 1.2f, 0);
            utility.transform.localScale = Vector3.one * 0.2f;
            utility.name = "Utility";
        }
        
        private static void AddClassSpecificComponents(GameObject classObject, ClassTemplate template)
        {
            // Add class-specific behavior components based on role
            switch (template.classRole)
            {
                case ClassTemplate.ClassRole.Tank:
                    AddTankComponents(classObject, template);
                    break;
                case ClassTemplate.ClassRole.Healer:
                    AddHealerComponents(classObject, template);
                    break;
                case ClassTemplate.ClassRole.Damage:
                    AddDamageComponents(classObject, template);
                    break;
                case ClassTemplate.ClassRole.Hybrid:
                    AddHybridComponents(classObject, template);
                    break;
            }
        }
        
        private static void AddTankComponents(GameObject classObject, ClassTemplate template)
        {
            // Tank-specific components could be added here
            // For example: Damage reduction, threat generation, etc.
        }
        
        private static void AddHealerComponents(GameObject classObject, ClassTemplate template)
        {
            // Healer-specific components could be added here
            // For example: Healing bonuses, mana efficiency, etc.
        }
        
        private static void AddDamageComponents(GameObject classObject, ClassTemplate template)
        {
            // Damage-specific components could be added here
            // For example: Damage bonuses, critical hit chance, etc.
        }
        
        private static void AddHybridComponents(GameObject classObject, ClassTemplate template)
        {
            // Hybrid-specific components could be added here
            // For example: Versatility bonuses, etc.
        }
        
        [MenuItem("MiniWoW/Generate All Class Prefabs")]
        public static void GenerateAllClassPrefabs()
        {
            // Find all class templates in the project
            string[] guids = AssetDatabase.FindAssets("t:ClassTemplate");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ClassTemplate template = AssetDatabase.LoadAssetAtPath<ClassTemplate>(path);
                
                if (template != null)
                {
                    GameObject prefab = GenerateClassPrefab(template);
                    if (prefab != null)
                    {
                        string prefabPath = $"Assets/Combat/GeneratedContent/Prefabs/{template.className}Prefab.prefab";
                        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
                        DestroyImmediate(prefab);
                        Debug.Log($"Generated prefab for {template.className}: {prefabPath}");
                    }
                }
            }
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "All class prefabs generated!", "OK");
        }
    }
}
