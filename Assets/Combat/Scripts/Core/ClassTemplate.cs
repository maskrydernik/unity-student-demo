using UnityEngine;
using System.Collections.Generic;

namespace MiniWoW
{
    [System.Serializable]
    public class ClassStat
    {
        public string statName;
        public float baseValue;
        public float perLevelGain;
    }

    [System.Serializable]
    public class StartingAbility
    {
        public string abilityName;
        public AbilityDefinition abilityDefinition;
        public int slotIndex; // Which ability slot (0-4) this goes in
    }

    [CreateAssetMenu(menuName = "MiniWoW/Class Template", fileName = "NewClass")]
    public class ClassTemplate : ScriptableObject
    {
        [Header("Class Identity")]
        public string className = "New Class";
        [TextArea(2, 4)]
        public string shortConcept = "A brief description of this class.";
        public ClassRole classRole = ClassRole.Damage;
        
        [Header("Core Identity")]
        public ArmorType armorType = ArmorType.Cloth;
        public List<WeaponType> weaponTypes = new List<WeaponType>();
        public PrimaryResource primaryResource = PrimaryResource.Mana;
        public List<ClassStat> coreStats = new List<ClassStat>();
        
        [Header("Unique Mechanic")]
        [TextArea(3, 5)]
        public string uniqueMechanic = "Describe what makes this class special.";
        
        [Header("Starting Abilities")]
        public List<StartingAbility> startingAbilities = new List<StartingAbility>();
        
        [Header("100-Word Summary")]
        [TextArea(5, 8)]
        public string classSummary = "Write about 100 words describing your class, its theme, playstyle, and what makes it fun or different.";
        
        [Header("Visual & Audio")]
        public GameObject characterPrefab;
        public Sprite classIcon;
        public Color classColor = Color.white;
        public AudioClip classSound;
        
        [Header("Gameplay Settings")]
        public float baseHealth = 100f;
        public float baseResource = 100f;
        public float healthPerLevel = 10f;
        public float resourcePerLevel = 5f;
        public float movementSpeed = 5f;
        
        public enum ClassRole
        {
            Tank,
            Healer,
            Damage,
            Hybrid
        }
        
        public enum ArmorType
        {
            Cloth,
            Leather,
            Mail,
            Plate
        }
        
        public enum WeaponType
        {
            Sword,
            Axe,
            Mace,
            Dagger,
            Bow,
            Crossbow,
            Staff,
            Wand,
            Shield,
            Fist
        }
        
        public enum PrimaryResource
        {
            Mana,
            Rage,
            Energy,
            Focus,
            Custom
        }
        
        public float GetStatValue(string statName, int level = 1)
        {
            foreach (var stat in coreStats)
            {
                if (stat.statName == statName)
                {
                    return stat.baseValue + (stat.perLevelGain * (level - 1));
                }
            }
            return 0f;
        }
        
        public float GetHealthAtLevel(int level)
        {
            return baseHealth + (healthPerLevel * (level - 1));
        }
        
        public float GetResourceAtLevel(int level)
        {
            return baseResource + (resourcePerLevel * (level - 1));
        }
    }
}
