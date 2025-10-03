#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;

public static class BasicFighter2DAnimatorBuilder
{
    [MenuItem("Tools/BasicFighter2D/Build Animator From Component")]
    public static void Build()
    {
        var go = Selection.activeGameObject;
        if (!go) { Debug.LogError("Select a GameObject with BasicFighter2D."); return; }

        var bf = go.GetComponent<BasicFighter2D>();
        var anim = go.GetComponent<Animator>();
        if (!bf || !anim) { Debug.LogError("Missing BasicFighter2D or Animator on selection."); return; }

        var path = EditorUtility.SaveFilePanelInProject(
            "Create Animator", $"{bf.fighterName}_BasicFighter2D", "controller", "Save Animator Controller");
        if (string.IsNullOrEmpty(path)) return;

        // Determine folder for placeholder clips
        var folderPath = Path.GetDirectoryName(path);
        var clipFolderPath = Path.Combine(folderPath, $"{bf.fighterName}_Animations");
        if (!AssetDatabase.IsValidFolder(clipFolderPath))
        {
            var parentFolder = Path.GetDirectoryName(clipFolderPath);
            var folderName = Path.GetFileName(clipFolderPath);
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        // Helper to get or create placeholder clip
        AnimationClip GetOrCreateClip(AnimationClip existing, string clipName)
        {
            if (existing != null) return existing;
            
            // Create placeholder
            var clipPath = Path.Combine(clipFolderPath, $"{clipName}.anim");
            var clip = new AnimationClip();
            clip.name = clipName;
            
            // Add a dummy keyframe so it's not completely empty
            var curve = AnimationCurve.Constant(0f, 1f, 0f);
            clip.SetCurve("", typeof(Transform), "localPosition.x", curve);
            
            AssetDatabase.CreateAsset(clip, clipPath);
            Debug.Log($"Created placeholder: {clipPath}");
            return clip;
        }

        // Get or create all clips
        var idleClip = GetOrCreateClip(bf.animIdle, "Idle");
        var walkClip = GetOrCreateClip(bf.animWalk, "Walk");
        var jumpClip = GetOrCreateClip(bf.animJump, "Jump");
        var fallClip = GetOrCreateClip(bf.animFall, "Fall");
        var dashClip = GetOrCreateClip(bf.animDash, "Dash");
        var hitstunClip = GetOrCreateClip(bf.animHitstun, "Hitstun");
        var koClip = GetOrCreateClip(bf.animKO, "KO");
        
        AnimationClip lightClip = null, mediumClip = null, heavyClip = null;
        if (bf.lightAttack != null)
            lightClip = GetOrCreateClip(bf.lightAttack.anim, $"{bf.lightAttack.name}_Light");
        if (bf.mediumAttack != null)
            mediumClip = GetOrCreateClip(bf.mediumAttack.anim, $"{bf.mediumAttack.name}_Medium");
        if (bf.heavyAttack != null)
            heavyClip = GetOrCreateClip(bf.heavyAttack.anim, $"{bf.heavyAttack.name}_Heavy");

        // Auto-assign created clips back to component
        if (bf.animIdle == null) bf.animIdle = idleClip;
        if (bf.animWalk == null) bf.animWalk = walkClip;
        if (bf.animJump == null) bf.animJump = jumpClip;
        if (bf.animFall == null) bf.animFall = fallClip;
        if (bf.animDash == null) bf.animDash = dashClip;
        if (bf.animHitstun == null) bf.animHitstun = hitstunClip;
        if (bf.animKO == null) bf.animKO = koClip;
        if (bf.lightAttack != null && bf.lightAttack.anim == null) bf.lightAttack.anim = lightClip;
        if (bf.mediumAttack != null && bf.mediumAttack.anim == null) bf.mediumAttack.anim = mediumClip;
        if (bf.heavyAttack != null && bf.heavyAttack.anim == null) bf.heavyAttack.anim = heavyClip;

        EditorUtility.SetDirty(bf);

        // Create controller
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);

        // Add bool parameters for each state (using fixed names)
        ctrl.AddParameter("Idle", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Walk", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Jump", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Fall", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Dash", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Hitstun", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("KO", AnimatorControllerParameterType.Bool);

        // Get layer + SM
        var layer = ctrl.layers[0];
        var sm = layer.stateMachine;

        // Clean slate: remove states and transitions via API
        foreach (var st in sm.states) sm.RemoveState(st.state);
        foreach (var t in sm.anyStateTransitions) sm.RemoveAnyStateTransition(t);
        foreach (var t in sm.entryTransitions) sm.RemoveEntryTransition(t);
        foreach (var sb in sm.stateMachines) sm.RemoveStateMachine(sb.stateMachine);

        AnimatorState defaultState = null;

        AnimatorState AddState(AnimationClip clip, bool makeDefault = false)
        {
            if (!clip) return null;
            // Ensure unique state name to avoid collisions
            var name = clip.name;
            int i = 1;
            while (System.Array.Exists(sm.states, s => s.state.name == name))
            {
                name = $"{clip.name}_{i++}";
            }

            var st = sm.AddState(name);
            st.motion = clip;
#if UNITY_2021_2_OR_NEWER
            st.writeDefaultValues = true;
#endif
            if (makeDefault || defaultState == null) defaultState = st;
            return st;
        }

        // Required states
        var idleState = AddState(idleClip, makeDefault: true);
        var walkState = AddState(walkClip);
        var jumpState = AddState(jumpClip);
        var fallState = AddState(fallClip);
        var dashState = AddState(dashClip);
        var hitstunState = AddState(hitstunClip);
        var koState = AddState(koClip);

        // Attack states
        if (lightClip != null) AddState(lightClip);
        if (mediumClip != null) AddState(mediumClip);
        if (heavyClip != null) AddState(heavyClip);

        sm.defaultState = defaultState;

        // Add transitions based on bool parameters
        void AddTransition(AnimatorState from, AnimatorState to, string paramName, bool value)
        {
            if (from == null || to == null || string.IsNullOrEmpty(paramName)) return;
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.exitTime = 0f;
            transition.duration = 0.1f;
            transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, paramName);
        }

        // Create transition network - each state transitions to all others based on their bool
        var states = new[] { 
            (idleState, "Idle"),
            (walkState, "Walk"),
            (jumpState, "Jump"),
            (fallState, "Fall"),
            (dashState, "Dash"),
            (hitstunState, "Hitstun"),
            (koState, "KO")
        };

        foreach (var (fromState, _) in states)
        {
            if (fromState == null) continue;
            foreach (var (toState, toParam) in states)
            {
                if (fromState != toState && toState != null)
                {
                    AddTransition(fromState, toState, toParam, true);
                }
            }
        }

        // Apply layer back and persist
        ctrl.layers = new[] { layer };
        anim.runtimeAnimatorController = ctrl;

        EditorUtility.SetDirty(ctrl);
        EditorUtility.SetDirty(anim);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Animator built and assigned: {path}");
        Debug.Log($"Placeholder animations created in: {clipFolderPath}");
    }
}
#endif
