using System.Collections;
using UnityEngine;

public class Hook : MonoBehaviour
{
    [SerializeField]
    private float speed = 6;

    private bool launched = false;
    private Vector3 movement;

    public void Launch()
    {
        CameraControl.Follow(transform);
        Vector3 orientation = transform.position - transform.parent.position;
        orientation.z = 0;
        movement = orientation.normalized;
        transform.parent = null;
        launched = true;
        StartCoroutine(Travel());
    }

    private IEnumerator Travel()
    {
        bool travelledAFrame = false;
        while (true)
        {
            transform.position += movement * speed * Time.deltaTime;
            if (travelledAFrame && Input.GetMouseButtonDown(0))
            {
                // Quantum tunneling
                transform.position += movement * 3f;
                CameraControl.Follow(transform);
            }
            yield return null;
            travelledAFrame = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // TODO: Collision logic
        Debug.Log($"Collided {collision.gameObject.name}");

    }
}