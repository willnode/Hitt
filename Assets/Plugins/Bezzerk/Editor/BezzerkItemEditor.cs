using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezzerkItem))]
public class BezzerkItemEditor : Editor
{

    public SerializedProperty key, center, entrances, children;

    //static GUIContent refreshbtn = new GUIContent("Refresh Scene", "Click to invalidate all bezzerk item in scene");

    protected virtual void OnEnable()
    {
        key = serializedObject.FindProperty("key");
        center = serializedObject.FindProperty("center");
        entrances = serializedObject.FindProperty("entrances");
        children = serializedObject.FindProperty("children");
    }

    public override void OnInspectorGUI()
    {
        var type = PrefabUtility.GetPrefabType(target);

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        // by design, user should not naively edit entrances individually.
        EditorGUI.BeginDisabledGroup(type == PrefabType.PrefabInstance);

        EditorGUILayout.PropertyField(key);
        EditorGUILayout.PropertyField(center);
        EditorGUILayout.PropertyField(entrances, true);

        EditorGUI.EndDisabledGroup();
        EditorGUILayout.PropertyField(children, true);

        // by design there should never a children in the prefab template
        if (type == PrefabType.Prefab && children.arraySize > 0)
            EditorGUILayout.HelpBox("A prefab should never have any bezzerk item attached", MessageType.Warning);

        serializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
            if (type != PrefabType.Prefab)
                foreach (BezzerkItem item in targets)
                    item.Synchronize();


        if (type == PrefabType.Prefab)// && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.KeyUp))
            // workaround to instantly update all prefab instance (slow)
            //  if (GUILayout.Button(refreshbtn))
            foreach (var item in FindObjectsOfType<Bezzerk>())
                item.Synchronize();
    }
}
