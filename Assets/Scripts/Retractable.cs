using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Retractable : MonoBehaviour
{
    public bool isRetracting = false;
    protected bool temporarilyPauseRetracting = false;

    protected IEnumerator DoRetract(
        List<Vector3> alongLine, 
        Vector3 hookedItemCollisionPoint, 
        int ropeRendererPointIndex,
        float withSpeed
        )
    {
        isRetracting = true;

        Vector3 a = hookedItemCollisionPoint - transform.position;
        a.z = 0;

        int currentTargetIndex = ropeRendererPointIndex;
        while (true)
        {
            if (temporarilyPauseRetracting)
            {
                yield return null;
            }
            else
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
        FinishedRetracting();
    }

    protected abstract void FinishedRetracting();
}
