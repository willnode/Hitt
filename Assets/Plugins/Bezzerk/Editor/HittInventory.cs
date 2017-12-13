using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HittInventory : EditorWindow
{

    public static class Styles
    {

        public static GUIContent[] tabContent = new GUIContent[]
        {
            new GUIContent("Items", "View and Manage items available in template"),
            new GUIContent("Keymaps", "Manage hotkeys for quick building"),
            new GUIContent("Entrances", "Modify entrance tags"),
        };

        public static GUIStyle[] tabStyles = new GUIStyle[]
        {
            EditorStyles.miniButtonLeft,
            EditorStyles.miniButtonMid,
            EditorStyles.miniButtonRight,
        };

        /// <summary>
        /// 1 = black/white, 2 = normal/bold
        /// </summary>
        public static GUIStyle[] labelStyles = new GUIStyle[]
        {
             new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter },
             new GUIStyle(EditorStyles.whiteLabel) { alignment = TextAnchor.MiddleCenter },

             new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter },
             new GUIStyle(EditorStyles.whiteBoldLabel) { alignment = TextAnchor.MiddleCenter },
        };

        public static GUIStyle labelThumb = new GUIStyle() { imagePosition = ImagePosition.ImageOnly };
        public static GUIStyle labelThumbName = new GUIStyle(EditorStyles.whiteLabel) { alignment = TextAnchor.LowerRight, margin = new RectOffset(), padding = new RectOffset(5, 5, 2, 2) };
        public static GUIStyle labelThumbKey = new GUIStyle(EditorStyles.whiteLabel) { fontStyle = FontStyle.Italic, alignment = TextAnchor.UpperLeft, margin = new RectOffset(), padding = new RectOffset(5, 5, 2, 2) };
        public static GUIStyle labelThumbQuestion = new GUIStyle(EditorStyles.whiteLabel) { fontStyle = FontStyle.Bold, fontSize = 20, alignment = TextAnchor.MiddleCenter, margin = new RectOffset(), padding = new RectOffset(5, 5, 2, 2) };

        public static GUIContent questionNoHitt = new GUIContent("??", "This object seems does not have HittItem attached");

        /// <summary>
        /// Make grid via GUILayout
        /// </summary>
        public static void MakeGrid(Vector2 itemSize, int count, Action<Rect, int> callback)
        {
            //  var r = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            var width = EditorGUIUtility.currentViewWidth - 17 - 200;
            int itemPerRow = (int)Mathf.Max(1f, width / itemSize.x);

            EditorGUILayout.BeginHorizontal(GUILayout.Height(itemSize.y));
            for (int i = 0; i < count; i++)
            {
                callback(EditorGUILayout.GetControlRect(GUILayout.Width(itemSize.x), GUILayout.Height(itemSize.y)), i);

                if ((i + 1) % itemPerRow == 0)
                {
                    // make new row
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal(GUILayout.Height(itemSize.y));
                }
            }
            EditorGUILayout.EndHorizontal();
            //  EditorGUILayout.EndVertical();
        }


        public static bool IsDark(Color c) { return (c.r * 0.299f) + (c.g * 0.587f) + (c.b * 0.114f) < 0.5f; }

    }
    static HittInventory singleton;

    static HittTemplate template { get { return HittSceneInteractive.singleton.template; } set { HittSceneInteractive.singleton.template = value; } }

    static HittItem active { get { return HittSceneInteractive.singleton.active; } }

    static Hitt root { get { return HittSceneInteractive.singleton.root; } }

    static GameObject activeObject { get { return HittSceneInteractive.singleton.activeObject; } }

    static int activeEntrance { get { return HittSceneInteractive.singleton.activeEntrance; } set { HittSceneInteractive.singleton.activeEntrance = value; } }


    [MenuItem("Window/Hitt Inventory")]
    public static void ShowHittInventory()
    {
        if (!singleton)
            CreateInstance<HittInventory>().Show();
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
        if (template)
            activeEntrance = Mathf.Clamp(activeEntrance, 0, template.gates.Length - 1);
        Repaint();
    }

    int activeTab = 0;


    Vector2 scroll;
    void OnGUI()
    {

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.ObjectField(root, typeof(Hitt), true);
        if (!(template = (HittTemplate)EditorGUILayout.ObjectField(template, typeof(HittTemplate), false)))
        {
            if (root)
                template = root.template;
            if (!template)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("Add a template to start designing with inventories", MessageType.Info);
                return;
            }
        }
        EditorGUI.BeginDisabledGroup(active == null);
        if (GUILayout.Button(root ? "Sync" : "Add", EditorStyles.miniButton, GUILayout.Width(100)))
        {
            if (root)
                template.Synchronize(root.transform);
            else
                activeObject.AddComponent<Hitt>();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        //  scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < Styles.tabContent.Length; i++)
            {
                if (GUILayout.Toggle(activeTab == i, Styles.tabContent[i], Styles.tabStyles[i]))
                    activeTab = i;
            }
            EditorGUILayout.EndHorizontal();
        }

        switch (activeTab)
        {
            case 0: ItemsGUI(); break;
            case 1: KeymapsGUI(); break;
            case 2: EntranceGUI(); break;
        }

    }

    void OnSceneGUI(SceneView view)
    {

    }

    void OnTemplateDesignGUI()
    {

    }

    #region Items

    int activeItem = 0;

    void ItemsGUI()
    {
        var ev = Event.current;
        var ms = ev.mousePosition;

        //if (activeItem >= 0)
        //{
        //    var e = template.items[activeItem];
        //    var ei = e ? e.GetComponent<HittItem>() : null;
        //    if (!ei)
        //    {
        //        EditorGUILayout.HelpBox("Selected item does not have HittItem.", MessageType.None);
        //    }
        //    else
        //    {
        //        EditorGUILayout.BeginHorizontal();
        //        EditorGUILayout.LabelField(e.name);
        //        bool exist = Array.IndexOf( template.items.con
        //       EditorGUI.BeginDisabledGroup()
        //        if (GUILayout.Button("+", GUILayout.Width(50)))
        //        {
        //            Undo.RecordObject(template, "Add new Entrance Template");
        //            ArrayUtility.Add(ref template.entrances, e.Clone());
        //        }

        //        if (GUILayout.Button("-", GUILayout.Width(50)) && template.entrances.Length > 1)
        //        {
        //            Undo.RecordObject(template, "Delete Entrance Template");
        //            ArrayUtility.RemoveAt(ref template.entrances, activeEntrance);
        //            activeEntrance--;
        //        }

        //        EditorGUILayout.EndHorizontal();
        //    }
        //}

        EditorGUILayout.BeginHorizontal();

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
        Styles.MakeGrid(new Vector2(100, 100), template.items.Length, delegate (Rect r, int i)
        {

            var g = template.items[i];

            if ((r).Contains(ms + scroll))
            {
                if (ev.type == EventType.DragPerform || ev.type == EventType.DragUpdated)
                {
                    var obj = HittUtility.GetPrefabOf(DragAndDrop.objectReferences.FirstOrDefault() as GameObject);
                    if (obj && !g.prefab && !template.objectIndex.ContainsKey(obj.name))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        if (ev.type == EventType.DragPerform)
                        {
                            g.prefab = obj;
                            template.Populate();
                            DragAndDrop.AcceptDrag();
                        }
                        ev.Use();
                    }
                }
                if (ev.type == EventType.MouseDown)
                {
                    activeItem = i;
                    if (g.prefab)
                    {
                        if (ev.clickCount >= 2)
                            EditorGUIUtility.PingObject(g.prefab);
                        else
                        {
                            DragAndDrop.PrepareStartDrag();
                            DragAndDrop.objectReferences = new UnityEngine.Object[] { g.prefab };
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                            DragAndDrop.StartDrag("Prefab Out");
                        }
                    }
                }
            }

            if (ev.type == EventType.Repaint)
            {
                if (g.prefab)
                {
                    GUI.Label(r, new GUIContent(AssetPreview.GetAssetPreview(g.prefab), g.name), Styles.labelThumb);

                    if (activeItem == i)
                    {
                        var r2 = r;
                        r2.height = 8;
                        GUI.Box(r2, EditorGUIUtility.whiteTexture);
                    }

                    GUI.Label(r, g.name, Styles.labelThumbName);
                    if (g.key != KeyCode.None)
                        GUI.Label(r, g.key.ToString(), Styles.labelThumbKey);
                    if (g.port.tag == 0)
                        GUI.Label(r, "??", Styles.labelThumbQuestion);
                }
                else
                {
                    GUI.Box(r, Texture2D.whiteTexture);
                    GUI.Label(r, "+", Styles.labelStyles[2]);
                }
            }
        });

        EditorGUILayout.EndScrollView();
        if (GUILayoutUtility.GetLastRect().Contains(ms))
        {
            if (ev.type == EventType.DragPerform || ev.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (ev.type == EventType.DragPerform)
                {
                    foreach (var o in DragAndDrop.objectReferences)
                    {
                        var g = HittUtility.GetPrefabOf(o as GameObject);
                        if (template.GetItemOf(g) == null)
                        {
                            var i = new HittItem();
                            i.AssignPrefab(g);
                            ArrayUtility.Add(ref template.items, i);
                        }
                    }
                    template.Populate();
                }
            }
        }
        EditorGUILayout.BeginVertical(GUILayout.Width(200), GUILayout.ExpandHeight(true));
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
                ArrayUtility.Add(ref template.items, new HittItem());
            if (GUILayout.Button("-")) { }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            if (activeItem >= 0)
            {
                var g = template.items[activeItem];
                g.name = EditorGUILayout.DelayedTextField(g.name);
                g.key = (KeyCode)EditorGUILayout.EnumPopup(g.key);
                if (GUILayout.Button("Reset Gates"))
                {
                    g.port.tag = 0;
                    g.entrances = new Entrance[] { };
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("Crop Gates"))
                {
                    var n = activeObject.GetComponentInChildren<MeshFilter>();
                    if (n && n.sharedMesh)
                    {
                        var b = n.sharedMesh.bounds;
                        g.port.position = b.ClosestPoint(g.port.position);
                        foreach (var e in g.entrances)
                        {
                            e.position = b.ClosestPoint(e.position);
                        }
                        SceneView.RepaintAll();
                    }
                }
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    #endregion Items

    #region Keymaps
    void KeymapsGUI()
    {

    }
    #endregion Keymaps

    #region Entrance

    void EntranceGUI()
    {
        var ev = Event.current;
        var ms = ev.mousePosition;
        GateTags e = null;

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();

        if (template.gates.Length > 0)
        {
            e = template.gates[HittUtility.Clamp(template.gates, activeEntrance)];
            e.name = EditorGUILayout.TextField(e.name);
            e.color = EditorGUILayout.ColorField(e.color);
            e.size = EditorGUILayout.Vector3Field(GUIContent.none, e.size);
        }

        if (GUILayout.Button("+", GUILayout.Width(50)))
        {
            Undo.RecordObject(template, "Add new Entrance Template");
            ArrayUtility.Add(ref template.gates, e == null ? new GateTags() : e.Clone());
        }

        if (GUILayout.Button("-", GUILayout.Width(50)) && template.gates.Length > 1)
        {
            Undo.RecordObject(template, "Delete Entrance Template");
            ArrayUtility.RemoveAt(ref template.gates, activeEntrance);
            activeEntrance--;
        }

        if (EditorGUI.EndChangeCheck())
            SceneView.RepaintAll();
        EditorGUILayout.EndHorizontal();

        Styles.MakeGrid(new Vector2(150, 50), template.gates.Length, delegate (Rect r, int i)
        {
            var g = template.gates[i];
            var c = GUI.color;

            if (ev.type == EventType.Repaint)
            {
                GUI.color = g.color;
                GUI.Box(r, Texture2D.whiteTexture);
                GUI.color = c;
                GUI.Label(r, g.name, Styles.labelStyles[(Styles.IsDark(g.color) ? 1 : 0) + (activeEntrance == i ? 2 : 0)]);
            }

            if (ev.type == EventType.MouseDown && r.Contains(ms))
            {
                activeEntrance = i;
                if (active != null)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new UnityEngine.Object[] { template };
                    DragAndDrop.SetGenericData("MittEntrance", g);
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    DragAndDrop.StartDrag("Make Prefab");
                }
                else
                    Repaint();
            }
        });
    }
    #endregion Entrance

}

