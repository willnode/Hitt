using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HittItem : MonoBehaviour
{
    /// <summary>
    /// Keycode to insert when Interactive Playground is used (prefab only)
    /// </summary>
    public KeyCode key = KeyCode.None;

    /// <summary>
    /// Center offset (configurable on prefab only)
    /// </summary>
    public Vector3 center = Vector3.zero;

    /// <summary>
    /// List of entrances (configurable on prefab only)
    /// </summary>
    public Entrance[] entrances = new Entrance[1];

    /// <summary>
    /// List of children for each entrance (configurable on scene game object only)
    /// </summary>
    public HittItem[] children = new HittItem[0];

    /// <summary>
    /// Parent of this item (auto-configured. Do not set)
    /// </summary>
    public HittItem parent;

    public int parentEntrance { get { return parent ? Array.IndexOf(parent.children, this) : -1; } }

    public void SetModelVisible(bool visible)
    {
        if (GetComponent<MeshRenderer>())
            GetComponent<MeshRenderer>().enabled = false;

        foreach (var item in children)
        {
            if (item != null)
                item.SetModelVisible(visible);
        }
    }

    [NonSerialized]
    Hitt _root;

    public Hitt root { get { return _root ? _root : (_root = GetComponent<Hitt>() ?? (parent ? parent.root : null)); } }

    static Quaternion Conjugate(Quaternion t)
    {
        // faster than inverse (ignoring normalization)
        return new Quaternion(-t.x, -t.y, -t.z, t.w);
    }

    public void Synchronize()
    {
        var p = parentEntrance;
        if (p >= 0)
        {
            var e = parent.entrances[parentEntrance];
            transform.SetPositionAndRotation(parent.transform.TransformPoint(center + e.position)
              , parent.transform.rotation * e.rotation);
        }

        var count = Math.Min(children.Length, entrances.Length);
        for (int i = 0; i < count; i++)
        {
            if (children[i])
            {
                children[i].parent = this;
                children[i].Synchronize();
            }
        }

    }

    public virtual bool IsReplaceableWith(HittItem other, out Quaternion rotation)
    {
        rotation = Quaternion.identity;

        if (this == other) { rotation = transform.rotation; return true; }
        if (entrances.Length != other.entrances.Length) { return false; }

        var count = entrances.Length;
        for (int i = 0; i < count; i++)
        {
            var e = entrances[i];
            var r = true;
            var q = e.rotation * Conjugate(other.entrances[0].rotation);

            // find matching parts
            for (int x = 0; x < count; x++)
            {
                var r2 = false;
                for (int y = 0; y < count; y++)
                {
                    if (Quaternion.Angle(q * other.entrances[y].rotation, e.rotation) < 1)
                    {
                        r2 = true;
                        break;
                    }
                }
                if (!r2)
                {
                    r = false;
                    break;
                }
            }

            if (r)
            {
                rotation = q;
                return true;
            }
        }

        return false;
    }

    public void AssignHitt(int entrance, GameObject g)
    {

        // validate
        if (entrance >= entrances.Length) return;
        if (entrance >= children.Length)
            Array.Resize(ref children, entrances.Length);

        // delete old
        if (children[entrance])
        {
#if UNITY_EDITOR
            UnityEditor.Undo.DestroyObjectImmediate(children[entrance].gameObject);
#else
            DestroyImmediate(children[entrance].gameObject);
#endif
        }

        children[entrance] = null;

        // assign to new one
        if (g)
        {
            var bz = g.GetComponent<HittItem>();
            if (bz)
            {
                children[entrance] = bz;
                bz.parent = this;
                bz.Synchronize();
            }
        }
    }
}

[Serializable]
public class Entrance
{

    public Vector3 position;
    public Quaternion rotation;
    public int tag;

    public Vector3 forward { get { return rotation * Vector3.forward; } }

}