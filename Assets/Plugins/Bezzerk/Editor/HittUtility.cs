using System;
using System.Collections.Generic;
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


}
