using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SkillConnection : MonoBehaviour
{
    [SerializeField]
    private RectTransform rectTransform;

    public void Initialise(GameObject from, GameObject to)
    {
        RectTransform fromRect = from.GetComponent<RectTransform>();
        RectTransform toRect = to.GetComponent<RectTransform>();
        Vector2 fromPosition = (Vector3)fromRect.anchoredPosition;
        Vector2 toPosition = (Vector3)toRect.anchoredPosition;
        rectTransform.anchoredPosition = (fromPosition + toPosition) / 2;
        Vector2 difference = toPosition - fromPosition;
        rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, difference.magnitude);
        transform.SetAsFirstSibling();
    }
}