using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookable : MonoBehaviour
{
    public int value;

    public void Hooked(GameObject toWhom)
    {
        // TODO: Some fancy animations here or something.
        //  We want the specifics to be determined by the object type, which
        //  will be somewhere on this GameObject.
        Debug.Log($"This {gameObject.name} got hooked to {toWhom.name}!");
    }

    public void Collected(GameObject byWhom)
    {
        Debug.Log($"This {gameObject.name} got collected by {byWhom.name}!");
        Destroy(gameObject);
    }

    public void Retract(List<Vector3> alongLine, HookedItemInfo info, float withSpeed)
    {
        StartCoroutine(DoRetract(alongLine, info, withSpeed));
    }

    private IEnumerator DoRetract(List<Vector3> alongLine, HookedItemInfo info, float withSpeed)
    {
        Vector3 a = (Vector3)info.hookedItemCollisionPoint - transform.position;
        a.z = 0;

        int currentTargetIndex = info.ropeRendererPointIndex;
        while (true)
        {
            Vector3 target = alongLine[currentTargetIndex];
            target.z = transform.position.z;

            Vector3 distanceToTarget = target - (transform.position + a);
            transform.position += distanceToTarget.normalized * withSpeed * Time.deltaTime;

            // If we're close to the target
            if (distanceToTarget.sqrMagnitude <= (withSpeed * Time.deltaTime) * (withSpeed * Time.deltaTime))
            {
                // Switch target
                currentTargetIndex -= 1;
                if (currentTargetIndex < 0)
                {
                    break;
                }
            }

            yield return null;
        }
    }
}
