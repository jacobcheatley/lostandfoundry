using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    private Transform following;

    private static CameraControl instance;
    private bool switching = false;
    private Camera myCamera;

    void Start()
    {
        instance = this;
        myCamera = GetComponent<Camera>();
    }

    void Update()
    {
        if (!switching && following != null)
            transform.position = new Vector3(following.position.x, following.position.y, transform.position.z);
    }

    public static void Follow(Transform follow, float timeFrame = 2, float delay = 0)
    {
        instance.StopAllCoroutines();
        // this makes the camera stop if a delay is provided, but we only do that when the
        // hook begins retracting and it still looks good there ;)
        instance.following = null;
        instance.StartCoroutine(instance.MoveToFollowNew(follow, timeFrame, delay));
    }

    public static void SetHeight(float height, float timeFrame, float delay = 0)
    {
        instance.StartCoroutine(instance.SetHeightCoroutine(height, timeFrame, delay));
    }

    private IEnumerator SetHeightCoroutine(float height, float timeFrame, float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        float elapsedTime = 0;
        while (elapsedTime < timeFrame)
        {
            elapsedTime += Time.deltaTime;
            myCamera.orthographicSize = Mathf.Lerp(instance.myCamera.orthographicSize, height, Mathf.SmoothStep(0, 1, elapsedTime / timeFrame));
            yield return null;
        }
    }

    private IEnumerator MoveToFollowNew(Transform follow, float timeFrame, float delay)
    {
        yield return new WaitForSeconds(delay);

        following = follow;
        switching = true;

        float elapsedTime = 0;
        while (elapsedTime < timeFrame)
        {
            elapsedTime += Time.deltaTime;
            Vector2 newPosition = Vector2.Lerp(transform.position, follow.position, Mathf.SmoothStep(0, 1, elapsedTime / timeFrame));
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
            yield return null;
        }
        switching = false;
    }
}
