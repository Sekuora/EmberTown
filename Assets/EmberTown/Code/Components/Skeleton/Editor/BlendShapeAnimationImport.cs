using UnityEditor;
using UnityEngine;

/// <summary>
/// Removes baked constant morph values from opted-in locomotion models.
/// Animated morph curves continue to drive expressions normally.
/// </summary>
public class BlendShapeAnimationImport : AssetPostprocessor
{
    public const string ImportMarker = "EmberTown.PreserveBlendShapeCustomization";

    private void OnPostprocessAnimation(GameObject root, AnimationClip clip)
    {
        if (!assetImporter.userData.Contains(ImportMarker)) { return; }
        RemoveConstantCurves(clip);
    }

    public static int RemoveConstantCurves(AnimationClip clip)
    {
        int removed = 0;
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
        {
            if (binding.type != typeof(SkinnedMeshRenderer) || !binding.propertyName.StartsWith("blendShape.")) { continue; }
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve == null || curve.length == 0) { continue; }
            Keyframe[] keys = curve.keys;
            bool constant = true;
            foreach (Keyframe key in keys)
            {
                if (!Mathf.Approximately(key.value, keys[0].value)) { constant = false; break; }
                if (keys.Length > 1 && ((!float.IsInfinity(key.inTangent) && key.inTangent != 0f) || (!float.IsInfinity(key.outTangent) && key.outTangent != 0f)))
                {
                    constant = false;
                    break;
                }
            }
            if (!constant) { continue; }
            AnimationUtility.SetEditorCurve(clip, binding, null);
            removed++;
        }
        return removed;
    }
}
