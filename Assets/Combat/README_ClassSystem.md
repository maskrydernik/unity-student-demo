# Unity Combat Class System

A comprehensive class template system for Unity that allows you to create, configure, and spawn character classes with unique abilities, stats, and gameplay mechanics.

## Features

- **Class Template System**: Create detailed character classes with stats, abilities, and unique mechanics
- **Visual Class Wizard**: Easy-to-use editor tool for creating and configuring classes
- **Automatic Prefab Generation**: Generate fully functional prefabs from class templates
- **Class Selection UI**: In-game interface for choosing and spawning different classes
- **Combat Integration**: Seamlessly integrates with the existing combat system

## Quick Start

### 1. Setup the Combat System

1. Add a `CombatSystemSetup` component to any GameObject in your scene
2. Click "Setup Complete Combat System" in the inspector
3. The system will automatically:
   - Create the scene setup
   - Generate class selection UI
   - Load available class templates

### 2. Create Your First Class

1. Go to `MiniWoW > Class Setup Wizard` in the menu
2. Fill out the class information:
   - **Basic Info**: Name, concept, role, armor type, weapons
   - **Stats & Resources**: Core stats, health, resource values
   - **Abilities**: Starting abilities and their slot assignments
   - **Summary**: Visual and audio settings
3. Click "Save Template" to create your class
4. Click "Create Prefab" to generate a playable character

### 3. Play with Your Classes

1. Run the scene
2. Use the class selection UI to choose a class
3. Click "Spawn Class" to create your character
4. Use number keys 1-5 to cast abilities
5. Click on enemies to target them

## Class Template Structure

### Class Identity
- **Class Name**: Display name for the class
- **Short Concept**: Brief description (1-2 sentences)
- **Class Role**: Tank, Healer, Damage, or Hybrid
- **Core Identity**: Armor type, weapon types, primary resource

### Core Stats
Define base values and per-level gains for:
- Strength, Agility, Intellect, Spirit
- Custom stats as needed

### Starting Abilities
Configure up to 5 starting abilities:
- Ability name and definition
- Slot assignment (0-4)
- Automatic integration with the ability system

### Unique Mechanics
Describe what makes your class special:
- Special systems or features
- Unique gameplay elements
- Class-specific behaviors

## Example Classes

The system includes four example classes:

### Warrior
- **Role**: Tank/Damage
- **Armor**: Plate
- **Weapons**: Sword, Axe, Mace, Fist
- **Resource**: Rage
- **Unique Mechanic**: Builds rage through combat and taking damage

### Mage
- **Role**: Damage
- **Armor**: Cloth
- **Weapons**: Staff, Wand
- **Resource**: Mana
- **Unique Mechanic**: Spell combinations and elemental synergies

### Priest
- **Role**: Healer
- **Armor**: Cloth
- **Weapons**: Mace, Staff, Wand
- **Resource**: Mana
- **Unique Mechanic**: Divine channeling and healing auras

### Rogue
- **Role**: Damage
- **Armor**: Leather
- **Weapons**: Dagger, Bow, Crossbow
- **Resource**: Energy
- **Unique Mechanic**: Stealth and combo points

## Scripts Overview

### Core Scripts
- `ClassTemplate.cs`: ScriptableObject for defining classes
- `ClassSetupWizard.cs`: Editor tool for creating classes
- `ClassPrefabGenerator.cs`: Automatic prefab generation
- `CombatSceneSetup.cs`: Scene configuration and setup
- `CombatSystemSetup.cs`: Complete system setup

### UI Scripts
- `ClassSelectionUI.cs`: Class selection interface
- `AbilityBarUI.cs`: Ability bar display
- `PlayerFrameUI.cs`: Player information display
- `TargetFrameUI.cs`: Target information display

## Usage Examples

### Creating a New Class

```csharp
// Create a new class template
ClassTemplate newClass = ScriptableObject.CreateInstance<ClassTemplate>();
newClass.className = "Paladin";
newClass.classRole = ClassTemplate.ClassRole.Hybrid;
newClass.armorType = ClassTemplate.ArmorType.Plate;
newClass.primaryResource = ClassTemplate.PrimaryResource.Mana;

// Configure stats
newClass.coreStats.Add(new ClassStat { 
    statName = "Strength", 
    baseValue = 12f, 
    perLevelGain = 2.5f 
});

// Save the template
AssetDatabase.CreateAsset(newClass, "Assets/MyClasses/Paladin.asset");
```

### Spawning a Class

```csharp
// Generate prefab from template
GameObject playerPrefab = ClassPrefabGenerator.GenerateClassPrefab(classTemplate);

// Spawn in scene
GameObject player = Instantiate(playerPrefab, spawnPosition, spawnRotation);
```

### Accessing Class Data

```csharp
// Get class information
string className = classTemplate.className;
float healthAtLevel10 = classTemplate.GetHealthAtLevel(10);
float strengthValue = classTemplate.GetStatValue("Strength", 5);
```

## Integration with Combat System

The class system integrates seamlessly with the existing combat system:

- **Health System**: Classes automatically configure health values
- **Ability System**: Starting abilities are automatically assigned
- **Targeting System**: All classes support targeting
- **UI System**: Class information displays in the UI
- **Faction System**: Classes are automatically assigned to the Player faction

## Customization

### Adding Custom Stats
```csharp
// Add custom stat to class template
newClass.coreStats.Add(new ClassStat { 
    statName = "Luck", 
    baseValue = 5f, 
    perLevelGain = 1f 
});
```

### Creating Custom Abilities
1. Create an `AbilityDefinition` ScriptableObject
2. Configure damage, healing, cooldowns, etc.
3. Assign to class template's starting abilities

### Custom Visual Elements
- Set `classColor` for visual distinction
- Assign `classIcon` for UI display
- Configure `characterPrefab` for custom models

## Troubleshooting

### Common Issues

1. **Class not spawning**: Check that the class template is assigned to `CombatSystemSetup`
2. **Abilities not working**: Verify ability definitions are properly configured
3. **UI not showing**: Ensure the class selection UI is properly set up
4. **Prefab generation fails**: Check that all required components are present

### Debug Tips

- Use the `CombatSceneSetup` component's inspector buttons for testing
- Check the console for error messages
- Verify that class templates are properly saved as assets
- Ensure all required scripts are compiled without errors

## Advanced Features

### Class Inheritance
Create base classes and derive specialized versions:
```csharp
// Create a base warrior class
ClassTemplate baseWarrior = CreateBaseWarrior();

// Create specialized versions
ClassTemplate berserker = CreateBerserker(baseWarrior);
ClassTemplate guardian = CreateGuardian(baseWarrior);
```

### Dynamic Class Loading
Load classes at runtime:
```csharp
// Load class from resources
ClassTemplate loadedClass = Resources.Load<ClassTemplate>("Classes/MyClass");

// Generate and spawn
GameObject player = ClassPrefabGenerator.GenerateClassPrefab(loadedClass);
```

### Class Progression
Implement level-based progression:
```csharp
// Get stats at different levels
float healthAtLevel1 = classTemplate.GetHealthAtLevel(1);
float healthAtLevel50 = classTemplate.GetHealthAtLevel(50);
```

## Contributing

To add new features or improve the system:

1. Follow the existing code structure
2. Add proper documentation
3. Test with multiple class types
4. Ensure backward compatibility
5. Update this README with new features

## Support

For issues or questions:
1. Check the console for error messages
2. Verify all required components are present
3. Test with the example classes first
4. Check that all scripts compile without errors
