using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static void MakeGrid(Vector2 itemSize, int count, Action<Rect, int> callback)
        {
            var width = EditorGUIUtility.currentViewWidth - 17;
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
        }


        public static bool IsDark(Color c) { return (c.r * 0.299f) + (c.g * 0.587f) + (c.b * 0.114f) < 0.5f; }

    }
    static HittInventory singleton;

    static HittTemplate template { get { return HittSceneInteractive.singleton.template; } set { HittSceneInteractive.singleton.template = value; } }

    static HittItem active { get { return HittSceneInteractive.singleton.active; } }

    static Hitt root { get { return HittSceneInteractive.singleton.root; } }

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
            activeEntrance = Mathf.Clamp(activeEntrance, 0, template.entrances.Length - 1);
        Repaint();
    }

    int activeTab = 0;


    Vector2 scroll;
    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));

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
        EditorGUILayout.EndHorizontal();
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

        EditorGUILayout.EndScrollView();
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

        Styles.MakeGrid(new Vector2(100, 100), template.items.Length, delegate (Rect r, int i)
        {

            var g = template.items[i];
            if (!g)
                return;

            if (r.Contains(ms))
            {
                if (ev.type == EventType.MouseDown)
                {
                    if (ev.clickCount >= 2)
                         Selection.activeObject = g;
                    else
                    {
                        activeItem = i;
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.objectReferences = new UnityEngine.Object[] { g };
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        DragAndDrop.StartDrag("Make Prefab");
                    }
                }
            }

            if (ev.type == EventType.Repaint)
            {
                GUI.Label(r, new GUIContent(AssetPreview.GetAssetPreview(g), g.name), Styles.labelThumb);
                if (activeItem == i)
                    GUI.Label(r, g.name, Styles.labelThumbName);
                var gi = g.GetComponent<HittItem>();
                if (!gi)
                    GUI.Label(r, Styles.questionNoHitt, Styles.labelThumbQuestion);
                else if (gi.key != KeyCode.None)
                    GUI.Label(r, gi.key.ToString(), Styles.labelThumbKey);
            }
        });
    }

    #endregion Items

    #region Keymaps
    void KeymapsGUI()
    {

    }
    #endregion Keymaps

    #region Entrance

    int activeEntrance;

    void EntranceGUI()
    {
        var ev = Event.current;
        var ms = ev.mousePosition;
        EditorGUILayout.Space();

        if (activeEntrance >= 0)
        {
            var e = template.entrances[activeEntrance];
            EditorGUILayout.BeginHorizontal();
            e.name = EditorGUILayout.TextField(e.name);
            EditorGUI.BeginChangeCheck();
            e.color = EditorGUILayout.ColorField(e.color);
            e.size = EditorGUILayout.Vector3Field(GUIContent.none, e.size);

            if (GUILayout.Button("+", GUILayout.Width(50)))
            {
                Undo.RecordObject(template, "Add new Entrance Template");
                ArrayUtility.Add(ref template.entrances, e.Clone());
            }

            if (GUILayout.Button("-", GUILayout.Width(50)) && template.entrances.Length > 1)
            {
                Undo.RecordObject(template, "Delete Entrance Template");
                ArrayUtility.RemoveAt(ref template.entrances, activeEntrance);
                activeEntrance--;
            }

            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
            EditorGUILayout.EndHorizontal();
        }

        Styles.MakeGrid(new Vector2(150, 50), template.entrances.Length, delegate (Rect r, int i)
        {
            var g = template.entrances[i];
            var c = GUI.color;
            GUI.color = g.color;
            GUI.Box(r, Texture2D.whiteTexture);
            GUI.color = c;
            GUI.Label(r, g.name, Styles.labelStyles[(Styles.IsDark(g.color) ? 1 : 0) + (activeEntrance == i ? 2 : 0)]);


            if (active && r.Contains(ms))
            {
                if (ev.type == EventType.MouseDown)
                {
                    activeEntrance = i;
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new UnityEngine.Object[] { template };
                    DragAndDrop.SetGenericData("MittEntrance", g);
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    DragAndDrop.StartDrag("Make Prefab");
                }
            }
        });
    }
    #endregion Entrance

}

