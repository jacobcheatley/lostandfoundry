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
    //[HideInInspector]
    public Camera mainCamera;
    [HideInInspector]
    public GameObject hookPrefab;
    [HideInInspector]
    public HookLauncher hookLauncher;
    [SerializeField]
    private LineRenderer ropeRenderer;

    [Header("General Config Base Values")]
    [SerializeField]
    private float baseHookDurationSeconds = 2f;
    [SerializeField]
    private float baseSpeed = 4f;
    [SerializeField]
    private int baseMaxHookedItems = 1;
    [SerializeField]
    private int baseMaxLaunchCount = 1;
    [SerializeField]
    private float baseRetractionRate = 3f;
    [SerializeField]
    private float quantumTunnelCooldownSeconds = 2f;
    [SerializeField]
    private float targetedRedirectCooldownSeconds = 2f;

    [Header("Skill Specifics")]
    [SerializeField]
    private float durationAddSecondPerLevel = 1f;
    [SerializeField]
    private float speedAddPerLevel = 2f;
    [SerializeField]
    private float tripleShotDegrees = 15f;
    [SerializeField]
    private float pentaShotDegrees = 15f;
    [SerializeField]
    private float septaShotDegrees = 15f;

    [Header("Active Config Values Post-Skill")]
    [SerializeField]
    private float hookDurationSeconds;
    [SerializeField]
    private float speed;
    [SerializeField]
    private int maxHookedItems;
    [SerializeField]
    private int hookedItemsThisLaunch;
    [SerializeField]
    private int maxLaunchCount;
    [SerializeField]
    private int launchCount;
    [SerializeField]
    private float retractionRate;
    [SerializeField]
    private float quantumTunnelElapsedCooldown = 0;
    [SerializeField]
    private float targetedRedirectElapsedCooldown = 0;

    /// <summary>
    /// 0 = just used it, 1 = ready
    /// </summary>
    public float quantumTunnelCooldownProgress
    {
        get
        {
            return Mathf.Clamp(quantumTunnelElapsedCooldown / quantumTunnelCooldownSeconds, 0, 1);
        }
    }

    public float targetedRedirectCooldownProgress
    {
        get
        {
            return Mathf.Clamp(targetedRedirectElapsedCooldown / targetedRedirectCooldownSeconds, 0, 1);
        }
    }

    [Header("Movement-related variables")]
    private bool launched = false;
    [SerializeField]
    private Vector3 movement;
    private List<Coroutine> travellingCoroutines = new List<Coroutine>();

    private List<Vector3> ropeRendererPoints = new List<Vector3>();
    private List<int> newJumpPointIndices = new List<int>(); 

    [Header("Hooking-related variables")]
    private List<HookedItemInfo> hookedItems = new List<HookedItemInfo>();
    private Hook parentHook;
    private List<ChildHookInfo> childHooks = new List<ChildHookInfo>();

    public void BeginDangle()
    {
        if (!isChild)
        {
            if (transform.parent != null)
            {
                AddRopeRendererPoint(transform.parent.position);
            }
            else
            {
                AddRopeRendererPoint(transform.position);
            }
            AddRopeRendererPoint(transform.position);
        }
    }

    public void ResetBasicSkills()
    {
        hookDurationSeconds = baseHookDurationSeconds;
        speed = baseSpeed;
        maxHookedItems = baseMaxHookedItems;
        maxLaunchCount = baseMaxLaunchCount;
        retractionRate = baseRetractionRate;
        quantumTunnelElapsedCooldown = quantumTunnelCooldownSeconds;
        targetedRedirectElapsedCooldown = targetedRedirectCooldownSeconds;
    }

    private void ApplyBasicSkills()
    {
        ResetBasicSkills();

        new List<bool>()
        {
            SkillTracker.IsSkillUnlocked(SkillID.HookDuration1),
            SkillTracker.IsSkillUnlocked(SkillID.HookDuration2),
            SkillTracker.IsSkillUnlocked(SkillID.HookDuration3),
            SkillTracker.IsSkillUnlocked(SkillID.HookDuration4),
        }.FindAll(u => u).ForEach(u => hookDurationSeconds += durationAddSecondPerLevel);

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

        new List<bool>()
        {
            SkillTracker.IsSkillUnlocked(SkillID.LaunchAgain1),
            SkillTracker.IsSkillUnlocked(SkillID.LaunchAgain2),
            SkillTracker.IsSkillUnlocked(SkillID.LaunchAgain3),
        }.FindAll(u => u).ForEach(u => maxLaunchCount += 1);

        int totalUnlockedSkills = 0;
        foreach (SkillID skill in Enum.GetValues(typeof(SkillID))) {
            if ((SkillTracker.UnlockedSkills & skill) == skill)
            {
                totalUnlockedSkills += 1;
            }
        }
        retractionRate += totalUnlockedSkills;
    }

    public List<Hookable> nearestHookables = new List<Hookable>();

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
        launchCount += 1;
        hookedItemsThisLaunch = 0;

        // Start the behaviour coroutines:
        // Movement
        travellingCoroutines.Add(StartCoroutine(Travel()));
        // The hook can only travel for so long before it runs out of time
        travellingCoroutines.Add(StartCoroutine(StopTravellingAfter(hookDurationSeconds)));

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

        void NewChildHook(Quaternion rotation)
        {
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
            newObject.transform.localScale = transform.localScale * 0.5f;

            newObject.transform.localRotation = transform.localRotation * rotation;
            newObjectHook.LaunchChild(rotation * movement);
        }

        if (!SkillTracker.IsSkillUnlocked(SkillID.HomingExtraShots))
        {
            for (int hookIndex = -extraHooksPerSide; hookIndex <= extraHooksPerSide; hookIndex++)
            {
                // Don't add a new hook in the same path that we're travelling
                if (hookIndex == 0)
                {
                    continue;
                }
                // Normally we just splay them out with `separationDegrees` between them
                NewChildHook(Quaternion.Euler(0, 0, hookIndex * separationDegrees));
            }
        }
        else
        {
            List<Hookable> unhookedHookables = new List<Hookable>(
                LevelGenerator.instance.levelParent.GetComponentsInChildren<Hookable>()
            ).FindAll(hookable => !hookable.isHooked);

            unhookedHookables.Sort(Comparer<Hookable>.Create(
                (h1, h2) => Vector3.Distance(transform.position, h1.transform.position)
                .CompareTo(Vector3.Distance(transform.position, h2.transform.position))
            ));

            nearestHookables = new List<Hookable>();
            int hookableIndex;
            // stop early if we've reached the number of hooks or the number of hookables
            for (hookableIndex = 0; hookableIndex < extraHooksPerSide * 2 && hookableIndex < unhookedHookables.Count; hookableIndex++)
            {
                nearestHookables.Add(unhookedHookables[hookableIndex]);
            }

            // With homingExtraShots, we target the `n` nearest Hookables and aim at them.
            for (hookableIndex = 0; hookableIndex < nearestHookables.Count; hookableIndex++)
            {
                Vector3 from = movement;
                Vector3 to = nearestHookables[hookableIndex].transform.position - transform.position;
                Quaternion rotation = Quaternion.FromToRotation(from, to);
                NewChildHook(rotation);
            }
            // for any leftovers, we alternate the side they get splayed out at
            for (; hookableIndex < extraHooksPerSide * 2; hookableIndex++)
            {
                Debug.LogError("Ran out of hookables to target! Hopefully I can get away with not implementing this since there will always be a ton of hookables ;)");
            }
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
        movement.z = 0;
        movement = movement.normalized;
        launched = true;
        travellingCoroutines.Add(StartCoroutine(Travel()));
        travellingCoroutines.Add(StartCoroutine(StopTravellingAfter(hookDurationSeconds)));
    }

    private bool travelledAFrame = false;

    public void DoQuantumTunnel()
    {
        if (SkillTracker.IsSkillUnlocked(SkillID.QuantumTunnel) &&
            quantumTunnelElapsedCooldown >= quantumTunnelCooldownSeconds &&
            travelledAFrame &&
            !isChild
            )
        {
            quantumTunnelElapsedCooldown = 0f;
            CameraControl.Follow(transform, timeFrame: 2f);
            transform.position += movement * 3f;
        }
    }

    public void DoTargetedRedirect()
    {
        if (SkillTracker.IsSkillUnlocked(SkillID.Redirect) &&
            targetedRedirectElapsedCooldown >= targetedRedirectCooldownSeconds &&
            travelledAFrame &&
            !isChild
            )
        {
            targetedRedirectElapsedCooldown = 0;

            // Get the mouse location
            Vector3 mouseWorldCoords = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldCoords.z = transform.position.z;

            // Look at the mouse
            Vector3 perpendicular = mouseWorldCoords - transform.position;
            Quaternion lookingAtMouse = Quaternion.LookRotation(Vector3.forward, perpendicular);

            movement = perpendicular.normalized;
            transform.localRotation = lookingAtMouse;
            AddRopeRendererPoint(transform.position);
        }
    }

    private IEnumerator Travel()
    {
        travelledAFrame = false;
        while (true)
        {
            transform.position += movement * speed * Time.deltaTime;

            if (Input.GetMouseButtonDown(0))
            {
                DoTargetedRedirect();
            }

            quantumTunnelElapsedCooldown += Time.deltaTime;
            targetedRedirectElapsedCooldown += Time.deltaTime;

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

        if (isChild || launchCount >= maxLaunchCount)
        {
            launchCount = 0;
            BeginRetracting();
        }
        else
        {
            hookLauncher.ReDangle(fromPosition: transform.position);
        }
    }

    public void BeginRetracting()
    {
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
                    Collider2D collider = info.hookedItem.GetComponent<Collider2D>();
                    if (collider != null)
                    {
                        HookedItemInfo newInfo = new HookedItemInfo()
                        {
                            hookedItem = info.hookedItem,
                            ropeRendererPointIndex = parentHook.currentMovingIndex,
                            hookedItemCollisionPoint = collider.ClosestPoint(parentHook.transform.position)
                        };

                        parentHook.hookedItems.Add(newInfo);

                        info.hookedItem.Retract(
                            parentHook.ropeRendererPoints,
                            newInfo,
                            retractionRate
                        );
                    }
                    else
                    {
                        Debug.LogWarning("Couldn't get a collider for the hooked item we're passing up to the parent!");
                    }
                }
            });
            Destroy(gameObject);
        }
        else
        {
            // This is the parent hook, and it's now fully retracted all the way to the collector
            childHooks.FindAll(info => info.childHook == null).ForEach(info => childHooks.Remove(info));
            ropeRendererPoints.Clear();
            DayNightSwitcher.instance.Night();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If we've hit a hookable that isn't already hooked...
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hookable") &&
            !collision.gameObject.GetComponent<Hookable>().isHooked &&
            launched
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

            hookedItemsThisLaunch += 1;

            // If we've reached the max hookable items, we stop
            if (hookedItemsThisLaunch >= maxHookedItems)
            {
                // This function also handles relaunching if we still have a quota for those left.
                StopTravelling();
            }
        }
    }
}
