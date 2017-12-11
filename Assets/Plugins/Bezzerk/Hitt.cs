using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitt : MonoBehaviour {

    public HittTemplate template;

    public static Action updateFeed;

    void Reset ()
    {
        // when component get added
        if (updateFeed != null)
            updateFeed();
    }
}


