using UnityEditor;
using UnityEngine;

public class SetLegacyFlag
{
    [MenuItem("Tools/Set Selected AnimClips Legacy")]
    static void SetLegacy()
    {
        foreach (Object obj in Selection.objects)
        {
            var clip = obj as AnimationClip;
            if (clip != null)
            {
                SerializedObject so = new SerializedObject(clip);
                so.FindProperty("m_Legacy").boolValue = true;
                so.ApplyModifiedProperties();
                Debug.Log("Set Legacy: " + clip.name);
            }
        }
    }
}
