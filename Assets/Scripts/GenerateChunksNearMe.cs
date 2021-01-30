using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateChunksNearMe : MonoBehaviour
{
    [SerializeField]
    private float interval = 0.1f;
    [SerializeField]
    private float radius = 40f;

    void Start()
    {
        StartCoroutine(NeedADispenserHere());
    }

    private IEnumerator NeedADispenserHere()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            Debug.Log((Vector2)transform.position);
            LevelGenerator.PutisSpencerHere(transform.position, radius);
        }
    }
}
