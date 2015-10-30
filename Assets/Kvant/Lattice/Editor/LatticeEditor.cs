//
// Custom editor for Lattice
//
using UnityEngine;
using UnityEditor;

namespace Kvant
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Lattice))]
    public class LatticeEditor : Editor
    {
        SerializedProperty _columns;
        SerializedProperty _rows;
        SerializedProperty _extent;
        SerializedProperty _offset;

        SerializedProperty _noiseFrequency;
        SerializedProperty _noiseDepth;
        SerializedProperty _noiseClampMin;
        SerializedProperty _noiseClampMax;
        SerializedProperty _noiseElevation;
        SerializedProperty _noiseWarp;
        SerializedProperty _noiseOffset;

        SerializedProperty _material;
        SerializedProperty _castShadows;
        SerializedProperty _receiveShadows;
        SerializedProperty _lineColor;

        SerializedProperty _debug;

        static GUIContent _textFrequency = new GUIContent("Frequency");
        static GUIContent _textDepth     = new GUIContent("Depth");
        static GUIContent _textClamp     = new GUIContent("Clamp");
        static GUIContent _textElevation = new GUIContent("Elevation");
        static GUIContent _textOffset    = new GUIContent("Offset");
        static GUIContent _textWarp      = new GUIContent("Warp");

        void OnEnable()
        {
            _columns = serializedObject.FindProperty("_columns");
            _rows    = serializedObject.FindProperty("_rows");
            _extent  = serializedObject.FindProperty("_extent");
            _offset  = serializedObject.FindProperty("_offset");

            _noiseFrequency = serializedObject.FindProperty("_noiseFrequency");
            _noiseDepth     = serializedObject.FindProperty("_noiseDepth");
            _noiseClampMin  = serializedObject.FindProperty("_noiseClampMin");
            _noiseClampMax  = serializedObject.FindProperty("_noiseClampMax");
            _noiseElevation = serializedObject.FindProperty("_noiseElevation");
            _noiseWarp      = serializedObject.FindProperty("_noiseWarp");
            _noiseOffset    = serializedObject.FindProperty("_noiseOffset");

            _material       = serializedObject.FindProperty("_material");
            _castShadows    = serializedObject.FindProperty("_castShadows");
            _receiveShadows = serializedObject.FindProperty("_receiveShadows");
            _lineColor      = serializedObject.FindProperty("_lineColor");

            _debug = serializedObject.FindProperty("_debug");
        }

        public override void OnInspectorGUI()
        {
            var targetLattice = target as Lattice;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_columns);
            EditorGUILayout.PropertyField(_rows);

            if (!_rows.hasMultipleDifferentValues) {
                var note = "Allocated: " + targetLattice.rows;
                EditorGUILayout.LabelField(" ", note, EditorStyles.miniLabel);
            }

            if (EditorGUI.EndChangeCheck())
                targetLattice.NotifyConfigChange();

            EditorGUILayout.PropertyField(_extent);
            EditorGUILayout.PropertyField(_offset);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Fractal Noise", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_noiseFrequency, _textFrequency);
            EditorGUILayout.PropertyField(_noiseDepth, _textDepth);
            MinMaxSlider(_textClamp, _noiseClampMin, _noiseClampMax, -1.0f, 1.0f);
            EditorGUILayout.PropertyField(_noiseElevation, _textElevation);
            EditorGUILayout.PropertyField(_noiseWarp, _textWarp);
            EditorGUILayout.PropertyField(_noiseOffset, _textOffset);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_material);
            EditorGUILayout.PropertyField(_castShadows);
            EditorGUILayout.PropertyField(_receiveShadows);
            EditorGUILayout.PropertyField(_lineColor);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_debug);

            serializedObject.ApplyModifiedProperties();
        }

        void MinMaxSlider(
            GUIContent label,
            SerializedProperty propMin, SerializedProperty propMax,
            float minLimit, float maxLimit)
        {
            var min = propMin.floatValue;
            var max = propMax.floatValue;

            EditorGUI.BeginChangeCheck();

            // Min-max slider.
            EditorGUILayout.MinMaxSlider(label, ref min, ref max, minLimit, maxLimit);

            var prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Float value boxes.
            var rect = EditorGUILayout.GetControlRect();
            rect.x += EditorGUIUtility.labelWidth;
            rect.width = (rect.width - EditorGUIUtility.labelWidth) / 2 - 2;

            if (EditorGUIUtility.wideMode)
            {
                EditorGUIUtility.labelWidth = 28;
                min = Mathf.Clamp(EditorGUI.FloatField(rect, "min", min), minLimit, max);
                rect.x += rect.width + 4;
                max = Mathf.Clamp(EditorGUI.FloatField(rect, "max", max), min, maxLimit);
                EditorGUIUtility.labelWidth = 0;
            }
            else
            {
                min = Mathf.Clamp(EditorGUI.FloatField(rect, min), minLimit, max);
                rect.x += rect.width + 4;
                max = Mathf.Clamp(EditorGUI.FloatField(rect, max), min, maxLimit);
            }

            EditorGUI.indentLevel = prevIndent;

            if (EditorGUI.EndChangeCheck()) {
                propMin.floatValue = min;
                propMax.floatValue = max;
            }
        }
    }
}
