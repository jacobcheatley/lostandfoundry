using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hookable"))
        {
            Hookable other = collision.GetComponent<Hookable>();
            other.Collected(gameObject);
            ResourceTracker.Money += other.value;
        }
    }
}
