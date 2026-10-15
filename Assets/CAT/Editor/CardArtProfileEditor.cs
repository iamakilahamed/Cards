using UnityEngine;
using UnityEditor;
using CAT.Data;
using CAT.Runtime;

namespace CAT.Editor
{
    [CustomEditor(typeof(CardArtProfile))]
    public class CardArtProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Start listening for any user interaction in the Inspector
            EditorGUI.BeginChangeCheck();

            // Draw the standard ScriptableObject interface
            DrawDefaultInspector();

            // If the user changed a slider, added a sprite, or edited a value...
            if (EditorGUI.EndChangeCheck())
            {
                // Find the active CardPrefab_3D in the open scene
                CardArtBuilder activeBuilder = FindFirstObjectByType<CardArtBuilder>();

                if (activeBuilder != null)
                {
                    // Pass the newly updated data directly to the physical prefab
                    CardArtProfile profile = (CardArtProfile)target;
                    activeBuilder.BuildArt(profile);

                    // Force the Scene View to visually refresh immediately
                    SceneView.RepaintAll();
                }
            }
        }
    }
}
