using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class BezzerkInventory : EditorWindow
{

    static BezzerkInventory singleton;

    Bezzerk root;
    BezzerkItem active;
    int activeEntrance;

    BezzerkTemplate template { get { return root.template; } }


    [MenuItem("Window/Bezzerk Inventory")]
    public static void ShowBezzerkInventory()
    {
        if (!singleton)
            CreateInstance<BezzerkInventory>().Show();
        else
            singleton.Show();
    }

    void OnEnable()
    {
        singleton = this;
        Selection.selectionChanged += SelectionChange;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    void OnDisable()
    {
        Selection.selectionChanged -= SelectionChange;
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }


    void SelectionChange()
    {
        var sel = Selection.activeGameObject;
        if (sel && sel.GetComponent<BezzerkItem>())
        {
            active = sel.GetComponent<BezzerkItem>();
            root = active.root;
            if (template)
                template.PopulateIndex();
            root.Synchronize();
            activeEntrance = Mathf.Clamp(activeEntrance, 0, active.entrances.Length);
        }
        else
            root = null;
    }

    string GetStatusString()
    {
        if (root)
         //   if (focusedWindow == this)
                if (template)
                    return "Click on magic keys to pop new one!";
                else
                    return "Bezzerk doesn't have a template to start!";
            //else
            //    return "Click here and start building!";
        else
            return "Select on any Bezzerk to get started!";
    }

    void OnGUI()
    {
        root = (Bezzerk)EditorGUILayout.ObjectField(root, typeof(Bezzerk), true);

        EditorGUILayout.LabelField(GetStatusString());

    }

    void OnSceneGUI (SceneView view)
    {
        if (root && template && active && Event.current.type == EventType.KeyDown)
            InteractivePlayground(Event.current.keyCode);
    }

    void InteractivePlayground(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.UpArrow: MoveUp(); break;
            case KeyCode.DownArrow: MoveDown(); break;
            case KeyCode.RightArrow: MoveRight(); break;
            case KeyCode.LeftArrow: MoveLeft(); break;
            case KeyCode.Delete: Delete(); break;
            default:
                GameObject g;
                
                if (template.keyIndex.TryGetValue(key, out g))
                {
                    active.AssignBezzerk(activeEntrance, Selection.activeGameObject = (GameObject)PrefabUtility.InstantiatePrefab(g));
                    break;
                }
                return;
        }
        Event.current.Use();
    }

    void Delete ()
    {
        active.AssignBezzerk(activeEntrance, null);

    }

    void MoveDown ()
    {
        if (!active.parent) { MoveRight(); return; }

        BezzerkItem g = active;

        var p = active.parent.children;

        do
        {
            var idx = g.parentEntrance + 1;
            if (idx >= p.Length)
                idx = 0;
            g = p[idx];
        } while (g);       
        
        Selection.activeGameObject = g.gameObject;

    }

    void MoveUp()
    {
        if (!active.parent) { return; }

        BezzerkItem g = active;

        var p = active.parent.children;

        do
        {
            var idx = g.parentEntrance - 1;
            if (idx < 0)
                idx = p.Length - 1;
            g = p[idx];
        } while (g);

        Selection.activeGameObject = g.gameObject;
    }

    void MoveRight ()
    {

        var p = active.children;
        var idx = 0;
        while (idx < p.Length && !p[idx])
        {
            idx++;
        }

        if (idx < p.Length)
            Selection.activeGameObject = p[idx].gameObject;
    }

    void MoveLeft()
    {
        if (active.parent)
        {
            Selection.activeGameObject = active.parent.gameObject;
        }

    }

}

