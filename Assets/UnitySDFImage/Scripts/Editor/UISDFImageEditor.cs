namespace AillieoUtils.UI.SDFImage.Editor
{
    using UnityEditor;
    using UnityEditor.UI;
    using UnityEngine;

    [CustomEditor(typeof(UISDFImage))]
    [CanEditMultipleObjects]
    internal class UISDFImageEditor : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("textureValue"), new GUIContent("Texture"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("blendRadiusValue"), new GUIContent("Blend Radius"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("softnessValue"), new GUIContent("Softness"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
