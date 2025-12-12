using UnityEngine;
using UnityEditor;

public class FixMeshColliders : EditorWindow
{
    [MenuItem("Tools/Fix MeshColliders (Disable Fast Midphase)")]
    static void FixColliders()
    {
        // New API for Unity 2022+
        MeshCollider[] colliders = Object.FindObjectsByType<MeshCollider>(
            FindObjectsSortMode.None
        );

        int count = 0;

        foreach (var col in colliders)
        {
            // Check if "UseFastMidphase" is enabled
            if ((col.cookingOptions & MeshColliderCookingOptions.UseFastMidphase) != 0)
            {
                // Remove the flag
                col.cookingOptions &= ~MeshColliderCookingOptions.UseFastMidphase;

                EditorUtility.SetDirty(col);
                count++;
            }
        }

        Debug.Log($"Fixed {count} MeshColliders (Disabled UseFastMidphase).");
    }
}
