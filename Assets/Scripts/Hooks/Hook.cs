using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    [SerializeField]
    private float speed = 6;
    [SerializeField]
    private float launchTimeoutSeconds = 2f;
    [SerializeField]
    private LineRenderer ropeRenderer;
    [SerializeField]
    private float addPointToRopeRendererSeconds = 0.25f;

    private bool launched = false;
    private Vector3 movement;
    private List<Coroutine> travellingCoroutines = new List<Coroutine>();

    private List<GameObject> hookedItems = new List<GameObject>();

    private void Awake()
    {
        ropeRenderer = GetComponent<LineRenderer>();
    }

    public void Launch()
    {
        CameraControl.Follow(transform);
        Vector3 orientation = transform.position - transform.parent.position;
        orientation.z = 0;
        movement = orientation.normalized;
        transform.parent = null;
        launched = true;

        travellingCoroutines.Add(StartCoroutine(Travel()));
        travellingCoroutines.Add(StartCoroutine(UpdateRopeRenderer()));
        StartCoroutine(StopTravellingAfter(launchTimeoutSeconds));
    }

    private IEnumerator Travel()
    {
        bool travelledAFrame = false;
        while (true)
        {
            transform.position += movement * speed * Time.deltaTime;
            //if (travelledAFrame && Input.GetMouseButtonDown(0))
            //{
            //    // Quantum tunneling
            //    transform.position += movement * 3f;
            //    CameraControl.Follow(transform);
            //}
            yield return null;
            travelledAFrame = true;
        }
    }

    private IEnumerator UpdateRopeRenderer()
    {
        ropeRenderer.SetPosition(ropeRenderer.positionCount, transform.position);

        yield return new WaitForSeconds(addPointToRopeRendererSeconds);
    }

    /// <summary>
    /// This is just a test example of using StopTravelling(), since I don't want to rely on right-click
    /// to stop it due to Fun Multiplatform Shenanigans. Replace it with whatever else once skills are
    /// defined.
    /// </summary>
    private IEnumerator StopTravellingAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        StopTravelling();
    }

    public void StopTravelling()
    {
        Debug.Log("Stop hooking!");
        travellingCoroutines.ForEach(StopCoroutine);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hookable"))
        {
            hookedItems.Add(collision.gameObject);
            collision.gameObject.GetComponent<Hookable>().Hooked(this.gameObject);
        }
    }
}
