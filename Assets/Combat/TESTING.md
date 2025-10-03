# Combat System - Quick Test Guide

## Current Status

✅ All scripts have been updated with:
- **Debug logging** for ability key presses
- **Placeholder UI visuals** (color-coded ability slots)
- **Player health UI** (top-left corner)
- **Target health UI** (top-center)
- **Ability bar UI** (bottom-center, 5 slots)
- **Floating damage/heal numbers**
- **Visible projectiles** (cyan spheres)

## What Was Fixed

### 1. Ability Keys (1-5)
- ✅ Added keypad support (Keypad1-5)
- ✅ Added debug logging for each key press
- ✅ Shows why abilities fail (cooldown, range, target, etc.)

### 2. UI Improvements
- ✅ **Ability Bar** - Color-coded slots:
  - Red = Damage abilities
  - Green = Healing abilities  
  - Blue = Utility abilities
  - Gray = Empty slots
- ✅ **Cooldown overlays** - Radial dark overlay shows remaining cooldown
- ✅ **Player Health Frame** - Top-left corner shows your HP
- ✅ **Target Health Frame** - Top-center shows target HP
- ✅ **Floating Numbers** - Damage (red) and healing (green) numbers

### 3. Visual Feedback
- ✅ Projectiles now have visible cyan spheres
- ✅ Selected targets turn yellow
- ✅ Health bars update in real-time
- ✅ Damage/heal numbers float upward

## How to Test

### Step 1: Run the Setup Wizard
1. Open Unity
2. Menu: **Tools > Combat > Generate Sample Content**
3. Wait for "Sample content generated..." dialog
4. Click OK

### Step 2: Open the Scene
1. Navigate to `Assets/Combat/GeneratedContent/Scenes/`
2. Double-click `CombatSample.unity`
3. Press **Play** button

### Step 3: Select a Player
1. A UI will appear with 3 player options
2. Click any button (PlayerPrefab, MagePrefab, or PriestPrefab)
3. Player will spawn at center

### Step 4: Test Movement
1. Press **W A S D** to move around
2. Hold **Right Mouse Button** and move mouse to rotate
3. **Scroll wheel** to zoom camera

### Step 5: Test Targeting
1. **Left-click** on the red enemy dummy (right side)
2. It should turn **yellow** (selected)
3. Top-center UI should show: "Training Dummy [Enemy]"
4. Health bar should show full health (green bar)

### Step 6: Test Damage Abilities
1. With enemy dummy selected, press **1** (Fireball)
   - Watch console for: "[AbilitySystem] Casting Fireball on Training Dummy"
   - Cyan projectile should fly toward target
   - Red damage number "-40" should appear
   - Enemy health bar should decrease
   - Ability slot 1 should show cooldown overlay

2. Press **1** again immediately
   - Console should show: "[AbilitySystem] Fireball on cooldown: X.Xs"
   - Nothing happens (ability on cooldown)

3. Wait 2 seconds, press **1** again
   - Should cast successfully

4. Try other damage abilities:
   - Press **2** for Smite (instant 25 damage)
   - Press **4** for Bolt (projectile 20 damage)

### Step 7: Test Healing Abilities
1. **Left-click** on the other dummy (friendly)
2. It should turn yellow
3. Top-center UI shows: "Friendly Dummy [Friendly]"

4. Press **3** (Heal)
   - Console: "[AbilitySystem] Casting Heal on Friendly Dummy"
   - Green healing number "+35" should appear
   - Health increases (if it was damaged)

5. Press **5** (Big Heal)
   - Heals for 60 HP
   - 4 second cooldown

### Step 8: Test Faction Rules
1. Target the **friendly dummy**
2. Press **1** (Fireball - enemy only ability)
   - Console: "[AbilitySystem] Fireball invalid target faction"
   - Nothing happens (correct behavior!)

3. Target the **enemy dummy**
4. Press **3** (Heal - friendly only ability)
   - Console: "[AbilitySystem] Heal invalid target faction"
   - Nothing happens (correct behavior!)

