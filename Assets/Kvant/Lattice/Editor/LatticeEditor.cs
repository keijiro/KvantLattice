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
        SerializedProperty propNoiseDepth;
        SerializedProperty propNoiseElevation;
        SerializedProperty propNoiseClampMin;
        SerializedProperty propNoiseClampMax;
        SerializedProperty propNoiseWarp;
        SerializedProperty propSurfaceColor;
        SerializedProperty propLineColor;
        SerializedProperty propDebug;

        static GUIContent textOffset    = new GUIContent("Offset");
        static GUIContent textFrequency = new GUIContent("Frequency");
        static GUIContent textDepth     = new GUIContent("Fractal Depth");
        static GUIContent textElevation = new GUIContent("Elevation");
        static GUIContent textWarp      = new GUIContent("Warp");

        void OnEnable()
        {
            propColumns        = serializedObject.FindProperty("_columns");
            propRows           = serializedObject.FindProperty("_rows");
            propSize           = serializedObject.FindProperty("_size");
            propNoiseOffset    = serializedObject.FindProperty("_noiseOffset");
            propNoiseFrequency = serializedObject.FindProperty("_noiseFrequency");
            propNoiseDepth     = serializedObject.FindProperty("_noiseDepth");
            propNoiseElevation = serializedObject.FindProperty("_noiseElevation");
            propNoiseClampMin  = serializedObject.FindProperty("_noiseClampMin");
            propNoiseClampMax  = serializedObject.FindProperty("_noiseClampMax");
            propNoiseWarp      = serializedObject.FindProperty("_noiseWarp");
            propSurfaceColor   = serializedObject.FindProperty("_surfaceColor");
            propLineColor      = serializedObject.FindProperty("_lineColor");
            propDebug          = serializedObject.FindProperty("_debug");
        }

        void MinMaxSlider(string label, SerializedProperty propMin, SerializedProperty propMax, float minLimit, float maxLimit, string format)
        {
            var min = propMin.floatValue;
            var max = propMax.floatValue;

            EditorGUI.BeginChangeCheck();

            var text = new GUIContent(label + " (" + min.ToString(format) + "," + max.ToString(format) + ")");
            EditorGUILayout.MinMaxSlider(text, ref min, ref max, minLimit, maxLimit);

            if (EditorGUI.EndChangeCheck()) {
                propMin.floatValue = min;
                propMax.floatValue = max;
            }
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
            EditorGUILayout.PropertyField(propNoiseDepth, textDepth);
            MinMaxSlider("Clamp", propNoiseClampMin, propNoiseClampMax, -1.5f, 1.5f, "0.0");
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
