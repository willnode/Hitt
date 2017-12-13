using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

[CreateAssetMenu]
public class HittTemplate : ScriptableObject
{

    public HittItem[] items = new HittItem[0];

    public GateTags[] gates = { new GateTags() };

    public GameObject empty;

    [NonSerialized]
    public Dictionary<KeyCode, HittItem> keyIndex = new Dictionary<KeyCode, HittItem>();

    [NonSerialized]
    public Dictionary<string, HittItem> objectIndex = new Dictionary<string, HittItem>();

    [NonSerialized]
    public Dictionary<int, GateTags> gateIndex = new Dictionary<int, GateTags>();

    void OnEnable()
    {
        Populate();
    }

    public void Populate()
    {

        keyIndex.Clear();
        objectIndex.Clear();
        gateIndex.Clear();

        foreach (var item in items)
        {
            if (item.key != KeyCode.None)
            {
                if (keyIndex.ContainsKey(item.key))
                {
                    Debug.LogWarning("HittTemplate: Duplicate KeyCode Found! Fixing.");
                    item.key = KeyCode.None;
                }
                else
                    keyIndex[item.key] = item;
            }

            if (item.prefab != null)
            {
                if (objectIndex.ContainsKey(item.name))
                {
                    Debug.LogWarning("HittTemplate: Duplicate Prefab Name Found! Fixing.");
                    item.name = item.prefab.name;
                    while (objectIndex.ContainsKey(item.name))
                    {
                        item.name += ":";
                    }
                }
                objectIndex[item.name] = item;
            }
        }

        foreach (var gate in gates)
        {
            if (gateIndex.ContainsKey(gate.hash))
            {
                Debug.LogWarning("HittTemplate: Duplicate Gate Hash Found! Fixing.");

                var r = new Random();
                do gate.hash = r.Next();
                while (gate.hash == 0 || gateIndex.ContainsKey(gate.hash));
            }
            gateIndex[gate.hash] = gate;
        }
    }

    public void DrawEntrance(Entrance e, HittItem item, bool solid = false)
    {
        GateTags gate;
        if (gateIndex.TryGetValue(e.tag, out gate))
            gate.DrawWire(e.position, e.rotation, item == null ? item.center : e.position, solid);
    }

    void OnValidate()
    {
        Populate();
    }

}

[Serializable]
public class HittItem
{
    /// <summary>
    /// Unique prefab name
    /// </summary>
    public string name;

    /// <summary>
    /// Category name
    /// </summary>
    public string category;

    /// <summary>
    /// Prefab attached to item
    /// </summary>
    public GameObject prefab;
    /// <summary>
    /// Keycode to insert at Interactive Playground
    /// </summary>
    public KeyCode key = KeyCode.None;

    /// <summary>
    /// Center offset (useful at editing)
    /// </summary>
    public Vector3 center = Vector3.zero;

    /// <summary>
    /// port used
    /// </summary>
    public Entrance port = new Entrance();

    /// <summary>
    /// List of entrances (
    /// </summary>
    public Entrance[] entrances = new Entrance[0];

    public void AddEntrance(Entrance c)
    {
        if (c == null) throw new ArgumentNullException("c");

        if (port.tag == 0)
        {
            // this will feed to input port instead
            port = c;
        }
        else
        {
            Array.Resize(ref entrances, entrances.Length + 1);
            entrances[entrances.Length - 1] = c;
        }
    }

    public void RemoveEntrance(int index = -1)
    {
        // -1 means from last. We can't remove the port before all entrances gone
        if (index == -1)
        {
            if (entrances.Length == 0)
                port.tag = 0;
            else
                Array.Resize(ref entrances, entrances.Length - 1);
        }
        else
        {
            Array.Copy(entrances, index + 1, entrances, index, entrances.Length - index);
            Array.Resize(ref entrances, entrances.Length - 1);
        }
    }

    public void AssignPrefab(GameObject g)
    {
        if (!g)
        {
            center = Vector3.zero;
            prefab = null;
            name = string.Empty;
        }
        else
        {
            var mf = g.GetComponentInChildren<MeshFilter>();
            if (mf && mf.sharedMesh)
                center = mf.sharedMesh.bounds.center;

            prefab = g;
            name = g.name;
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

[Serializable]
public class GateTags
{

    public string name = "Uncategorized";

    public Vector3 size = new Vector3(1f, 1f, 0.2f);

    public Color color = Color.cyan;

    public int hash;

    public GateTags()
    {
        if (hash == 0)
            GenerateNewHash();
    }

    protected void GenerateNewHash()
    {
        var r = new Random();
        do hash = r.Next();
        while (hash == 0);
    }

    public void DrawWire(Vector3 pos, Quaternion rot, Vector3 center, bool solid)
    {
        var c = Handles.color;
        var m2 = Handles.matrix;
        var c2 = color;

        if (!solid) c2.a *= 0.8f;

        Handles.color = c2;
        Handles.DrawLine(center, pos);
        var m = Matrix4x4.TRS(pos, rot, Vector3.one);
        Handles.matrix *= m;
        if (!solid) Handles.DrawWireCube(Vector3.zero, size);
        else
        {
            var v = Vector3.forward * size.z * 0.5f; var s = (size.x + size.y) * 0.5f;
            Handles.DrawWireDisc(v, Vector3.forward, s);
            Handles.DrawWireDisc(-v, Vector3.forward, s);
        }
        Handles.DrawLine(Vector3.zero, Vector3.forward * size.z);
        //        if (solid)
        //          Handles.CubeHandleCap(0, pos, Quaternion.identity, size);
        Handles.matrix = m2;
        Handles.color = c;
    }

    public GateTags Clone()
    {
        var n = (GateTags)MemberwiseClone();
        n.GenerateNewHash();
        return n;
    }
}
