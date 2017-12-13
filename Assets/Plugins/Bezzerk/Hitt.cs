using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// nothing but a placeholder
public class Hitt : MonoBehaviour
{

    public HittTemplate template;

    public static Action updateFeed;

    void Reset()
    {
        // when component get added
        if (updateFeed != null)
            updateFeed();
    }

    [ContextMenu("Flatten")]
    void FlattenHierarchy()
    {
        FlattenHierarchy(transform, transform);
        DestroyImmediate(this);
    }

    void FlattenHierarchy(Transform from, Transform to)
    {
        for (int i = from.childCount; i-- > 0;)
        {
            FlattenHierarchy(from.GetChild(i), to);
        }

        if (from != to)
          UnityEditor.Undo.SetTransformParent(from, to, "Flatten");
    }
}


