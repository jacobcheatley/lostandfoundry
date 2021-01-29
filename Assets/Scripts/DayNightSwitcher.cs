﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class DayNightSwitcher : MonoBehaviour
{
    [Header("Anchors")]
    [SerializeField]
    private Transform cameraAnchorDay;
    [SerializeField]
    private Transform cameraAnchorNight;

    [Header("Controlled objects")]
    [SerializeField]
    private GameObject starCanvas;
    [SerializeField]
    private CanvasGroup starCanvasGroup;

    [Header("Properties and settings")]
    [SerializeField]
    private float starFadeTime = 4f;
    [SerializeField]
    private Color nightBackground;
    [SerializeField]
    private Color dayBackground;

    private Camera mainCamera;
    private bool day = true;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (day)
                Night();
            else
                Day();
        }
    }

    public void Day()
    {
        day = true;
        Debug.Log("Day");
        StartCoroutine(FadeStars(false));
        CameraControl.Follow(cameraAnchorDay, 0.3f);
    }
    
    public void Night()
    {
        day = false;
        Debug.Log("Night");
        StartCoroutine(FadeStars(true));
        CameraControl.Follow(cameraAnchorNight, 0.3f);
    }

    private void DelayFunction(Action function, float waitTime)
    {
        StartCoroutine(DelayFunctionCoroutine(function, waitTime));
    }

    private IEnumerator DelayFunctionCoroutine(Action function, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        function.Invoke();
    }

    private IEnumerator FadeStars(bool visible)
    {
        starCanvas.SetActive(true);
        starCanvasGroup.alpha = visible ? 0 : 1;
        float elapsedTime = 0;
        while (elapsedTime < starFadeTime)
        {
            elapsedTime += Time.deltaTime;
            float fraction = elapsedTime / starFadeTime;
            fraction = visible ? fraction : 1 - fraction;
            starCanvasGroup.alpha = fraction;
            mainCamera.backgroundColor = Color.Lerp(dayBackground, nightBackground, fraction);
            yield return null;
        }
        starCanvasGroup.alpha = visible ? 1 : 0;
        starCanvas.SetActive(visible);
    }
}
