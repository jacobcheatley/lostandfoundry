using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookable : Retractable
{
    public int value;

    public void Hooked(GameObject toWhom)
    {
        // TODO: Some fancy animations here or something.
        //  We want the specifics to be determined by the object type, which
        //  will be somewhere on this GameObject.
        Debug.Log($"This {gameObject.name} got hooked to {toWhom.name}!");
    }

    public void Collected(GameObject byWhom)
    {
        Debug.Log($"This {gameObject.name} got collected by {byWhom.name}!");
        Destroy(gameObject);
    }

    public void Retract(List<Vector3> alongLine, HookedItemInfo info, float withSpeed)
    {
        StartCoroutine(DoRetract(alongLine, info.hookedItemCollisionPoint, info.ropeRendererPointIndex, withSpeed));
    }
}
