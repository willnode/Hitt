using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class HittTemplate : ScriptableObject
{

    public GameObject[] items = new GameObject[0];

    public EntranceTemplate[] entrances = { new EntranceTemplate() };

    void OnEnable()
    {
        // normalizing
        int iter = 0;
        foreach (var item in entrances)
            item.index = iter++;
    }

    [NonSerialized]
    public Dictionary<KeyCode, GameObject> keyIndex = new Dictionary<KeyCode, GameObject>();

    public void PopulateIndex()
    {
        keyIndex.Clear();

        foreach (var item in items)
        {
            if (!item || !item.GetComponent<HittItem>()) continue;

            keyIndex[item.GetComponent<HittItem>().key] = item;
        }
    }

    public void DrawEntrance(Entrance e, HittItem item, bool solid = false)
    {
        if (e.tag < 0 || e.tag >= entrances.Length) return;
        entrances[e.tag].DrawWire(e.position, e.rotation, item ? item.center : e.position, solid);
    }

}

[Serializable]
public class EntranceTemplate
{

    public string name = "Uncategorized";

    public Vector3 size = new Vector3(1f, 1f, 0.2f);

    public Color color = Color.cyan;

    public int index;

    public void DrawWire(Vector3 pos, Quaternion rot, Vector3 center, bool solid)
    {
        var c = Handles.color;
        var m2 = Handles.matrix;
        var c2 = color;
        if (!solid) c2.a *= 0.4f;

        Handles.color = c2;
        Handles.DrawLine(center, pos);
        var m = Matrix4x4.TRS(pos, rot, Vector3.one);
        Handles.matrix *= m;
        Handles.DrawWireCube(Vector3.zero, size);
        //        if (solid)
        //          Handles.CubeHandleCap(0, pos, Quaternion.identity, size);
        Handles.matrix = m2;
        Handles.color = c;
    }

    public EntranceTemplate Clone()
    {
        return (EntranceTemplate)MemberwiseClone();
    }
}
