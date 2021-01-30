using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : Retractable
{
    [Header("Numeric Config")]
    [SerializeField]
    private float speed = 6;
    [SerializeField]
    private float launchTimeoutSeconds = 2f;
    [SerializeField]
    private int maxHookedItems = 1;
    [SerializeField]
    private float retractionRate = 3f;

    [Header("Skill Specifics")]
    [SerializeField]
    private float split3Degrees = 15f;

    [Header("GameObject References")]
    [SerializeField]
    private LineRenderer ropeRenderer;
    private List<Vector3> ropeRendererPoints = new List<Vector3>();

    [SerializeField]
    private GameObject retractCameraAnchor;

    private bool launched = false;
    private Vector3 movement;
    private List<Coroutine> travellingCoroutines = new List<Coroutine>();

    private List<HookedItemInfo> hookedItems = new List<HookedItemInfo>();
    private List<ChildHookInfo> childHooks = new List<ChildHookInfo>();

    [SerializeField]
    private bool isChild = false;

    private void Start()
    {
        // TODO: Set private fields for upgraded skill levels from SkillManager

        // We need to prime the rope renderer with a couple of initial points.
        // The second point gets dragged along with the hook as it travels.
        if (transform.parent == null)
        {
            AddRopeRendererPoint(transform.position);
        }
        else
        {
            AddRopeRendererPoint(transform.parent.position);
        }
        AddRopeRendererPoint(transform.position);
    }

    /// <summary>
    /// Use this for the main hook only
    /// </summary>
    public void Launch()
    {
        CameraControl.Follow(transform);
        Vector3 orientation = transform.position - transform.parent.position;
        orientation.z = 0;
        movement = orientation.normalized;
        transform.parent = null;
        launched = true;

        // Start the behaviour coroutines:
        // Movement
        travellingCoroutines.Add(StartCoroutine(Travel()));
        // The hook can only travel for so long before it runs out of time
        travellingCoroutines.Add(StartCoroutine(StopTravellingAfter(launchTimeoutSeconds)));
    }

    /// <summary>
    /// Use this for child hooks only.
    /// </summary>
    /// <param name="orientation">The direction the hook should travel in</param>
    public void LaunchChild(Vector3 orientation)
    {
        isChild = true;
        launchTimeoutSeconds *= 0.75f;

        AddRopeRendererPoint(transform.position);
        AddRopeRendererPoint(transform.position);

        movement = orientation;
        launched = true;
        travellingCoroutines.Add(StartCoroutine(Travel()));
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

    private void Update()
    {
        // Drag the last point of the renderer along with us.
        // We don't ever add any new ones here - when other methods add new points, it'll
        // work as a HARD angle
        if (ropeRenderer.positionCount > 1)
        {
            ropeRenderer.SetPosition(
                ropeRenderer.positionCount - 1,
                new Vector3(transform.position.x, transform.position.y)
                );
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
        // Retract rope renderer handles pausing when we need to wait for children to retract
        // This is gross I know but it's fine, don't worry
        StartCoroutine(RetractRopeRenderer());

        if (!isChild)
        {
            CameraControl.Follow(retractCameraAnchor.transform, timeFrame: 0.5f, delay: 0.3f);
        }
    }

    private IEnumerator RetractRopeRenderer()
    {
        // Similar code to Retractable::DoRetract(), but with more logic for pausing and shit
        int currentMovingIndex = ropeRenderer.positionCount - 1;
        while (true)
        {
            // Retract to the next point up the rope
            Vector3 target = ropeRenderer.GetPosition(currentMovingIndex - 1);
            Vector3 distanceToTarget = target - ropeRenderer.GetPosition(currentMovingIndex);

            // Move the last rope segment so it stays with us as we move
            ropeRenderer.SetPosition(
                currentMovingIndex,
                ropeRenderer.GetPosition(currentMovingIndex) +
                    (distanceToTarget.normalized * retractionRate * Time.deltaTime)
                );

            // If we've reached the target rope segment...
            if (distanceToTarget.sqrMagnitude <= (retractionRate * Time.deltaTime) * (retractionRate * Time.deltaTime))
            {
                // Start moving towards the next segment
                currentMovingIndex -= 1;
                // And remove the segment we were just using
                ropeRenderer.positionCount -= 1;

                // If there are any child hooks that still exist and split off at the point we're at,
                // we need to wait for them to finish retracting
                if (childHooks.Find(info => info.ropeRendererIndex == currentMovingIndex) != null)
                {
                    // Tell the main Retractable method to pause in the meantime
                    temporarilyPauseRetracting = true;
                    while (true)
                    {
                        // Remove references to any child hooks that have killed themselves by retracting fully
                        childHooks.FindAll(info => info.childHook == null).ForEach(info => childHooks.Remove(info));
                        // If there are none left that split off at this point, then we're done and we can continue.
                        if (childHooks.Find(info => info.ropeRendererIndex == currentMovingIndex) == null)
                        {
                            temporarilyPauseRetracting = false;
                            break;
                        }
                        yield return null;
                    }
                }

                // If we've reached the end of the line, we're done.
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

    override protected void FinishedRetracting()
    {
        isRetracting = false;
        if (isChild)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If we've hit a hookable that isn't already hooked...
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hookable") &&
            !collision.gameObject.GetComponent<Hookable>().isHooked
            )
        {
            // Add it to the hooked items we keep track of
            hookedItems.Add(
                new HookedItemInfo() { 
                    hookedItem = collision.gameObject.GetComponent<Hookable>(),
                    ropeRendererPointIndex = ropeRendererPoints.Count - 1,
                    hookedItemCollisionPoint = collision.ClosestPoint(transform.position)
                    }
                );

            // Add a new point to the rope renderer
            AddRopeRendererPoint(transform.position);

            // Tell the hookable it's been hooked by us
            collision.gameObject.GetComponent<Hookable>().Hooked(this.gameObject);

            // If we've reached the max hookable items, we stop
            if (hookedItems.Count >= maxHookedItems)
            {
                StopTravelling();
            }
            // Split3 skill logic
            else if (SkillTracker.IsSkillUnlocked(SkillID.TripleShot))
            {
                for (int i = -1; i < 2; i++)
                {
                    // Don't add a new hook in the same path that we're travelling
                    if (i == 0)
                    {
                        continue;
                    }

                    // Clone ourselves and keep track of the new Hook
                    GameObject newObject = Instantiate(gameObject);
                    Hook newObjectHook = newObject.GetComponent<Hook>();
                    childHooks.Add(new ChildHookInfo(
                        _childHook: newObjectHook,
                        _ropeRendererIndex: ropeRendererPoints.Count - 2
                    ));

                    // Set up its scale and rotation, then tell it to start moving.
                    newObject.transform.localRotation = transform.rotation * Quaternion.Euler(0, 0, i * split3Degrees);
                    newObject.transform.localScale = transform.localScale * 0.5f;
                    newObjectHook.LaunchChild(Quaternion.Euler(0, 0, i * split3Degrees) * movement);
                }
            }
        }
    }
}
