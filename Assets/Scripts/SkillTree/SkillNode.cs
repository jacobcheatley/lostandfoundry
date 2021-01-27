using System.Collections;
using UnityEngine;
using TMPro;

public class SkillNode : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private TextMeshProUGUI costText;
    [SerializeField]
    private GameObject connectionPrefab;

    public void Initialise(Skill skill, GameObject requiredNode)
    {
        titleText.text = skill.title;
        costText.text = $"${skill.cost}";
        if (requiredNode != null)
            Instantiate(connectionPrefab, transform.parent).GetComponent<SkillConnection>().Initialise(requiredNode, gameObject);
    }
}