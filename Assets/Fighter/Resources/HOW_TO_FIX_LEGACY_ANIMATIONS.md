# How to Mark AnimationClips as Legacy

## The Problem

Unity's **Animation** component (Legacy system) requires all AnimationClips to be marked as "Legacy" type. If they're not, you'll see errors like:

```
The AnimationClip 'Fall' used by the Animation component must be marked as Legacy.
```

## The Solution - 2 Methods

### **Method 1: Using Inspector (Recommended)**

1. **Select your AnimationClip** in the Project window
2. In the Inspector, click the **☰ menu** (top right)
3. Select **"Debug"** mode
4. Find the **"Legacy"** checkbox
5. ✅ **Check the "Legacy" box**
6. Click **☰ menu** → "Normal" to return to normal mode
7. **Repeat for ALL your animation clips**

### **Method 2: Using Import Settings** (For FBX/3D animations)

1. **Select your animation file** (FBX, etc.) in the Project window
2. Go to the **"Animation"** tab in Inspector
3. Change **"Animation Type"** from "Generic" or "Humanoid" to **"Legacy"**
4. Click **"Apply"**

---

## Which Clips Need to Be Legacy?

**ALL of them!** Every clip you assigned in the Inspector needs to be Legacy:

### ✅ State Animations (7 clips):
- [ ] Idle
- [ ] Walk
- [ ] Jump
- [ ] Fall
- [ ] Dash
- [ ] Hitstun
- [ ] KO

### ✅ Attack Animations (3 clips):
- [ ] Light Attack
- [ ] Medium Attack
- [ ] Heavy Attack

---

## Quick Checklist

1. ✅ Select first AnimationClip
2. ✅ Inspector → Debug mode
3. ✅ Check "Legacy" box
4. ✅ Inspector → Normal mode
5. ✅ Repeat for all 10 clips
6. ✅ Press Play in Unity

---

## The Script Will Help You!

The script now **automatically detects** non-Legacy clips and shows you:
- ❌ Which clip is the problem
- ✅ Clear instructions to fix it
- 🛑 Disables the fighter until fixed

When you see the error:
```
AnimationClip 'Fall' must be marked as LEGACY!
Fix: Select the clip → Inspector → Debug mode → Check 'Legacy' checkbox
```

Just follow the instructions for each clip!

---

## Why Legacy?

Unity has two animation systems:
- **Legacy (Animation component)** - Simple, direct playback (what we use)
- **Mecanim (Animator component)** - Complex state machines (what we removed)

Since we're using the Legacy Animation component for direct playback, all clips must be marked as Legacy type.

---

## Visual Guide

```
Project Window:
└── Animations/
    ├── Idle.anim          ← Select this
    ├── Walk.anim          ← Then this
    ├── Jump.anim          ← Then this
    └── ...                ← etc.

For Each Clip:
Inspector (Top Right) → ☰ → Debug → ✅ Legacy → ☰ → Normal
```

---

## Still Having Issues?

If clips still won't work:
1. Make sure you checked the Legacy box for **every single clip**
2. Try closing and reopening Unity
3. Check the Console for the specific clip name causing issues
4. The script will tell you exactly which clip needs to be fixed!

---

## Summary

🎯 **Goal**: Mark all 10 AnimationClips as Legacy
📍 **Where**: Inspector → Debug mode → Legacy checkbox
✅ **When**: Before pressing Play
🎮 **Result**: Animations will work perfectly!
