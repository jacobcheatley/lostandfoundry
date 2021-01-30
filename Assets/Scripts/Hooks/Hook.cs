using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : Retractable
{
    [SerializeField]
    private bool isChild = false;

    [Header("GameObject References")]
    [HideInInspector]
    public GameObject retractCameraAnchor;
    [HideInInspector]
    public GameObject hookPrefab;
    [HideInInspector]
    public HookLauncher hookLauncher;
    [SerializeField]
    private LineRenderer ropeRenderer;

    [Header("General Config Base Values")]
    [SerializeField]
    private float maxHookDurationSeconds = 2f;
    [SerializeField]
    private float speed = 6f;
    [SerializeField]
    private int maxHookedItems = 1;
    [SerializeField]
    private float retractionRate = 3f;

    [Header("Skill Specifics")]
    [SerializeField]
    private float durationAddSecondPerLevel = 1f;
    [SerializeField]
    private float speedAddPerLevel = 6f;
    [SerializeField]
    private float tripleShotDegrees = 15f;
    [SerializeField]
    private float pentaShotDegrees = 15f;
    [SerializeField]
    private float septaShotDegrees = 15f;

    [Header("Movement-related variables")]
    private bool launched = false;
    private Vector3 movement;
    private List<Coroutine> travellingCoroutines = new List<Coroutine>();

    [SerializeField]
    private List<Vector3> ropeRendererPoints = new List<Vector3>();

    [Header("Hooking-related variables")]
    private List<HookedItemInfo> hookedItems = new List<HookedItemInfo>();
    private Hook parentHook;
    private List<ChildHookInfo> childHooks = new List<ChildHookInfo>();

    public void BeginDangle()
    {
        if (!isChild)
        {
            AddRopeRendererPoint(transform.parent.position);
            AddRopeRendererPoint(transform.position);
        }
    }

    private void ApplyBasicSkills()
    {
        new List<bool>()
        {
            SkillTracker.IsSkillUnlocked(SkillID.HookDuration1),
            SkillTracker.IsSkillUnlocked(SkillID.HookDuration2),
            SkillTracker.IsSkillUnlocked(SkillID.HookDuration3),
            SkillTracker.IsSkillUnlocked(SkillID.HookDuration4),
        }.FindAll(u => u).ForEach(u => maxHookDurationSeconds += durationAddSecondPerLevel);

        new List<bool>()
        {
            SkillTracker.IsSkillUnlocked(SkillID.HookSpeed1),
            SkillTracker.IsSkillUnlocked(SkillID.HookSpeed2),
            SkillTracker.IsSkillUnlocked(SkillID.HookSpeed3),
            SkillTracker.IsSkillUnlocked(SkillID.HookSpeed4),
        }.FindAll(u => u).ForEach(u => speed += speedAddPerLevel);

        new List<bool>()
        {
            SkillTracker.IsSkillUnlocked(SkillID.Pierce1),
            SkillTracker.IsSkillUnlocked(SkillID.Pierce2),
            SkillTracker.IsSkillUnlocked(SkillID.Pierce3),
            SkillTracker.IsSkillUnlocked(SkillID.Pierce4),
        }.FindAll(u => u).ForEach(u => maxHookedItems += 1);
    }

    /// <summary>
    /// Use this for the main hook only
    /// </summary>
    public void Launch()
    {
        ApplyBasicSkills();

        // Leave a point behind when we launch, and correct the one that wasn't kept up to
        // date with the LineRenderer point that got dragged along as we dangle.
        ropeRendererPoints[ropeRendererPoints.Count - 1] = new Vector3(transform.position.x, transform.position.y);
        AddRopeRendererPoint(transform.position);

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
        travellingCoroutines.Add(StartCoroutine(StopTravellingAfter(maxHookDurationSeconds)));

        // Multishot skill logic
        int extraHooksPerSide = 0;
        float separationDegrees = 0f;
        if (SkillTracker.IsSkillUnlocked(SkillID.SeptaShot))
        {
            extraHooksPerSide = 3;
            separationDegrees = septaShotDegrees;
        }
        else if (SkillTracker.IsSkillUnlocked(SkillID.PentaShot))
        {
            extraHooksPerSide = 2;
            separationDegrees = pentaShotDegrees;
        }
        else if (SkillTracker.IsSkillUnlocked(SkillID.TripleShot))
        {
            extraHooksPerSide = 1;
            separationDegrees = tripleShotDegrees;
        }
        for (int i = -extraHooksPerSide; i <= extraHooksPerSide; i++)
        {
            // Don't add a new hook in the same path that we're travelling
            if (i == 0)
            {
                continue;
            }

            // Instantiate a copy of the hook prefab, and keep track of the new child
            GameObject newObject = Instantiate(hookPrefab, transform.position, transform.rotation);
            Hook newObjectHook = newObject.GetComponent<Hook>();
            childHooks.Add(new ChildHookInfo(
                _childHook: newObjectHook,
                // magic number here just makes retraction wait for the children properly
                _ropeRendererIndex: ropeRendererPoints.Count - 2
            ));
            newObjectHook.parentHook = this;

            // Set up its scale and rotation, then tell it to start moving.
            newObject.transform.localRotation = transform.rotation * Quaternion.Euler(0, 0, i * tripleShotDegrees);
            newObject.transform.localScale = transform.localScale * 0.5f;
            newObjectHook.LaunchChild(Quaternion.Euler(0, 0, i * separationDegrees) * movement);
        }
    }

    /// <summary>
    /// Use this for child hooks only.
    /// </summary>
    /// <param name="orientation">The direction the hook should travel in</param>
    public void LaunchChild(Vector3 orientation)
    {
        isChild = true;

        ApplyBasicSkills();

        AddRopeRendererPoint(transform.position);
        AddRopeRendererPoint(transform.position);

        movement = orientation;
        launched = true;
        travellingCoroutines.Add(StartCoroutine(Travel()));
        travellingCoroutines.Add(StartCoroutine(StopTravellingAfter(maxHookDurationSeconds)));
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
        hookedItems.FindAll(info => info.hookedItem == null).ForEach(info => hookedItems.Remove(info));
        hookedItems.ForEach(item => item.hookedItem.Retract(ropeRendererPoints, item, retractionRate));

        // Retract ourselves
        StartCoroutine(DoRetract(ropeRendererPoints, transform.position, ropeRendererPoints.Count - 1, retractionRate));
        // Retract rope renderer handles pausing when we need to wait for children to retract
        // This is gross I know but it's fine, don't worry
        StartCoroutine(RetractRopeRenderer());

        if (!isChild)
        {
            CameraControl.Follow(retractCameraAnchor.transform, timeFrame: 2f, delay: 0.3f);
        }
    }

    // This is the one for RetractRopeRenderer. It's class-scope so children can access it when they
    // pass their hooked items up to us to make retraction work properly there.
    [HideInInspector]
    public int currentMovingIndex;

    private IEnumerator RetractRopeRenderer()
    {
        // Similar code to Retractable::DoRetract(), but with more logic for pausing and shit
        currentMovingIndex = ropeRenderer.positionCount - 1;
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
                childHooks.FindAll(info => info.childHook == null).ForEach(info => childHooks.Remove(info));
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
                if (currentMovingIndex == 0)
                {
                    ropeRenderer.positionCount = 0;
                    currentMovingIndex = 0;
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
        // If we're a child of another hook, we need to pass all the hooked items up to be owned by
        // our parent hook. This includes giving them a new Retract call with a freshly constructed
        // HookedItemInfo.
        if (isChild)
        {
            hookedItems.ForEach(info => {
                if (info.hookedItem != null)
                {
                    HookedItemInfo newInfo = new HookedItemInfo()
                    {
                        hookedItem = info.hookedItem,
                        ropeRendererPointIndex = parentHook.currentMovingIndex,
                        hookedItemCollisionPoint = info.hookedItem.GetComponent<BoxCollider2D>().ClosestPoint(parentHook.transform.position)
                    };

                    parentHook.hookedItems.Add(newInfo);

                    info.hookedItem.Retract(
                        parentHook.ropeRendererPoints,
                        newInfo,
                        retractionRate
                    );
                }
            });
            Destroy(gameObject);
        }
        else
        {
            childHooks.FindAll(info => info.childHook == null).ForEach(info => childHooks.Remove(info));
            ropeRendererPoints.Clear();
            // This is for when it's fully retracted all the way back to the collector
            DayNightSwitcher.instance.Night();
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
        }
    }
}
