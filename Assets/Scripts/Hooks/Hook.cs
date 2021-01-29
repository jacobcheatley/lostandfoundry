using System;
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
    private int maxHookedItems = 1;

    [SerializeField]
    private float retractionRate = 3f;

    [SerializeField]
    private LineRenderer ropeRenderer;
    private List<Vector3> ropeRendererPoints = new List<Vector3>();

    private bool launched = false;
    private Vector3 movement;
    private List<Coroutine> travellingCoroutines = new List<Coroutine>();

    private List<HookedItemInfo> hookedItems = new List<HookedItemInfo>();

    private void Start()
    {
        // TODO: Set private fields for upgraded skill levels from SkillManager
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
        travellingCoroutines.Add(StartCoroutine(StopTravellingAfter(launchTimeoutSeconds)));
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

    private void AddRopeRendererPoint(Vector3 point)
    {
        ropeRenderer.positionCount += 1;
        ropeRenderer.SetPosition(ropeRenderer.positionCount - 1, point);
        ropeRendererPoints.Add(point);
    }

    private IEnumerator UpdateRopeRenderer()
    {
        while (true)
        {
            AddRopeRendererPoint(transform.position);

            // TODO: Optimisation target here
            //  Only add new points when there's a change
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
            }
        }
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

        hookedItems.ForEach(item => item.hookedItem.Retract(ropeRendererPoints, item, retractionRate));

        // TODO: Start a "retract rope" coroutine
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hookable"))
        {
            hookedItems.Add(
                new HookedItemInfo() { 
                    hookedItem = collision.gameObject.GetComponent<Hookable>(),
                    ropeRendererPointIndex = ropeRendererPoints.Count - 1,
                    hookedItemCollisionPoint = collision.ClosestPoint(transform.position)
                    }
                );

            AddRopeRendererPoint(transform.position);

            collision.gameObject.GetComponent<Hookable>().Hooked(this.gameObject);

            if (hookedItems.Count >= maxHookedItems)
            {
                StopTravelling();
            }
        }
    }
}
