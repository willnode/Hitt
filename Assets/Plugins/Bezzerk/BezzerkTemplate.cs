using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BezzerkTemplate : ScriptableObject
{

    public GameObject[] items = new GameObject[0];
    
    public EntranceTemplate[] entrances = { new EntranceTemplate() };

    [NonSerialized]
    public Dictionary<KeyCode, GameObject> keyIndex = new Dictionary<KeyCode, GameObject>();

    public void PopulateIndex()
    {
        keyIndex.Clear();

        foreach (var item in items)
        {
            if (!item || !item.GetComponent<BezzerkItem>()) continue;

            keyIndex[item.GetComponent<BezzerkItem>().key] = item;
        }
    }

    public void DrawEntrance (Entrance e)
    {
        if (e.tag >= entrances.Length) return;
        entrances[e.tag].DrawWire(e.position, e.rotation);
    }

}

[Serializable]
public class EntranceTemplate
{

    public string name = "Uncategorized";

    public Vector3[] shape = new Vector3[]
    {
        new Vector3(-1, -1),
        new Vector3(1, -1),
        new Vector3(1, 1),
        new Vector3(-1, 1),
    };

    public Color color = Color.cyan;

    public void DrawWire(Vector3 pos, Quaternion rot)
    {
        Gizmos.color = color;

        for (int i = 0; i < shape.Length;)
        {
            Gizmos.DrawLine(pos + rot * shape[i++], 
                pos + rot * shape[i % shape.Length]);
        }

    }
}
