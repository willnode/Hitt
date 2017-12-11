//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(HittItem))]
//public class HittItemEditor : Editor
//{

//    public SerializedProperty key, center, entrances, children;
    
//    protected virtual void OnEnable()
//    {
//        key = serializedObject.FindProperty("key");
//        center = serializedObject.FindProperty("center");
//        entrances = serializedObject.FindProperty("entrances");
//        children = serializedObject.FindProperty("children");
//    }

//    public override void OnInspectorGUI()
//    {
//        var type = PrefabUtility.GetPrefabType(target);

//        serializedObject.Update();

//        EditorGUI.BeginChangeCheck();

//        // by design, user should not naively edit entrances individually.
//        EditorGUI.BeginDisabledGroup(type == PrefabType.PrefabInstance);

//        EditorGUILayout.PropertyField(key);
//        EditorGUILayout.PropertyField(center);
//        EditorGUILayout.PropertyField(entrances, true);

//        EditorGUI.EndDisabledGroup();
//        EditorGUILayout.PropertyField(children, true);

//        // by design there should never a children in the prefab template
//        if (type == PrefabType.Prefab && children.arraySize > 0)
//            EditorGUILayout.HelpBox("A prefab should never have any Hitt item attached", MessageType.Warning);

//        serializedObject.ApplyModifiedProperties();

//        if (EditorGUI.EndChangeCheck())
//            if (type != PrefabType.Prefab)
//                foreach (HittItem item in targets)
//                    item.Synchronize();


//        if (type == PrefabType.Prefab)
//            // workaround to instantly update all prefab instance (slow)
//            foreach (var item in FindObjectsOfType<Hitt>())
//                item.Synchronize();
//    }
//}
