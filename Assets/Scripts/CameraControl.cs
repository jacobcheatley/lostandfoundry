using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    private Transform following;

    private static CameraControl instance;
    private bool switching = false;

    void Start()
    {
        instance = this;
    }

    void Update()
    {
        if (!switching && following != null)
            transform.position = new Vector3(following.position.x, following.position.y, transform.position.z);
    }

    public static void Follow(Transform follow, float switchSpeedFactor = 2)
    {
        instance.StopAllCoroutines();
        instance.StartCoroutine(instance.MoveToFollowNew(follow, switchSpeedFactor));
    }

    private IEnumerator MoveToFollowNew(Transform follow, float switchSpeedFactor)
    {
        following = follow;
        switching = true;

        float currentSpeed = 0;

        while (true)
        {
            Vector2 distance = following.position - transform.position;
            if (distance.sqrMagnitude <= 0.1)
                break;

            currentSpeed += Time.deltaTime * switchSpeedFactor;
            Vector2 travelDistance = currentSpeed * distance.normalized; // Move the distance to the target each 1/switchSpeedFactor seconds - continuously - math is fun

            transform.position += new Vector3(travelDistance.x, travelDistance.y, 0);
            yield return null;
        }
        switching = false;
    }
}
