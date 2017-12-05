using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezzerk : BezzerkItem {

    public BezzerkTemplate template;

    protected override void OnDrawGizmosSelected()
    {
        DrawWireHierarchy(true);
    }

}


