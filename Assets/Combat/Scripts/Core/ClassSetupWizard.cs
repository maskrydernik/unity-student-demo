using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MiniWoW
{
    public class ClassSetupWizard : EditorWindow
    {
        private ClassTemplate currentTemplate;
        private Vector2 scrollPosition;
        private int selectedTab = 0;
        private readonly string[] tabNames = { "Basic Info", "Stats & Resources", "Abilities", "Summary" };
        
        [MenuItem("MiniWoW/Class Setup Wizard")]
        public static void ShowWindow()
        {
            GetWindow<ClassSetupWizard>("Class Setup Wizard");
        }
        
        private void OnEnable()
        {
            if (currentTemplate == null)
            {
                currentTemplate = CreateInstance<ClassTemplate>();
                InitializeDefaultTemplate();
            }
        }
        
        private void InitializeDefaultTemplate()
        {
            currentTemplate.className = "New Class";
            currentTemplate.shortConcept = "A powerful new class with unique abilities.";
            currentTemplate.classRole = ClassTemplate.ClassRole.Damage;
            currentTemplate.armorType = ClassTemplate.ArmorType.Cloth;
            currentTemplate.primaryResource = ClassTemplate.PrimaryResource.Mana;
            currentTemplate.uniqueMechanic = "This class has a special mechanic that makes it unique.";
            currentTemplate.classSummary = "This is a new class that brings something fresh to the game. It has a unique playstyle that focuses on...";
            
            // Initialize default stats
            currentTemplate.coreStats = new List<ClassStat>
            {
                new ClassStat { statName = "Strength", baseValue = 10f, perLevelGain = 2f },
                new ClassStat { statName = "Agility", baseValue = 10f, perLevelGain = 2f },
                new ClassStat { statName = "Intellect", baseValue = 10f, perLevelGain = 2f },
                new ClassStat { statName = "Spirit", baseValue = 10f, perLevelGain = 2f }
            };
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Class Setup Wizard", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            // Tab selection
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            GUILayout.Space(10);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            switch (selectedTab)
            {
                case 0: DrawBasicInfoTab(); break;
                case 1: DrawStatsTab(); break;
                case 2: DrawAbilitiesTab(); break;
                case 3: DrawSummaryTab(); break;
            }
            
            EditorGUILayout.EndScrollView();
            
            GUILayout.Space(20);
            DrawActionButtons();
        }
        
        private void DrawBasicInfoTab()
        {
            GUILayout.Label("Class Identity", EditorStyles.boldLabel);
            currentTemplate.className = EditorGUILayout.TextField("Class Name", currentTemplate.className);
            currentTemplate.shortConcept = EditorGUILayout.TextArea(currentTemplate.shortConcept, GUILayout.Height(60));
            currentTemplate.classRole = (ClassTemplate.ClassRole)EditorGUILayout.EnumPopup("Class Role", currentTemplate.classRole);
            
            GUILayout.Space(10);
            GUILayout.Label("Core Identity", EditorStyles.boldLabel);
            currentTemplate.armorType = (ClassTemplate.ArmorType)EditorGUILayout.EnumPopup("Armor Type", currentTemplate.armorType);
            
            GUILayout.Label("Weapon Types", EditorStyles.label);
            for (int i = 0; i < currentTemplate.weaponTypes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                currentTemplate.weaponTypes[i] = (ClassTemplate.WeaponType)EditorGUILayout.EnumPopup($"Weapon {i + 1}", currentTemplate.weaponTypes[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    currentTemplate.weaponTypes.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Weapon Type"))
            {
                currentTemplate.weaponTypes.Add(ClassTemplate.WeaponType.Sword);
            }
            
            currentTemplate.primaryResource = (ClassTemplate.PrimaryResource)EditorGUILayout.EnumPopup("Primary Resource", currentTemplate.primaryResource);
            
            GUILayout.Space(10);
            GUILayout.Label("Unique Mechanic", EditorStyles.boldLabel);
            currentTemplate.uniqueMechanic = EditorGUILayout.TextArea(currentTemplate.uniqueMechanic, GUILayout.Height(80));
        }
        
        private void DrawStatsTab()
        {
            GUILayout.Label("Core Stats", EditorStyles.boldLabel);
            for (int i = 0; i < currentTemplate.coreStats.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                currentTemplate.coreStats[i].statName = EditorGUILayout.TextField("Stat Name", currentTemplate.coreStats[i].statName);
                currentTemplate.coreStats[i].baseValue = EditorGUILayout.FloatField("Base", currentTemplate.coreStats[i].baseValue);
                currentTemplate.coreStats[i].perLevelGain = EditorGUILayout.FloatField("Per Level", currentTemplate.coreStats[i].perLevelGain);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    currentTemplate.coreStats.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Stat"))
            {
                currentTemplate.coreStats.Add(new ClassStat { statName = "New Stat", baseValue = 10f, perLevelGain = 2f });
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Health & Resources", EditorStyles.boldLabel);
            currentTemplate.baseHealth = EditorGUILayout.FloatField("Base Health", currentTemplate.baseHealth);
            currentTemplate.healthPerLevel = EditorGUILayout.FloatField("Health Per Level", currentTemplate.healthPerLevel);
            currentTemplate.baseResource = EditorGUILayout.FloatField("Base Resource", currentTemplate.baseResource);
            currentTemplate.resourcePerLevel = EditorGUILayout.FloatField("Resource Per Level", currentTemplate.resourcePerLevel);
            currentTemplate.movementSpeed = EditorGUILayout.FloatField("Movement Speed", currentTemplate.movementSpeed);
        }
        
        private void DrawAbilitiesTab()
        {
            GUILayout.Label("Starting Abilities", EditorStyles.boldLabel);
            for (int i = 0; i < currentTemplate.startingAbilities.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");
                currentTemplate.startingAbilities[i].abilityName = EditorGUILayout.TextField("Ability Name", currentTemplate.startingAbilities[i].abilityName);
                currentTemplate.startingAbilities[i].abilityDefinition = (AbilityDefinition)EditorGUILayout.ObjectField("Ability Definition", currentTemplate.startingAbilities[i].abilityDefinition, typeof(AbilityDefinition), false);
                currentTemplate.startingAbilities[i].slotIndex = EditorGUILayout.IntSlider("Slot Index", currentTemplate.startingAbilities[i].slotIndex, 0, 4);
                
                if (GUILayout.Button("Remove Ability"))
                {
                    currentTemplate.startingAbilities.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("Add Starting Ability"))
            {
                currentTemplate.startingAbilities.Add(new StartingAbility { abilityName = "New Ability", slotIndex = 0 });
            }
        }
        
        private void DrawSummaryTab()
        {
            GUILayout.Label("Class Summary", EditorStyles.boldLabel);
            currentTemplate.classSummary = EditorGUILayout.TextArea(currentTemplate.classSummary, GUILayout.Height(120));
            
            GUILayout.Space(10);
            GUILayout.Label("Visual & Audio", EditorStyles.boldLabel);
            currentTemplate.characterPrefab = (GameObject)EditorGUILayout.ObjectField("Character Prefab", currentTemplate.characterPrefab, typeof(GameObject), false);
            currentTemplate.classIcon = (Sprite)EditorGUILayout.ObjectField("Class Icon", currentTemplate.classIcon, typeof(Sprite), false);
            currentTemplate.classColor = EditorGUILayout.ColorField("Class Color", currentTemplate.classColor);
            currentTemplate.classSound = (AudioClip)EditorGUILayout.ObjectField("Class Sound", currentTemplate.classSound, typeof(AudioClip), false);
            
            GUILayout.Space(10);
            GUILayout.Label("Preview", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Class Name:", currentTemplate.className);
            EditorGUILayout.LabelField("Role:", currentTemplate.classRole.ToString());
            EditorGUILayout.LabelField("Armor Type:", currentTemplate.armorType.ToString());
            EditorGUILayout.LabelField("Primary Resource:", currentTemplate.primaryResource.ToString());
            EditorGUILayout.LabelField("Base Health:", currentTemplate.baseHealth.ToString());
            EditorGUILayout.LabelField("Starting Abilities:", currentTemplate.startingAbilities.Count.ToString());
        }
        
        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Load Template"))
            {
                string path = EditorUtility.OpenFilePanel("Load Class Template", "Assets", "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                    currentTemplate = AssetDatabase.LoadAssetAtPath<ClassTemplate>(path);
                }
            }
            
            if (GUILayout.Button("Save Template"))
            {
                string path = EditorUtility.SaveFilePanelInProject("Save Class Template", currentTemplate.className, "asset", "Save Class Template");
                if (!string.IsNullOrEmpty(path))
                {
                    // Ensure we're saving a distinct asset instance
                    var asset = CreateInstance<ClassTemplate>();
                    EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(currentTemplate), asset);
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = asset;
                    // Reset currentTemplate to a fresh instance after save
                    currentTemplate = CreateInstance<ClassTemplate>();
                    InitializeDefaultTemplate();
                }
            }
            
            if (GUILayout.Button("Create Prefab"))
            {
                CreateClassPrefab();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void CreateClassPrefab()
        {
            if (currentTemplate == null)
            {
                EditorUtility.DisplayDialog("Error", "No template loaded!", "OK");
                return;
            }
            
            // Create a new GameObject for the class
            GameObject classObject = new GameObject(currentTemplate.className + " Prefab");
            
            // Add required components
            var health = classObject.AddComponent<Health>();
            var targetable = classObject.AddComponent<Targetable>();
            var abilitySystem = classObject.AddComponent<AbilitySystem>();
            var targetingSystem = classObject.AddComponent<TargetingSystem>();
            var playerMotor = classObject.AddComponent<PlayerMotor>();
            
            // Configure health
            health.SetMax(currentTemplate.baseHealth, true);
            health.SetFaction(Faction.Player);
            
            // Configure targetable
            targetable.name = currentTemplate.className;
            
            // Configure ability system
            abilitySystem.loadout = new AbilityDefinition[5];
            foreach (var startingAbility in currentTemplate.startingAbilities)
            {
                if (startingAbility.abilityDefinition != null && startingAbility.slotIndex >= 0 && startingAbility.slotIndex < 5)
                {
                    abilitySystem.loadout[startingAbility.slotIndex] = startingAbility.abilityDefinition;
                }
            }
            
            // Create projectile spawn point
            GameObject projectileSpawn = new GameObject("ProjectileSpawn");
            projectileSpawn.transform.SetParent(classObject.transform);
            projectileSpawn.transform.localPosition = new Vector3(0, 1.2f, 0.5f);
            abilitySystem.projectileSpawn = projectileSpawn.transform;
            
            // Add a simple capsule for visualization
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.SetParent(classObject.transform);
            capsule.transform.localPosition = Vector3.zero;
            capsule.name = "Visual";
            
            // Create the prefab
            string prefabPath = $"Assets/Combat/GeneratedContent/Prefabs/{currentTemplate.className}Prefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(classObject, prefabPath);
            
            // Clean up the temporary object
            DestroyImmediate(classObject);
            
            EditorUtility.DisplayDialog("Success", $"Created prefab: {prefabPath}", "OK");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = prefab;
        }
    }
}
