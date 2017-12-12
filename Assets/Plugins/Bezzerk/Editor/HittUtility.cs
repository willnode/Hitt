using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class HittUtility
{
    public static V GetValue<K, V>(this Dictionary<K, V> dic, K val, V def = default(V))
    {
        return dic.ContainsKey(val) ? dic[val] : def;
    }

    public static Ray MouseRay(Event ev, SceneView view)
    {
        var m = Event.current.mousePosition;
        m.y = -m.y + view.position.height - 16;
        return view.camera.ScreenPointToRay(m);
    }

    public static Vector3 Align(Vector3 v)
    {
        float x = Math.Abs(v.x), y = Math.Abs(v.y), z = Math.Abs(v.z);
        if (x > y && x > z)
            return Vector3.right * v.x;
        else
            return y > z ? Vector3.up * v.y : Vector3.forward * v.z;
    }

    public static Vector3 Snap(Vector3 v, float sz)
    {
        float invsnap = 1 / sz;
        return new Vector3(Mathf.Round(v.x * invsnap) * sz, 
            Mathf.Round(v.y * invsnap) * sz, Mathf.Round(v.z * invsnap) * sz);
    }

    public static int Clamp<T>(T[] array, int v)
    {
        return Mathf.Clamp(v, 0, array.Length - 1);
    }

    public static Quaternion Conjugate (Quaternion q)
    {
        // faster inversion
        return new Quaternion(-q.x, -q.y, -q.z, q.w);
    }

    

    public static GameObject GetPrefabOf(GameObject g)
    {
        if (!g) return null;
        switch (PrefabUtility.GetPrefabType(g))
        {
            case PrefabType.Prefab:
            case PrefabType.ModelPrefab:
                return g;
            case PrefabType.PrefabInstance:
            case PrefabType.ModelPrefabInstance:
                return PrefabUtility.GetPrefabParent(g) as GameObject;
            default:
                return null;
        }
    }

    public static HittItem GetItemOf(this HittTemplate template, GameObject g)
    {
        g = GetPrefabOf(g);
        return g ? template.objectIndex.GetValue(g.name) : null;
    }

    public static HittItem GetItemOf(this HittTemplate template, Transform g)
    {
        return GetItemOf(template, g.gameObject);
    }

    public static int IndexOf<T>(this IEnumerable<T> source, T val)
    {
        int i = 0;
        var comp = EqualityComparer<T>.Default;
        foreach (T item in source)
        {
            if (comp.Equals(item, val)) return i;
            i++;
        }
        return -1;
    }

    public static IEnumerable<Transform> GetChildren(this HittTemplate template, Transform parent, bool emptyasnull = true)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var n = parent.GetChild(i);
            var g = GetPrefabOf(n.gameObject) as GameObject;
            if (g && template.objectIndex.GetValue(g.name) != null)
                yield return n;
            else if (template.empty == g)
                yield return emptyasnull ? null : n; // for empty
        }
    }

    public static void SetChildren(this HittTemplate template, int gateindex, Transform parent, GameObject newobject)
    {

        // validate
        if (template == null || parent == null) throw new Exception("Something wrong?");

        var item = template.GetItemOf(parent);

        if (item == null || item.entrances.Length == 0) return;

        gateindex = Mathf.Clamp(gateindex, 0, item.entrances.Length);// Clamp(item.entrances, );

        var childs = template.GetChildren(parent, false).ToArray();

        // add dummy int if needed
        int iter = 0;
        while ((childs.Length + iter++) <= gateindex)
            (PrefabUtility.InstantiatePrefab(template.empty) as GameObject).transform.SetParent(parent, false);

        if (iter > 0) // refresh
            childs = template.GetChildren(parent, false).ToArray();

        if (!newobject)
            newobject = PrefabUtility.InstantiatePrefab(template.empty) as GameObject;

        // assign to new one
        newobject.transform.SetParent(parent, false, childs[gateindex].GetSiblingIndex());

        // delete old
        Undo.DestroyObjectImmediate(childs[gateindex].gameObject);

        // sync
        template.Synchronize(newobject.transform);
    }

    public static void SetParent(this Transform obj, Transform parent, bool worldPositionStays, int index)
    {
        obj.SetParent(parent, worldPositionStays);
        obj.SetSiblingIndex(index);
    }

    public static void Synchronize(this HittTemplate template, Transform root, bool recursive = true)
    {
        var r = template.GetItemOf(root);
        if (r == null || r.entrances.Length == 0) return;

        // attempt to syncronize itself
        if (root.parent)
        {
            var pp = root.parent;
            var p = template.GetItemOf(pp);
            if (p != null && r.port.tag != 0)
            {
                var i = template.GetChildren(pp).IndexOf(root);
                var e = r.port; var g = p.entrances[i];
                root.localRotation = Conjugate(e.rotation * g.rotation);
                root.localPosition = g.position + root.localRotation * - e.position;
            }
        }

        if (recursive)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                template.Synchronize(root.GetChild(i), true);
            }
        }
    }


    public static GameObject Instantiate(this HittItem item)
    {
        if (!item.prefab) return null;
        var p = PrefabUtility.InstantiatePrefab(item.prefab) as GameObject;
        p.name = item.name; // yep
        return p;
    }
}
