# 🎯 TARGETING TROUBLESHOOTING GUIDE

## Issue: Can't Target Enemies or Allies

### Debug Logging Added

I've added extensive debug logging to the `TargetingSystem` to help diagnose the issue.

## How To Test

### Step 1: Open Scene and Press Play
```
Assets/Combat/GeneratedContent/Scenes/CombatSample.unity
```

### Step 2: Select a Player
Click one of the player buttons

### Step 3: Try to Click on a Dummy
Left-click on the red or green training dummy

### Step 4: Check Console Output

You should see messages like:

#### If Everything Works:
```
[TargetingSystem] Initialized with camera: MainCamera
[TargetingSystem] Mouse down at (960, 540)
[TargetingSystem] Mouse up - Distance: 1.50, Time: 0.150, Over UI: False
[TargetingSystem] Raycasting from (960, 540) with ray: (0, 5, -10) -> (0.5, -0.3, 0.8)
[TargetingSystem] Hit: EnemyDummy at (6, 1, 10)
[TargetingSystem] Found Targetable: Training Dummy
[TargetingSystem] Selected: Training Dummy (Faction: Enemy)
```

#### If Camera Missing:
```
[TargetingSystem] No camera found! Targeting will not work.
```
**Fix:** Camera wasn't created when player spawned. Check SimplePlayerSpawner logs.

#### If Click Over UI:
```
[TargetingSystem] Mouse down at (960, 540)
[TargetingSystem] Mouse up - Distance: 0.50, Time: 0.100, Over UI: True
[TargetingSystem] Click rejected: pointer over UI
```
**Fix:** You're clicking on UI elements. Click directly on the dummy capsule.

#### If Raycast Misses:
```
[TargetingSystem] Mouse down at (960, 540)
[TargetingSystem] Mouse up - Distance: 0.50, Time: 0.100, Over UI: False
[TargetingSystem] Raycasting from (960, 540) with ray: (0, 5, -10) -> (0.5, -0.3, 0.8)
[TargetingSystem] Raycast hit nothing (Layer mask: -1)
```
**Fix:** Raycast didn't hit any colliders. See "Common Causes" below.

#### If Hit But No Targetable:
```
[TargetingSystem] Hit: Ground at (5, 0, 8)
[TargetingSystem] Hit object 'Ground' has no Targetable component!
```
**Fix:** You clicked on something else (ground, etc). Try clicking the dummy directly.

#### If Click Too Fast/Slow:
```
[TargetingSystem] Click rejected: moved too far (15.50 > 5)
```
or
```
[TargetingSystem] Click rejected: took too long (0.300 > 0.25)
```
**Fix:** Click more precisely and quickly. These are safety checks to distinguish clicks from drags.

## Common Causes & Solutions

### 1. Camera Not Found
**Symptom:** `[TargetingSystem] No camera found!`

**Check:**
- Is MainCamera created after player spawn?
- Look in Hierarchy for "MainCamera"
- Select it → Tag should be "MainCamera"

**Fix:**
Regenerate scene:
```
Tools → Combat → Generate Sample Content
```

### 2. TargetingSystem Not on Player
**Symptom:** No targeting messages at all in console

**Check:**
- Select Player in Hierarchy
- Inspector should show `TargetingSystem` component

**Fix:**
Verify player prefab has TargetingSystem:
```
Assets/Combat/GeneratedContent/Prefabs/PlayerPrefab.prefab
```
Should have TargetingSystem component. If missing, regenerate:
```
Tools → Combat → Generate Sample Content
```

### 3. Dummies Have No Colliders
**Symptom:** Raycast hits nothing when clicking dummy

**Check:**
- Select EnemyDummy in Hierarchy
- Inspector should show `Capsule Collider` component
- Collider should be enabled (checkbox checked)

**Fix:**
Dummies are created with `CreatePrimitive(PrimitiveType.Capsule)` which includes collider.
If missing, regenerate scene.

### 4. Dummies Have No Targetable Component
**Symptom:** `Hit object has no Targetable component!`

**Check:**
- Select EnemyDummy in Hierarchy
- Inspector should show `Targetable` component

**Fix:**
Regenerate scene - setup code should add Targetable.

### 5. Camera Looking Wrong Direction
**Symptom:** Can see dummies but clicks don't hit

**Check:**
- Select MainCamera in Hierarchy
- Scene view → align view with camera (Ctrl+Shift+F)
- Does the camera see the dummies?

**Fix:**
CameraController should position camera automatically.
If broken, check PlayerMotor and CameraController are linked.

### 6. UI Blocking Clicks
**Symptom:** `Click rejected: pointer over UI`

**Check:**
- Are you clicking on the ability bar at bottom?
- Are you clicking on health bars?
- Try clicking dummy from different angle

**Fix:**
Click directly on the dummy capsule, avoiding UI elements.

### 7. Layer Mask Issue
**Symptom:** Raycast hits nothing despite clear line of sight

**Check:**
- Select Player in Hierarchy
- TargetingSystem component
- Target Mask field - should be "Everything" or all layers checked

