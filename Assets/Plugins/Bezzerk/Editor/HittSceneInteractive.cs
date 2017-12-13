using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public partial class HittSceneInteractive : ScriptableObject
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
        if (active == null) return;

        if (root && activeObject && template && Event.current.type == EventType.KeyDown)
            InteractivePlayground(view, Event.current.keyCode);

        InteractiveTemplate(view, Event.current);

        if (Event.current.type == EventType.Repaint)
            DrawItemHierarchy(activeObject.transform, root);

    }



    //---------------------------------------------------------------------------------//

    [NonSerialized]
    GateTags dragTemplate;
    [NonSerialized]
    DragStop dragStop;

    struct DragStop
    {
        public Vector3 pos, center, normal; 
    }

    void InteractiveTemplate(SceneView view, Event ev)
    {
        if (active == null) return;

        if (ev.type == EventType.DragUpdated || ev.type == EventType.DragPerform)
        {
            var data = DragAndDrop.GetGenericData("MittEntrance") as GateTags;
            if (data != null)
            {
                dragTemplate = data;
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

                    dragStop = new DragStop() { pos = point, center = center, normal = normal };

                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    ev.Use();
                }
                else
                {
                    {
                        Undo.RecordObject(activeObject, "Add new Entrance");
                        active.AddEntrance(new Entrance()
                        {
                            position = activeObject.transform.InverseTransformPoint(dragStop.pos),
                            rotation = HittUtility.Conjugate(activeObject.transform.rotation) * Quaternion.LookRotation(dragStop.pos - dragStop.center),
                            tag = dragTemplate.hash
                        });
                        template.Populate();
                    }
                    dragTemplate = null;
                    DragAndDrop.activeControlID = 0;
                    DragAndDrop.AcceptDrag();
                    ev.Use();
                }
            }
        }
        else if (ev.type == EventType.DragExited)
            dragTemplate = null;

        if (ev.type == EventType.Repaint)
        {
            if (dragTemplate != null)
            {
                var r = Mathf.Min(Vector3.Magnitude(dragStop.pos - dragStop.center) * 0.5f, 1);
                var c = dragTemplate.color; Handles.color = c;
                Handles.DrawLine(dragStop.center, dragStop.pos);
                c.a *= 0.5f; Handles.color = c;
                Handles.DrawWireDisc(dragStop.center, dragStop.normal, r);
                c.a *= 0.2f; Handles.color = c;
                Handles.DrawSolidDisc(dragStop.center, dragStop.normal, r);
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

            bool flag = recursive && activeObject == obj.gameObject;
            for (int i = 0; i < item.entrances.Length; i++)
                template.DrawEntrance(item.entrances[i], item, flag && i == activeEntrance);
        }

        // the childs recursive
        if (recursive)
            for (int i = 0; i < obj.childCount; i++)
                DrawItemHierarchy(obj.GetChild(i), true);
    }

}
