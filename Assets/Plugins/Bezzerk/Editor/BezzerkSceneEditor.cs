using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class BezzerkSceneEditor
{

    static BezzerkSceneEditor()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
        Selection.selectionChanged += SelectionChanged;
    }


    static void SelectionChanged()
    {

    }

    static void OnSceneGUI(SceneView view)
    {
        //var ev = Event.current;
        //if (ev.type == EventType.DragUpdated)
        //{
        //    var sel = Selection.activeGameObject;
        //    if (sel && sel.GetComponent<BezzerkItem>() && PrefabUtility.GetPrefabType(sel) == PrefabType.PrefabInstance)
        //    {
                
        //    }
        //}
    }


}
