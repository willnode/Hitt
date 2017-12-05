using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Bezzerk))]
public class BezzerkEditor : BezzerkItemEditor {

    SerializedProperty template;

    protected override void OnEnable ()
    {
        template = serializedObject.FindProperty("template");
        base.OnEnable();
       
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(template);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        
        base.OnInspectorGUI();
        
    }
}