### Step 9: Test Self-Targeting
1. **Left-click on your own character** (the capsule you're controlling)
2. Your character should turn yellow
3. Press **3** or **5** (healing abilities)
   - You can heal yourself!
   - Green numbers should appear above you
   - Your health bar (top-left) should increase

## What You Should See

### UI Elements

```
┌─────────────────────────────────────────────────────┐
│ [Player Frame]           [Target Frame]             │
│  Player                   Training Dummy [Enemy]    │
│  HP: 500/500              ██████████░░░░░░░░        │
│                                                      │
│                                                      │
│                      [Floating -40]                  │
│                         ↑↑↑                          │
│                                                      │
│                                                      │
│                                                      │
│                    [Ability Bar]                     │
│                 [1] [2] [3] [4] [5]                  │
│                 🔴  🔴  🟢  🔴  🟢                    │
└─────────────────────────────────────────────────────┘
```

Legend:
- 🔴 = Red (damage abilities)
- 🟢 = Green (healing abilities)
- Numbers = Key bindings

### Console Messages

When working correctly, you should see:
```
[AbilityBarUI] Initialized with 5 ability slots
[TargetFrameUI] Initialized
[PlayerFrameUI] Initialized

[AbilitySystem] Key 1 pressed
[AbilitySystem] Casting Fireball on Training Dummy

[AbilitySystem] Key 1 pressed
[AbilitySystem] Fireball on cooldown: 1.8s

[AbilitySystem] Key 3 pressed
[AbilitySystem] Casting Heal on Friendly Dummy
```

## Troubleshooting

### "I don't see any UI"
- Check console for initialization messages
- Make sure you selected a player (not still on selection screen)
- Try restarting the scene

### "Keys 1-5 don't work"
- Check console when pressing keys
- You should see "[AbilitySystem] Key X pressed"
- If not, make sure game window has focus (click on it)
- Try using numpad keys (Keypad1-5)

### "Abilities don't cast"
- Check console for error messages:
  - "requires target" = Click on a target first
  - "on cooldown" = Wait for cooldown to finish
  - "invalid target faction" = Wrong target type
  - "out of range" = Move closer (within 25 units)

### "I can't see projectiles"
- Projectiles are small cyan spheres
- They move fast (0.6 seconds)
- Watch carefully when casting Fireball (key 1) or Bolt (key 4)

### "Damage numbers don't show"
- They appear briefly (1.5 seconds)
- Red for damage, green for healing
- Float upward from target

### "Target doesn't turn yellow"
- Left-click directly on the capsule
- Make sure you're not clicking UI elements
- Try clicking the middle/top of the capsule

## Advanced Testing

### Test Cooldowns
1. Press **1 2 4** rapidly
2. All three should go on cooldown (dark overlay)
3. Watch as they gradually clear (2 seconds each)

### Test Range
1. Target enemy dummy
2. Move very far away (run with WASD)
3. Try casting - should fail with "out of range"
4. Move back closer and try again

### Test No Target
1. Press **Escape** to clear target
2. Try pressing **1** (Fireball)
3. Should fail with "requires target"

### Test Combat Loop
1. Target enemy dummy
2. Cast: 1 → 2 → 4 → 1 → 2 (rotation)
3. Watch health bar deplete
4. Damage numbers should keep appearing
5. Cooldowns should rotate properly

## Performance Check

- ✅ 60 FPS or higher (should be very smooth)
- ✅ No lag when casting abilities
- ✅ Projectiles move smoothly
- ✅ UI updates smoothly
- ✅ No memory leaks (can run indefinitely)

## Known Issues (Expected)

These are limitations of the prototype:
- No animations (by design)
- No sound effects (fields exist, no audio clips)
- No particle effects (fields exist, no VFX prefabs)
- Training dummies don't fight back (not implemented yet)
- No death state for dummies (health can go to 0 but they stay)

## Next Steps

If everything works:
1. ✅ All systems functional
2. ✅ Ready for customization
3. ✅ Can add your own abilities
4. ✅ Can create your own player prefabs
5. ✅ Can add animations, sounds, VFX

See `README.md` for customization guide!

## Quick Reference Card

| Key | Ability | Type | Effect | Cooldown |
|-----|---------|------|--------|----------|
| **1** | Fireball | Projectile Damage | 40 damage | 2s |
| **2** | Smite | Instant Damage | 25 damage | 2s |
| **3** | Heal | Instant Healing | 35 healing | 2s |
| **4** | Bolt | Projectile Damage | 20 damage | 2s |
| **5** | Big Heal | Instant Healing | 60 healing | 4s |

| Key | Action |
|-----|--------|
| **W A S D** | Move |
| **Space** | Jump |
| **Left Click** | Select target |
| **Right Click** | Rotate character |
| **Mouse Wheel** | Zoom camera |
| **Escape** | Clear target |
| **1 2 3 4 5** | Cast abilities |

---

**Remember:** Check the Console window for debug messages - they tell you exactly what's happening!
