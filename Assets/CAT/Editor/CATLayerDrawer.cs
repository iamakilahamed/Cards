using UnityEngine;
using UnityEditor;

namespace CAT.Editor
{
    public static class CATLayerDrawer
    {
        public static void DrawArraySection(string sectionName, SerializedProperty arrayProp, LayerType layerType)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(sectionName, EditorStyles.boldLabel);

            if (arrayProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No layers assigned.", MessageType.None);
            }

            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                SerializedProperty elementProp = arrayProp.GetArrayElementAtIndex(i);
                DrawCardLayerProperty($"Layer {i}", elementProp);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Layer")) arrayProp.arraySize++;
            if (arrayProp.arraySize > 0 && GUILayout.Button("- Remove Layer")) arrayProp.arraySize--;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        public static void DrawSingleLayer(string label, SerializedProperty layerProp)
        {
            DrawCardLayerProperty(label, layerProp);
        }

        private static void DrawCardLayerProperty(string label, SerializedProperty layerProp)
        {
            SerializedProperty artworkProp = layerProp.FindPropertyRelative("Artwork");
            SerializedProperty zOffsetProp = layerProp.FindPropertyRelative("ZOffset");
            SerializedProperty offsetProp = layerProp.FindPropertyRelative("Offset");
            SerializedProperty scaleProp = layerProp.FindPropertyRelative("Scale");
            SerializedProperty resProp = layerProp.FindPropertyRelative("MeshResolution");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(artworkProp, new GUIContent(label));

            if (artworkProp.objectReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(zOffsetProp, new GUIContent("Local Z-Offset (Depth)"));
                EditorGUILayout.PropertyField(offsetProp, new GUIContent("Position Offset (X, Y)"));
                EditorGUILayout.PropertyField(scaleProp, new GUIContent("Scale"));
                EditorGUILayout.PropertyField(resProp, new GUIContent("Mesh Resolution"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }
    }
}