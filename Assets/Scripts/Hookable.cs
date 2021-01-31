using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookable : Retractable
{
    public int value;
    [SerializeField]
    public bool isShipPart;
    [HideInInspector]
    public bool isHooked = false;

    public void Hooked(GameObject toWhom)
    {
        // TODO: Some fancy animations here or something.
        //  We want the specifics to be determined by the object type, which
        //  will be somewhere on this GameObject.
        isHooked = true;
        AudioController.PlayRandomSoundClip(SFX.HookGrab);
    }

    public void Collected(GameObject byWhom)
    {
        // TODO: Some more fancy animations
        AudioController.PlayRandomSoundClip(SFX.Coin);
        Destroy(gameObject);

        if (isShipPart)
        {
            Debug.Log($"Got ship piece!");
            ShipTracker.ShipPiecesCollected++;
        }
    }

    public void Retract(List<Vector3> alongLine, HookedItemInfo info, float withSpeed)
    {
        StartCoroutine(DoRetract(alongLine, info.hookedItemCollisionPoint, info.ropeRendererPointIndex, withSpeed));
    }

    protected override void FinishedRetracting()
    {
        isRetracting = false;
    }
}
