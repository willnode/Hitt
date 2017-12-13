using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class HittSceneInteractive
{


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
        if (p.Length > 0)
            Selection.activeGameObject = p[HittUtility.Clamp(p, activeEntrance)].gameObject;
    }

    void MoveLeft()
    {
        if (activeObject.transform.parent)
        {
            Selection.activeGameObject = activeObject.transform.parent.gameObject;
        }

    }
}
