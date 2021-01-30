using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Retractable : MonoBehaviour
{
    [Header("Retract")]
    public bool isRetracting = false;
    [SerializeField]
    protected bool temporarilyPauseRetracting = false;
    [SerializeField]
    private List<Vector3> retractingAlongLine;

    protected IEnumerator DoRetract(
        List<Vector3> alongLine, 
        Vector3 hookedItemCollisionPoint, 
        int ropeRendererPointIndex,
        float withSpeed
        )
    {
        isRetracting = true;
        retractingAlongLine = alongLine;

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
