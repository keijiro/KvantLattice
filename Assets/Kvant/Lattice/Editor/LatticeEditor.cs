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
        SerializedProperty _columns;
        SerializedProperty _rows;
        SerializedProperty _extent;

        SerializedProperty _noiseOffset;
        SerializedProperty _noiseFrequency;
        SerializedProperty _noiseDepth;
        SerializedProperty _noiseClampMin;
        SerializedProperty _noiseClampMax;
        SerializedProperty _noiseElevation;
        SerializedProperty _noiseWarp;

        SerializedProperty _surfaceColor;
        SerializedProperty _lineColor;
        SerializedProperty _metallic;
        SerializedProperty _smoothness;
        SerializedProperty _castShadows;
        SerializedProperty _receiveShadows;

        SerializedProperty _albedoMap;
        SerializedProperty _normalMap;
        SerializedProperty _occlusionMap;
        SerializedProperty _occlusionStrength;
        SerializedProperty _mapScale;

        SerializedProperty _debug;

        static GUIContent _textOffset    = new GUIContent("Offset");
        static GUIContent _textFrequency = new GUIContent("Frequency");
        static GUIContent _textDepth     = new GUIContent("Depth");
        static GUIContent _textClamp     = new GUIContent("Clamp");
        static GUIContent _textElevation = new GUIContent("Elevation");
        static GUIContent _textWarp      = new GUIContent("Warp");
        static GUIContent _textAlbedo    = new GUIContent("Albedo");
        static GUIContent _textNormal    = new GUIContent("Normal");
        static GUIContent _textOcclusion = new GUIContent("Occlusion");
        static GUIContent _textScale     = new GUIContent("Scale");
        static GUIContent _textEmpty     = new GUIContent(" ");

        void OnEnable()
        {
            _columns = serializedObject.FindProperty("_columns");
            _rows    = serializedObject.FindProperty("_rows");
            _extent  = serializedObject.FindProperty("_extent");

            _noiseOffset    = serializedObject.FindProperty("_noiseOffset");
            _noiseFrequency = serializedObject.FindProperty("_noiseFrequency");
            _noiseDepth     = serializedObject.FindProperty("_noiseDepth");
            _noiseClampMin  = serializedObject.FindProperty("_noiseClampMin");
            _noiseClampMax  = serializedObject.FindProperty("_noiseClampMax");
            _noiseElevation = serializedObject.FindProperty("_noiseElevation");
            _noiseWarp      = serializedObject.FindProperty("_noiseWarp");

            _surfaceColor   = serializedObject.FindProperty("_surfaceColor");
            _lineColor      = serializedObject.FindProperty("_lineColor");
            _metallic       = serializedObject.FindProperty("_metallic");
            _smoothness     = serializedObject.FindProperty("_smoothness");
            _castShadows    = serializedObject.FindProperty("_castShadows");
            _receiveShadows = serializedObject.FindProperty("_receiveShadows");

            _albedoMap         = serializedObject.FindProperty("_albedoMap");
            _normalMap         = serializedObject.FindProperty("_normalMap");
            _occlusionMap      = serializedObject.FindProperty("_occlusionMap");
            _occlusionStrength = serializedObject.FindProperty("_occlusionStrength");
            _mapScale          = serializedObject.FindProperty("_mapScale");

            _debug = serializedObject.FindProperty("_debug");
        }

        public override void OnInspectorGUI()
        {
            var targetLattice = target as Lattice;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_columns);
            EditorGUILayout.PropertyField(_rows);

            if (!_rows.hasMultipleDifferentValues)
                EditorGUILayout.LabelField(" ", "Allocated: " + targetLattice.rows, EditorStyles.miniLabel);

            if (EditorGUI.EndChangeCheck())
                targetLattice.NotifyConfigChange();

            EditorGUILayout.PropertyField(_extent);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Fractal Noise", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_noiseOffset, _textOffset);
            EditorGUILayout.PropertyField(_noiseFrequency, _textFrequency);
            EditorGUILayout.PropertyField(_noiseDepth, _textDepth);
            MinMaxSlider(_textClamp, _noiseClampMin, _noiseClampMax, -1.5f, 1.5f);
            EditorGUILayout.PropertyField(_noiseElevation, _textElevation);
            EditorGUILayout.PropertyField(_noiseWarp, _textWarp);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_surfaceColor);
            EditorGUILayout.PropertyField(_lineColor);
            EditorGUILayout.PropertyField(_metallic);
            EditorGUILayout.PropertyField(_smoothness);
            EditorGUILayout.PropertyField(_castShadows);
            EditorGUILayout.PropertyField(_receiveShadows);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Triplanar Mapping", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_albedoMap, _textAlbedo);
            EditorGUILayout.PropertyField(_normalMap, _textNormal);
            EditorGUILayout.PropertyField(_occlusionMap, _textOcclusion);
            if (_occlusionMap.hasMultipleDifferentValues || _occlusionMap.objectReferenceValue)
                EditorGUILayout.PropertyField(_occlusionStrength, _textEmpty);
            EditorGUILayout.PropertyField(_mapScale, _textScale);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_debug);

            serializedObject.ApplyModifiedProperties();
        }

        void MinMaxSlider(GUIContent label, SerializedProperty propMin, SerializedProperty propMax, float minLimit, float maxLimit)
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
