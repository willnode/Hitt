using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BezzerkItem : MonoBehaviour
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
    public BezzerkItem[] children = new BezzerkItem[0];

    /// <summary>
    /// Parent of this item (auto-configured. Do not set)
    /// </summary>
    public BezzerkItem parent;

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

    protected virtual void DrawWireHierarchy(bool recursive = false)
    {
        // the center
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.white;
        Gizmos.DrawCube(-center, Vector3.one * 0.1f);

        var count = Math.Min(children.Length, entrances.Length);

        // the line
        for (int i = 0; i < entrances.Length; i++)
            Gizmos.DrawLine(entrances[i].position, center);

        // children validity
        for (int i = 0; i < children.Length; i++)
            if (children[i])
                children[i].parent = this;

        // the childs recursive
        if (recursive)
            for (int i = 0; i < count; i++)
                if (children[i])
                    children[i].DrawWireHierarchy(true);


        // the gates
        var enter = parentEntrance;

        if (!root.template) return;

        var eligb = root.template.entrances.Length;

        for (int i = 0; i < count; i++)
        {
            var e = entrances[i];
            if (i != enter && children[i] && e.tag < eligb)
                root.template.entrances[e.tag].DrawWire(e.position, e.rotation);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        DrawWireHierarchy();
    }

    [NonSerialized]
    Bezzerk _root;

    public Bezzerk root { get { return _root ? _root : (_root = GetComponent<Bezzerk>() ?? parent.root); } }

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
                children[i].Synchronize();
        }

    }

    public virtual bool IsReplaceableWith(BezzerkItem other, out Quaternion rotation)
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

    public void AssignBezzerk (int entrance, GameObject g)
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
            var bz = g.GetComponent<BezzerkItem>();
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