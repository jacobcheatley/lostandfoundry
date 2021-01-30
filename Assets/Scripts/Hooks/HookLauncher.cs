﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookLauncher : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private GameObject hookPrefab;
    [SerializeField]
    private GameObject hookPivotPrefab;
    [SerializeField]
    private GameObject retractCameraAnchor;

    [Header("Configuration")]
    [SerializeField]
    private float dangleAngle = 90f;
    [SerializeField]
    private float dangleDistance = 0.5f;
    [SerializeField]
    private float dangleSpeed = 2f;

    [SerializeField]
    private float slowerDangleSpeed = 1f;

    private GameObject hook;
    private Hook hookHook;
    private GameObject baseHookPivot;

    void Start()
    {
        baseHookPivot = Instantiate(hookPivotPrefab, this.transform);
        hook = Instantiate(hookPrefab);
        hookHook = hook.GetComponent<Hook>();
        hookHook.hookPrefab = hookPrefab;
        hookHook.retractCameraAnchor = retractCameraAnchor;
        hookHook.hookLauncher = this;

        ReDangle();
    }

    public void ReDangle()
    {
        ReDangle(transform.position);
    }

    public void ReDangle(Vector3 fromPosition)
    {
        hook.transform.parent = baseHookPivot.transform;
        StartCoroutine(DangleHook(fromPosition));
    }

    private IEnumerator DangleHook(Vector3 fromPosition)
    {
        hookHook.BeginDangle();
        float time = 0;
        hook.transform.localPosition = Vector3.down * dangleDistance - Vector3.forward;
        while (true)
        {
            float dangleSpeedToUse = SkillTracker.IsSkillUnlocked(SkillID.SlowerSwingSpeed) ? slowerDangleSpeed : dangleSpeed;
            baseHookPivot.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(time * dangleSpeedToUse) * dangleAngle / 2);
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