**Fix:**
If wrong, set to -1 (everything) or manually check all layers.

### 8. Physics Not Initialized
**Symptom:** Raycasts always miss

**Very Rare Issue - Check:**
- Edit → Project Settings → Physics
- Default Contact Offset, etc. should have reasonable values

**Fix:**
Reset Physics settings to default.

## Quick Diagnostic Steps

### Test 1: Is TargetingSystem Working?
1. Press Play
2. Select player
3. Click anywhere in scene
4. Check Console for `[TargetingSystem] Mouse down/up` messages
5. If no messages: TargetingSystem not running

### Test 2: Is Camera Working?
1. After player spawn, check Console
2. Look for: `[TargetingSystem] Initialized with camera: MainCamera`
3. If "No camera found": Camera problem

### Test 3: Can Raycast Hit Anything?
1. Click on ground
2. Should see: `[TargetingSystem] Hit: Ground at ...`
3. If "Raycast hit nothing": Major physics/camera issue

### Test 4: Do Dummies Have Components?
1. Select EnemyDummy in Hierarchy
2. Should have:
   - Capsule Collider ✓
   - Mesh Renderer ✓
   - Health ✓
   - Targetable ✓
   - Training Dummy ✓

### Test 5: Is UI Blocking?
1. Click dummy
2. If `Click rejected: pointer over UI`
3. Try clicking from Scene view while Game view has focus

## Manual Fix (If Regeneration Doesn't Work)

### Add Targetable to Dummy:
1. Select EnemyDummy in Hierarchy
2. Add Component → Targetable
3. Add Component → Health (if missing)
4. Set Health → Faction to "Enemy"
5. Try clicking again

### Fix TargetingSystem:
1. Select Player in Hierarchy
2. If no TargetingSystem component:
   - Add Component → Targeting System
3. Check settings:
   - Target Mask: Everything (-1)
   - Click Max Move Pixels: 5
   - Click Max Time: 0.25

### Fix Camera Reference:
1. Select Player in Hierarchy
2. TargetingSystem component
3. Cam field should auto-fill with Main Camera
4. If not, drag MainCamera into Cam field

## Expected Behavior When Working

1. **Click on enemy dummy:**
   - Dummy turns yellow
   - Top UI shows "Training Dummy [Enemy]"
   - Health bar appears
   - Console: `[TargetingSystem] Selected: Training Dummy (Faction: Enemy)`

2. **Click on friendly dummy:**
   - Dummy turns yellow
   - Top UI shows "Friendly Dummy [Friendly]"
   - Health bar appears
   - Console: `[TargetingSystem] Selected: Friendly Dummy (Faction: Friendly)`

3. **Press Escape:**
   - Yellow color disappears
   - Top UI clears
   - Console: `[TargetingSystem] Target cleared`

4. **Click on ground:**
   - Nothing happens (ground has no Targetable)
   - Console: `[TargetingSystem] Hit object 'Ground' has no Targetable component!`

## Still Not Working?

### Enable Visual Debug:
Add this to TargetingSystem in the `TrySelectUnderCursor` method:

```csharp
// After getting the ray, add:
Debug.DrawRay(ray.origin, ray.direction * 200f, Color.red, 2f);
```

This will draw the raycast in Scene view for 2 seconds.

### Check Console Filtering:
- Console window → Make sure "Collapse" is OFF
- Make sure no filters active (Clear button)
- Look for ALL messages

### Create Test Scene:
1. Create new scene
2. Add a cube (GameObject → 3D Object → Cube)
3. Add Targetable component to cube
4. Add Health component to cube
5. Add player from prefab
6. Try targeting the cube

## Debug Checklist

After pressing Play and selecting player:

- [ ] Console shows: `[TargetingSystem] Initialized with camera: MainCamera`
- [ ] Clicking shows: `[TargetingSystem] Mouse down/up` messages
- [ ] No "Click rejected: pointer over UI" messages
- [ ] Raycast shows: `[TargetingSystem] Raycasting from...`
- [ ] Hit detection works: `[TargetingSystem] Hit: [object] at...`
- [ ] Targetable found: `[TargetingSystem] Found Targetable: [name]`
- [ ] Selection works: `[TargetingSystem] Selected: [name] (Faction: [faction])`
- [ ] Dummy turns yellow visually
- [ ] Target UI appears at top

If all checks pass: **Targeting is working!** ✅

If any check fails: Find that section above for specific fix.

## Most Likely Causes (In Order)

1. **You're clicking on UI elements** (ability bar, health bars)
   - Fix: Click directly on dummy capsule
   
2. **Camera not initialized yet**
   - Fix: Wait a moment after player spawns
   
3. **TargetingSystem not on player**
   - Fix: Regenerate scene/prefabs
   
4. **Dummy missing Targetable component**
   - Fix: Regenerate scene

5. **Clicking too fast or dragging**
   - Fix: Click more deliberately

## Contact Point

All debug logging will help you identify exactly where the issue is. Check the console messages and match them to the sections above!

**Remember:** After any changes, you may need to regenerate the scene!

```
Tools → Combat → Generate Sample Content
```
