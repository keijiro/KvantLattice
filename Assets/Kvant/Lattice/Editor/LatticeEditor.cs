//
// Custom editor for Lattice
//
using UnityEngine;
using UnityEditor;

namespace Kvant
{
    [CustomEditor(typeof(Lattice)), CanEditMultipleObjects]
    public class LatticeEditor : Editor
    {
        SerializedProperty propColumns;
        SerializedProperty propRows;
        SerializedProperty propSize;
        SerializedProperty propNoiseOffset;
        SerializedProperty propNoiseFrequency;
        SerializedProperty propNoiseElevation;
        SerializedProperty propNoiseWarp;
        SerializedProperty propSurfaceColor;
        SerializedProperty propLineColor;
        SerializedProperty propDebug;

        static GUIContent textOffset    = new GUIContent("Offset");
        static GUIContent textFrequency = new GUIContent("Frequency");
        static GUIContent textElevation = new GUIContent("Elevation");
        static GUIContent textWarp      = new GUIContent("Warp");

        void OnEnable()
        {
            propColumns        = serializedObject.FindProperty("_columns");
            propRows           = serializedObject.FindProperty("_rows");
            propSize           = serializedObject.FindProperty("_size");
            propNoiseOffset    = serializedObject.FindProperty("_noiseOffset");
            propNoiseFrequency = serializedObject.FindProperty("_noiseFrequency");
            propNoiseElevation = serializedObject.FindProperty("_noiseElevation");
            propNoiseWarp      = serializedObject.FindProperty("_noiseWarp");
            propSurfaceColor   = serializedObject.FindProperty("_surfaceColor");
            propLineColor      = serializedObject.FindProperty("_lineColor");
            propDebug          = serializedObject.FindProperty("_debug");
        }

        public override void OnInspectorGUI()
        {
            var targetLattice = target as Lattice;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(propColumns);
            EditorGUILayout.PropertyField(propRows);

            if (!propRows.hasMultipleDifferentValues)
                EditorGUILayout.HelpBox("Actual Number: " + targetLattice.rows, MessageType.None);

            if (EditorGUI.EndChangeCheck()) targetLattice.NotifyConfigChange();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(propSize);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Fractal Noise");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(propNoiseOffset, textOffset);
            EditorGUILayout.PropertyField(propNoiseFrequency, textFrequency);
            EditorGUILayout.PropertyField(propNoiseElevation, textElevation);
            EditorGUILayout.PropertyField(propNoiseWarp, textWarp);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(propSurfaceColor);
            EditorGUILayout.PropertyField(propLineColor);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(propDebug);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
