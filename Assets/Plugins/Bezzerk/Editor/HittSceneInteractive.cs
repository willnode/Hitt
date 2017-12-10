using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class HittSceneInteractive : ScriptableObject
{

    // avoid GC
    static public HittSceneInteractive singleton;

    public Hitt root;
    public HittItem active;
    public int activeEntrance;
    public HittTemplate template;

    static HittSceneInteractive()
    {
        EditorApplication.delayCall += delegate ()
        {
            singleton = Resources.FindObjectsOfTypeAll<HittSceneInteractive>().FirstOrDefault();
            if (!singleton)
            {
                singleton = CreateInstance<HittSceneInteractive>();
                singleton.hideFlags = HideFlags.DontSave;
                Debug.Log("Create new");
            }
            else
                Debug.Log("restore");

            SceneView.onSceneGUIDelegate += singleton.OnSceneGUI;
            Selection.selectionChanged += singleton.SelectionChanged;
        };
    }

    void OnEnable()
    {
        if (!singleton)
            singleton = this;
        else if (singleton != this)
            Debug.LogWarning("SHIT");
    }

    void SelectionChanged()
    {
        var sel = Selection.activeGameObject;
        if (sel && PrefabUtility.GetPrefabType(sel) != PrefabType.Prefab && sel.GetComponent<HittItem>())
        {
            active = sel.GetComponent<HittItem>();
            root = active.root;

            if (root)
            {
                root.Synchronize();
                if (!template)
                    template = root.template;
            }
            if (template)
                template.PopulateIndex();

            activeEntrance = Mathf.Clamp(activeEntrance, 0, active.entrances.Length);
        }
        else
            root = null;
    }

    void OnSceneGUI(SceneView view)
    {
        if (active)
        {
            if (root)
            {
                if (template && Event.current.type == EventType.KeyDown)
                    InteractivePlayground(Event.current.keyCode);
            }
            else
            {
                InteractiveTemplate(view, Event.current);
            }

        }
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
                    active.AssignHitt(activeEntrance, Selection.activeGameObject = (GameObject)PrefabUtility.InstantiatePrefab(g));
                    break;
                }
                return;
        }
        Event.current.Use();
    }


    void Delete()
    {

        if (active.parent)
        {
            var p = active.parent;
            p.AssignHitt(active.parentEntrance, null);
            Selection.activeGameObject = p.gameObject;
        }

    }

    void MoveDown()
    {
        if (!active.parent) { MoveRight(); return; }

        activeEntrance++;

        if (activeEntrance >= active.entrances.Length)
        {
            HittItem g = active;

            do
            {
                if (!g.parent) break;
                var idx = g.parentEntrance + 1;
                if (idx >= g.parent.children.Length)
                {
                    g = g.parent;
                    idx = 0;
                }
                g = g.parent.children[idx];
            } while (g);

            activeEntrance = 0;
            Selection.activeGameObject = g.gameObject;

        }

    }

    void MoveUp()
    {
        if (!active.parent) { return; }

        activeEntrance--;

        if (activeEntrance < 0)
        {
            HittItem g = active;

            do
            {
                if (!g.parent) break;
                var idx = g.parentEntrance - 1;
                if (idx < 0)
                {
                    g = g.parent;
                    idx = g.parent.children.Length - 1;
                }
                g = g.parent.children[idx];
            } while (g);

            activeEntrance = g.entrances.Length - 1;
            Selection.activeGameObject = g.gameObject;

        }
    }

    void MoveRight()
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

    [NonSerialized]
    EntranceTemplate draggedTemplate;
    [NonSerialized]
    Vector3 draggedTemplatePos, draggedTemplateCenter, draggedTemplateNormal;

    Ray mouseRay(Event ev, SceneView view)
    {
        var m = Event.current.mousePosition;
        m.y = -m.y + view.position.height - 16;
        return view.camera.ScreenPointToRay(m);
    }

    Vector3 align(Vector3 v)
    {
        float x = Math.Abs(v.x), y = Math.Abs(v.y), z = Math.Abs(v.z);
        if (x > y && x > z)
            return Vector3.right * v.x;
        else
            return y > z ? Vector3.up * v.y : Vector3.forward * v.z;
    }

    Vector3 snap(Vector3 v)
    {
        const float snap = 0.5f; const float invsnap = 1 / snap;
        return new Vector3(Mathf.Round(v.x * invsnap) * snap, Mathf.Round(v.y * invsnap) * snap, Mathf.Round(v.z * invsnap) * snap);
    }

    void InteractiveTemplate(SceneView view, Event ev)
    {
        if (!active) return;

        if (ev.type == EventType.DragUpdated || ev.type == EventType.DragPerform)
        {
            var data = DragAndDrop.GetGenericData("MittEntrance") as EntranceTemplate;
            if (data != null)
            {
                draggedTemplate = data;
                if (ev.type == EventType.DragUpdated)
                {
                    float d; RaycastHit hit;

                    Vector3 center = active.transform.TransformPoint(active.center);
                    Vector3 normal = -view.camera.transform.forward;
                    if (!ev.shift)
                        normal = align(normal);

                    Plane plane = new Plane(normal, center);
                    Ray ray = mouseRay(ev, view);
                    plane.Raycast(ray, out d);
                    Vector3 point = ray.GetPoint(d);

                    if (!ev.shift)
                        point = snap(point - center) + center;
                    if (!(ev.control || ev.command) && Physics.Linecast(point, center, out hit))
                        point = hit.point;

                    draggedTemplatePos = point;
                    draggedTemplateCenter = center;
                    draggedTemplateNormal = normal;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    ev.Use();
                }
                else
                {
                    {
                        Undo.RecordObject(active, "Add new Entrance");
                        ArrayUtility.Add(ref active.entrances, new Entrance()
                        {
                            position = active.transform.InverseTransformPoint(draggedTemplatePos),
                            rotation = Quaternion.LookRotation(draggedTemplatePos - draggedTemplateCenter),
                            tag = draggedTemplate.index
                        });
                    }
                    draggedTemplate = null;
                    DragAndDrop.activeControlID = 0;
                    DragAndDrop.AcceptDrag();
                    ev.Use();
                }
            }
        }
        else if (ev.type == EventType.DragExited)
            draggedTemplate = null;

        if (ev.type == EventType.Repaint)
        {
            DrawItemHierarchy(active, root);

            if (draggedTemplate != null)
            {
                var r = Mathf.Min(Vector3.Magnitude(draggedTemplatePos - draggedTemplateCenter) * 0.5f, 1);
                var c = draggedTemplate.color; Handles.color = c;
                Handles.DrawLine(draggedTemplateCenter, draggedTemplatePos);
                c.a *= 0.5f; Handles.color = c;
                Handles.DrawWireDisc(draggedTemplateCenter, draggedTemplateNormal, r);
                c.a *= 0.2f; Handles.color = c;
                Handles.DrawSolidDisc(draggedTemplateCenter, draggedTemplateNormal, r);
            }
        }
    }

    void DrawItemHierarchy(HittItem item, bool recursive)
    {
        // the center
        if (!item) return;

        var children = item.children;
        var entrances = item.entrances;
        var center = item.center;
        var count = Math.Min(children.Length, entrances.Length);

        Handles.matrix = item.transform.localToWorldMatrix;
        Handles.color = Color.white;
        Handles.DrawWireCube(center, Vector3.one * 0.1f);

        // the line
        if (template)
            for (int i = 0; i < entrances.Length; i++)
                template.DrawEntrance(entrances[i], item, i == 0);

        // the childs recursive
        if (recursive)
            for (int i = 0; i < count; i++)
                DrawItemHierarchy(children[i], true);

    }
}
