//
// Custom editor for LatticeNoiseScroller
//
using UnityEngine;
using UnityEditor;

namespace Kvant
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LatticeNoiseScroller))]
    public class LatticeNoiseScrollerEditor : Editor
    {
        SerializedProperty _yawAngle;
        SerializedProperty _speed;

        void OnEnable()
        {
            _yawAngle = serializedObject.FindProperty("_yawAngle");
            _speed = serializedObject.FindProperty("_speed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_yawAngle);
            EditorGUILayout.PropertyField(_speed);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
