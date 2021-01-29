using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : Retractable
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

        // We need to prime the rope renderer with a couple of initial points.
        // The second point gets dragged along with the hook as it travels.
        AddRopeRendererPoint(transform.position);
        AddRopeRendererPoint(transform.position);

        // Start the behaviour coroutines:
        // Movement
        travellingCoroutines.Add(StartCoroutine(Travel()));
        // Make the rope renderer render the rope at the right position as we travel
        travellingCoroutines.Add(StartCoroutine(UpdateRopeRenderer()));
        // The hook can only travel for so long before it runs out of time
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
        // The rope renderer needs to totally ignore the Z value it gets given, so it's consistent
        // and its distance measurements work properly.
        point.z = 0;
        ropeRenderer.positionCount += 1;
        ropeRenderer.SetPosition(ropeRenderer.positionCount - 1, point);
        ropeRendererPoints.Add(point);
    }

    private IEnumerator UpdateRopeRenderer()
    {
        while (true)
        {
            // Drag the last point of the renderer along with us.
            // We don't ever add any new ones - when other methods add new points, it'll
            // work as a HARD angle
            ropeRenderer.SetPosition(
                ropeRenderer.positionCount - 1,
                new Vector3(transform.position.x, transform.position.y)
                );

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator StopTravellingAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        StopTravelling();
    }

    public void StopTravelling()
    {
        // We might stop travelling for one of several reasons - timeout, hooked too much, etc.
        // Regardless, all of our moving-hook-mode coroutines need to stop.
        travellingCoroutines.ForEach(StopCoroutine);

        // Inform each of the hooked items that they've been hooked and should begin being retracted to the colector
        hookedItems.ForEach(item => item.hookedItem.Retract(ropeRendererPoints, item, retractionRate));

        // Retract ourselves
        StartCoroutine(DoRetract(ropeRendererPoints, transform.position, ropeRendererPoints.Count - 1, retractionRate));
        StartCoroutine(RetractRopeRenderer());
    }

    private IEnumerator RetractRopeRenderer()
    {
        // Similar code to Retractable::DoRetract()
        int currentMovingIndex = ropeRenderer.positionCount - 1;
        while (true)
        {
            Vector3 target = ropeRenderer.GetPosition(currentMovingIndex - 1);
            Vector3 distanceToTarget = target - ropeRenderer.GetPosition(currentMovingIndex);

            ropeRenderer.SetPosition(
                currentMovingIndex,
                ropeRenderer.GetPosition(currentMovingIndex) + 
                    (distanceToTarget.normalized * retractionRate * Time.deltaTime)
                );

            if (distanceToTarget.sqrMagnitude <= (retractionRate * Time.deltaTime) * (retractionRate * Time.deltaTime))
            {
                currentMovingIndex -= 1;
                ropeRenderer.positionCount -= 1;
                if (ropeRenderer.positionCount <= 1)
                {
                    ropeRenderer.positionCount = 0;
                    launched = false;
                    break;
                }
            }

            yield return null;
        }
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

            // Add a new point to the rope renderer, in case of future rope-bending upgrades.
            AddRopeRendererPoint(transform.position);

            collision.gameObject.GetComponent<Hookable>().Hooked(this.gameObject);

            // Stop condition
            if (hookedItems.Count >= maxHookedItems)
            {
                StopTravelling();
            }
        }
    }
}
