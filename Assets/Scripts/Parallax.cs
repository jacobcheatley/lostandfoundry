using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField]
    private Transform camera;
    [SerializeField]
    private float factor = 0.1f;

    // Update is called once per frame
    void Update()
    {
        float newX = camera.position.x * factor;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}
