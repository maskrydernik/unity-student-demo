# Animation Setup - Quick Reference

## How Animations Are Registered

The script now registers all animations with **simple, consistent names** in the Animation component:

### **State Animations:**
| Inspector Field | Registered As | Wrap Mode |
|----------------|---------------|-----------|
| `animIdle` | "Idle" | Loop |
| `animWalk` | "Walk" | Loop |
| `animJump` | "Jump" | Once |
| `animFall` | "Fall" | Loop |
| `animDash` | "Dash" | Once |
| `animHitstun` | "Hitstun" | Once |
| `animKO` | "KO" | Once |

### **Attack Animations:**
| Inspector Field | Registered As | Wrap Mode |
|----------------|---------------|-----------|
| `lightAttack.anim` | "LightAttack" | Once |
| `mediumAttack.anim` | "MediumAttack" | Once |
| `heavyAttack.anim` | "HeavyAttack" | Once |

## What This Means

✅ **You assign AnimationClips in the Inspector** (same as before)
✅ **Script registers them with simple names** automatically in `Awake()`
✅ **Animations play using these registered names** (Idle, Walk, LightAttack, etc.)

## No More Errors!

The previous error was because:
- ❌ OLD: Tried to play clips by their asset name (which might be different)
- ✅ NEW: Registers clips with consistent names ("Idle", "Walk", etc.)

## What You See in Unity

When you select the GameObject at runtime, the Animation component will show:
- "Idle" ← Your idle animation
- "Walk" ← Your walk animation
- "Jump" ← Your jump animation
- etc.

All automatically registered from your Inspector assignments!

## Summary

**Before:** Animation couldn't find "Idle" because it wasn't registered
**After:** All animations are registered with predictable names in `SetupAnimations()`

Just assign your clips in the Inspector and the script handles the rest! 🎮
