using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookable : MonoBehaviour
{
    public int value;

    public void Hooked(GameObject toWhom)
    {
        // TODO: Some fancy animations here or something.
        //  We want the specifics to be determined by the object type, which
        //  will be somewhere on this GameObject.
        Debug.Log($"This {gameObject.name} got hooked to {toWhom.name}!");
    }
}
