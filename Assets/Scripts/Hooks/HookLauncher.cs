using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookLauncher : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private GameObject hook;
    [SerializeField]
    private GameObject hookPivot;

    [Header("Configuration")]
    [SerializeField]
    private float dangleAngle = 90;
    [SerializeField]
    private float dangleDistance = 0.5f;
    [SerializeField]
    private float dangleSpeed = 2;

    void Start()
    {
        StartCoroutine(DangleHook());
    }

    private IEnumerator DangleHook()
    {
        float time = 0;
        hook.transform.localPosition = Vector3.down * dangleDistance - Vector3.forward;
        while (true)
        {
            hookPivot.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(time * dangleSpeed) * dangleAngle / 2);
            if (Input.GetMouseButtonDown(0) && DayNightSwitcher.IsDay())
            {
                hook.GetComponent<Hook>().Launch();
                yield break;
            }
            time += Time.deltaTime;
            yield return null;
        }
    }
}
