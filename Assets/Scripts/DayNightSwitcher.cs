using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightSwitcher : MonoBehaviour
{
    [SerializeField]
    private Transform cameraAnchorDay;
    [SerializeField]
    private Transform cameraAnchorNight;

    private bool day = true;

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
        CameraControl.Follow(cameraAnchorDay);
        day = true;
    }

    public void Night()
    {
        CameraControl.Follow(cameraAnchorNight);
        day = false;
    }
}
