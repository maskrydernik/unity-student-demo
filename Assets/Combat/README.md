# Combat System

A self-contained, MMO-style combat system for Unity inspired by World of Warcraft mechanics.

## Overview

This folder contains a complete combat system with the following features:
- Player movement and camera controls
- Target selection system
- Ability system with cooldowns
- Health and faction system
- Training dummies for testing
- UI for abilities and target frames
- Editor tools for content generation

## Folder Structure

```
Combat/
├── Editor/
│   └── SetupWizard.cs          # Editor tool to generate sample content
├── Scripts/
│   ├── Abilities/
│   │   ├── AbilityDefinition.cs    # ScriptableObject for ability data
│   │   ├── AbilitySystem.cs        # Player ability execution system
│   │   └── Projectile.cs           # Projectile movement and collision
│   ├── Camera/
│   │   └── CameraController.cs     # MMO-style camera controls
│   ├── Core/
│   │   ├── Faction.cs              # Faction enum and rules
│   │   ├── GameManager.cs          # Main game setup and initialization
│   │   ├── Health.cs               # Health component with damage/heal
│   │   ├── Targetable.cs           # Makes objects targetable
│   │   └── TargetingSystem.cs      # Player targeting system
│   ├── Dummy/
│   │   └── TrainingDummy.cs        # Training dummy setup
│   ├── Movement/
│   │   └── PlayerMotor.cs          # Player movement controller
│   └── UI/
│       ├── AbilityBarUI.cs         # Ability bar UI display
│       └── TargetFrameUI.cs        # Target frame UI display
└── GeneratedContent/               # Created by SetupWizard
    ├── Abilities/                  # Ability ScriptableObjects
    ├── Prefabs/                    # Player and projectile prefabs
    └── Scenes/                     # Sample combat scene
```

## Getting Started

### Option 1: Use the Setup Wizard (Recommended)

1. In Unity, go to **Tools > Combat > Generate Sample Content**
2. This will create:
   - Projectile prefab
   - Sample abilities (Fireball, Smite, Heal, Bolt, Big Heal)
   - Player prefabs (PlayerPrefab, MagePrefab, PriestPrefab)
   - A sample scene at `Assets/Combat/GeneratedContent/Scenes/CombatSample.unity`
3. Open the generated scene and press Play

### Option 2: Manual Setup

1. Create a new scene
2. Add a GameObject with the `GameManager` component
3. The GameManager will automatically:
   - Create an EventSystem
   - Create a ground plane
   - Spawn enemy and friendly training dummies
   - Display player selection UI
4. Select a player to spawn and start playing

## Controls

### Movement
- **WASD** - Move character
- **Mouse Right Button** - Rotate character
- **Space** - Jump

### Camera
- **Mouse Left Button** - Orbit camera around character
- **Mouse Right Button + Drag** - Rotate character and adjust camera pitch
- **Mouse Scroll Wheel** - Zoom in/out

### Combat
- **Mouse Left Click** - Select target
- **1-5** - Cast abilities in slots 1-5
- **Escape** - Clear target

## System Components

### Core Systems

#### Health Component
- Manages unit health (current/max)
- Handles damage and healing
- Tracks faction (Player, Friendly, Enemy)
- Fires events on health change and death

#### Targetable Component
- Makes objects selectable by the targeting system
- Displays selection highlight (yellow tint)
- Provides display name and aim point
- Links to Health component for faction checking

#### TargetingSystem Component
- Handles mouse click targeting
- Raycasts to select units
- Manages current target
- Prevents UI click-through

#### Faction System
- Three factions: Player, Friendly, Enemy
- Player and Friendly are allies
- FactionRules helper class for friend/foe checks

### Ability System

#### AbilityDefinition (ScriptableObject)
- Defines ability properties:
  - Targeting rules (Self, Friendly, Enemy, Any)
  - Damage and healing values
  - Cooldown and cast time
  - Range and projectile settings
  - VFX and SFX references
  - Can cast while moving flag

#### AbilitySystem Component
- Manages 5 ability slots
- Handles cooldowns and global cooldown (GCD)
- Validates targeting and range
- Executes abilities (instant or projectile)
- Applies damage/healing effects

#### Projectile Component
- Moves toward target (homing or fixed trajectory)
- Supports travel time or speed-based movement
- Triggers callback on arrival
- Auto-destroys on impact

### Movement and Camera

#### PlayerMotor Component
- Character controller-based movement
- Camera-relative controls (WASD relative to camera view)
- Jump and gravity
- Mouse-controlled rotation
- Integrates with CameraController

#### CameraController Component
- MMO-style orbit camera
- Mouse-controlled orbit and zoom
- Pitch constraints (prevent upside-down)
- Syncs with player rotation when right-clicking
- Handles cursor lock/unlock

### UI Systems

#### AbilityBarUI Component
- Displays 5 ability slots at bottom center
- Shows ability icons (if set)
- Displays cooldown overlays (radial fill)
- Auto-creates canvas if needed

#### TargetFrameUI Component
- Displays at top center
- Shows target name and faction
- Health bar with fill indicator
- Auto-hides when no target

## Customization

### Creating New Abilities

1. Right-click in Project window
2. **Create > MiniWoW > Ability Definition**
3. Configure:
   - Display name and ID
   - Target rule (Self/Friendly/Enemy/Any)
   - Range and requires target
   - Damage/healing values
   - Cooldown and GCD
   - Projectile settings (if needed)
   - VFX and SFX (optional)

### Creating Player Prefabs

1. Create a capsule GameObject
2. Add components:
   - CharacterController
   - Health (set faction to Player)
   - Targetable
   - TargetingSystem
   - AbilitySystem (assign abilities to loadout)
   - PlayerMotor
3. Save as prefab
4. Assign to GameManager's playerPrefabs array

### Extending the System

#### Adding New Targeting Rules
Edit `Faction.cs` to add new factions and modify `FactionRules` class.

#### Custom Ability Effects
Extend `AbilitySystem.ApplyEffects()` to add:
- Damage over time
- Buffs/debuffs
- Area of effect
- Chain targeting

#### Advanced AI
Add scripts to `Dummy/` folder for:
- Auto-attacking
- Ability usage
- Movement behaviors

## Dependencies

All scripts are self-contained within the Combat folder and only depend on:
- Unity built-in namespaces (UnityEngine, UnityEngine.UI, UnityEngine.EventSystems)
- Unity Editor namespaces (for SetupWizard.cs only)
- System namespaces (System, System.Collections.Generic, System.IO)

**No external packages or assets required!**

## Namespace

All scripts use the `MiniWoW` namespace to avoid conflicts with other systems.

## Technical Notes

### Reflection Usage
- `Targetable.displayName` uses reflection to set private fields
- This is for component-based initialization
- Consider making `displayName` public if reflection is undesirable

### Material Instances
- `Targetable` creates material instances for selection highlighting
- This prevents affecting shared materials
- Materials are automatically cleaned up by Unity

### Canvas Management
- UI components auto-create canvases if not provided
- Uses ScreenSpaceOverlay mode
- CanvasScaler set to 1920x1080 reference resolution

### Performance Considerations
- Raycasting limited to mouse clicks (not per-frame)
- Cooldown checks use dictionary lookup
- Material color changes only on selection state change
- Projectiles auto-destroy on arrival

## License

This is a demonstration/educational combat system. Feel free to use and modify as needed.
