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
    public HittTemplate template;

    [NonSerialized]
    public HittItem active;
    [NonSerialized]
    public GameObject activeObject;

    public int activeEntrance;

    static HittSceneInteractive()
    {
        EditorApplication.delayCall += delegate ()
        {
            singleton = Resources.FindObjectsOfTypeAll<HittSceneInteractive>().FirstOrDefault();

            if (!singleton)
            {
                // this will create once per editor lifetime
                singleton = CreateInstance<HittSceneInteractive>();
                singleton.hideFlags = HideFlags.DontSave;
            }

            SceneView.onSceneGUIDelegate += singleton.OnSceneGUI;
            Selection.selectionChanged += singleton.SelectionChanged;
            Hitt.updateFeed += singleton.SelectionChanged;
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
        activeObject = Selection.activeGameObject;

        if (activeObject && template && PrefabUtility.GetPrefabType(activeObject) != PrefabType.Prefab)
        {
            active = template.GetItemOf(activeObject);

            root = activeObject.GetComponentInParent<Hitt>();

            if (active != null)
                activeEntrance = Mathf.Clamp(activeEntrance, 0, active.entrances.Length);
        }
        else
        {
            active = null;

            root = null;
        }
    }

    void OnSceneGUI(SceneView view)
    {
        if (active != null)
        {
            if (root)
            {
                if (activeObject && template && Event.current.type == EventType.KeyDown)
                    InteractivePlayground(view, Event.current.keyCode);
            }
            //            else
            {
                InteractiveTemplate(view, Event.current);
            }

            if (Event.current.type == EventType.Repaint)
                DrawItemHierarchy(activeObject.transform, root);
        }
    }



    void InteractivePlayground(SceneView view, KeyCode key)
    {
        switch (key)
        {
            case KeyCode.UpArrow: MoveUp(); break;
            case KeyCode.DownArrow: MoveDown(); break;
            case KeyCode.RightArrow: MoveRight(); break;
            case KeyCode.LeftArrow: MoveLeft(); break;
            case KeyCode.Delete: Delete(); break;
            default:

                var obj = template.keyIndex.GetValue(key);

                if (obj != null && obj.prefab)
                {
                    template.SetChildren(activeEntrance, activeObject.transform, Selection.activeGameObject = obj.Instantiate());
                    break;
                }
                return;
        }
        Event.current.Use();
        if (Selection.activeTransform)
            view.LookAt(Selection.activeTransform.position);
    }


    void Delete()
    {
        var p = activeObject.transform.parent;
        if (p && template.GetItemOf(p) != null)
        {

            template.SetChildren(template.GetChildren(p).IndexOf(activeObject.transform), p, null);// activeObject.transform.GetSiblingIndex(), p, null);
            Selection.activeGameObject = p.gameObject;
        }

    }

    void MoveDown()
    {
        if (root.gameObject == activeObject) { MoveRight(); return; }

        activeEntrance++;

        if (activeEntrance >= active.entrances.Length)
        {

            activeEntrance = 0;

        }

    }

    void MoveUp()
    {
        if (root.gameObject == activeObject) { return; }

        activeEntrance--;

        if (activeEntrance < 0)
        {

            activeEntrance = active.entrances.Length - 1;
        }
    }

    void MoveRight()
    {

        var p = template.GetChildren(activeObject.transform).ToArray();

        Selection.activeGameObject = p[HittUtility.Clamp(p, activeEntrance)].gameObject;
    }

    void MoveLeft()
    {
        if (activeObject.transform.parent)
        {
            Selection.activeGameObject = activeObject.transform.parent.gameObject;
        }

    }

    //---------------------------------------------------------------------------------//

    [NonSerialized]
    GateTags draggedTemplate;
    [NonSerialized]
    Vector3 draggedTemplatePos, draggedTemplateCenter, draggedTemplateNormal;


    void InteractiveTemplate(SceneView view, Event ev)
    {
        if (active == null) return;

        if (ev.type == EventType.DragUpdated || ev.type == EventType.DragPerform)
        {
            var data = DragAndDrop.GetGenericData("MittEntrance") as GateTags;
            if (data != null)
            {
                draggedTemplate = data;
                if (ev.type == EventType.DragUpdated)
                {
                    float d; RaycastHit hit;

                    Vector3 center = activeObject.transform.TransformPoint(active.center);
                    Vector3 normal = -view.camera.transform.forward;
                    if (!ev.shift)
                        normal = HittUtility.Align(normal);

                    Plane plane = new Plane(normal, center);
                    Ray ray = HittUtility.MouseRay(ev, view);
                    plane.Raycast(ray, out d);
                    Vector3 point = ray.GetPoint(d);

                    if (!ev.shift)
                        point = HittUtility.Snap(point - center, HandleUtility.GetHandleSize(center) * 0.2f) + center;
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
                        Undo.RecordObject(activeObject, "Add new Entrance");
                        active.AddEntrance(new Entrance()
                        {
                            position = activeObject.transform.InverseTransformPoint(draggedTemplatePos),
                            rotation = HittUtility.Conjugate( activeObject.transform.rotation) * Quaternion.LookRotation(draggedTemplatePos - draggedTemplateCenter),
                            tag = draggedTemplate.hash
                        });
                        template.Populate();
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

    void DrawItemHierarchy(Transform obj, bool recursive)
    {
        // the center
        if (obj == null) return;
        var item = template.GetItemOf(obj.gameObject);
        if (item == null) return;

        Handles.matrix = obj.transform.localToWorldMatrix;
        Handles.color = Color.white;
        Handles.DrawWireCube(item.center, Vector3.one * 0.1f);

        // the line
        if (template)
        {
            if (!recursive && item.port.tag != 0)
                template.DrawEntrance(item.port, item, true);

            for (int i = 0; i < item.entrances.Length; i++)
                template.DrawEntrance(item.entrances[i], item, recursive && activeObject == obj.gameObject && i == activeEntrance);
        }

        // the childs recursive
        if (recursive)
            for (int i = 0; i < obj.childCount; i++)
                DrawItemHierarchy(obj.GetChild(i), true);

    }
}
