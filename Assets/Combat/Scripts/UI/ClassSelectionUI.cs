using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MiniWoW
{
    public class ClassSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        public Transform classButtonContainer;
        public GameObject classButtonPrefab;
        public Text classNameText;
        public Text classDescriptionText;
        public Text classStatsText;
        public Image classIconImage;
        public Button spawnButton;
        public Button closeButton;
        
        [Header("Class Templates")]
        public ClassTemplate[] availableClasses;
        
        private ClassTemplate selectedClass;
        private CombatSceneSetup sceneSetup;
        
        private void Start()
        {
            sceneSetup = FindFirstObjectByType<CombatSceneSetup>();
            if (sceneSetup == null)
            {
                Debug.LogError("[ClassSelectionUI] No CombatSceneSetup found in scene!");
                return;
            }
            
            SetupUI();
            CreateClassButtons();
        }
        
        private void SetupUI()
        {
            if (spawnButton) spawnButton.onClick.AddListener(SpawnSelectedClass);
            if (closeButton) closeButton.onClick.AddListener(CloseClassSelection);
            
            // Hide spawn button initially
            if (spawnButton) spawnButton.gameObject.SetActive(false);
        }
        
        private void CreateClassButtons()
        {
            if (classButtonContainer == null || classButtonPrefab == null)
            {
                Debug.LogWarning("[ClassSelectionUI] Class button container or prefab not assigned!");
                return;
            }
            
            // Clear existing buttons
            foreach (Transform child in classButtonContainer)
            {
                DestroyImmediate(child.gameObject);
            }
            
            // Create buttons for each class
            for (int i = 0; i < availableClasses.Length; i++)
            {
                if (availableClasses[i] == null) continue;
                
                GameObject buttonGO = Instantiate(classButtonPrefab, classButtonContainer);
                ClassButton classButton = buttonGO.GetComponent<ClassButton>();
                if (classButton == null)
                {
                    classButton = buttonGO.AddComponent<ClassButton>();
                }
                
                classButton.Setup(availableClasses[i], this);
            }
        }
        
        public void SelectClass(ClassTemplate classTemplate)
        {
            selectedClass = classTemplate;
            UpdateClassInfo();
            
            if (spawnButton) spawnButton.gameObject.SetActive(true);
        }
        
        private void UpdateClassInfo()
        {
            if (selectedClass == null) return;
            
            if (classNameText) classNameText.text = selectedClass.className;
            
            if (classDescriptionText)
            {
                classDescriptionText.text = $"{selectedClass.shortConcept}\n\n" +
                                          $"Role: {selectedClass.classRole}\n" +
                                          $"Armor: {selectedClass.armorType}\n" +
                                          $"Resource: {selectedClass.primaryResource}";
            }
            
            if (classStatsText)
            {
                string statsText = "Core Stats:\n";
                foreach (var stat in selectedClass.coreStats)
                {
                    statsText += $"{stat.statName}: {stat.baseValue} (+{stat.perLevelGain}/level)\n";
                }
                statsText += $"\nBase Health: {selectedClass.baseHealth}\n";
                statsText += $"Base Resource: {selectedClass.baseResource}\n";
                statsText += $"Movement Speed: {selectedClass.movementSpeed}";
                
                classStatsText.text = statsText;
            }
            
            if (classIconImage)
            {
                if (selectedClass.classIcon)
                {
                    classIconImage.sprite = selectedClass.classIcon;
                    classIconImage.color = Color.white;
                }
                else
                {
                    classIconImage.sprite = null;
                    classIconImage.color = selectedClass.classColor;
                }
            }
        }
        
        private void SpawnSelectedClass()
        {
            if (selectedClass == null || sceneSetup == null) return;
            
            // Find the index of the selected class
            int classIndex = -1;
            for (int i = 0; i < availableClasses.Length; i++)
            {
                if (availableClasses[i] == selectedClass)
                {
                    classIndex = i;
                    break;
                }
            }
            
            if (classIndex >= 0)
            {
                sceneSetup.SpawnPlayerClass(classIndex);
                CloseClassSelection();
            }
        }
        
        private void CloseClassSelection()
        {
            gameObject.SetActive(false);
        }
        
        public void ShowClassSelection()
        {
            gameObject.SetActive(true);
        }
    }
    
    public class ClassButton : MonoBehaviour
    {
        [Header("UI Components")]
        public Image classIcon;
        public Text className;
        public Text classRole;
        public Button button;
        
        private ClassTemplate classTemplate;
        private ClassSelectionUI selectionUI;
        
        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (button) button.onClick.AddListener(OnButtonClicked);
        }
        
        public void Setup(ClassTemplate template, ClassSelectionUI ui)
        {
            classTemplate = template;
            selectionUI = ui;
            
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            if (className) className.text = classTemplate.className;
            if (classRole) classRole.text = classTemplate.classRole.ToString();
            
            if (classIcon)
            {
                if (classTemplate.classIcon)
                {
                    classIcon.sprite = classTemplate.classIcon;
                    classIcon.color = Color.white;
                }
                else
                {
                    classIcon.sprite = null;
                    classIcon.color = classTemplate.classColor;
                }
            }
        }
        
        private void OnButtonClicked()
        {
            if (selectionUI) selectionUI.SelectClass(classTemplate);
        }
    }
}
