using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Knob : MonoBehaviour
{
    private SpriteRenderer r;
    void Start()
    {
        r = GetComponent<SpriteRenderer>();
    }
    public void SetColor(string hex)
    {
        Color result;
        if (ColorUtility.TryParseHtmlString(hex, out result))
            r.color = result;
    }
}
