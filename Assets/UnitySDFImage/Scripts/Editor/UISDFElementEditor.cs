namespace AillieoUtils.UI.SDFImage.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(UISDFElement))]
    [CanEditMultipleObjects]
    internal class UISDFElementEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty shape = serializedObject.FindProperty("shape");
            EditorGUILayout.PropertyField(shape);
            if (shape.intValue == (int)UISDFElement.Shape.RegularPolygon)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("n"), new GUIContent("n"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("startAngle"));
                EditorGUI.indentLevel--;
            }

            SerializedProperty operation = serializedObject.FindProperty("operation");
            EditorGUILayout.PropertyField(operation);
            if(operation.intValue == (int)UISDFElement.SDFOperation.ShapeBlending)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("blendFactor"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
