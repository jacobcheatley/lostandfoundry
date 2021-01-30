using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillNode : MonoBehaviour
{
    private enum SkillNodeState
    {
        NotUnlockable,
        Unaffordable,
        Unlockable,
        Unlocked
    }

    [Header("Components")]
    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private TextMeshProUGUI costText;
    [SerializeField]
    private Image backgroundImage;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject connectionPrefab;

    [Header("Visual Settings")]
    [SerializeField]
    private SkillNodeVisual notUnlockableVisual;
    [SerializeField]
    private SkillNodeVisual notAffordableVisual;
    [SerializeField]
    private SkillNodeVisual unlockableVisual;
    [SerializeField]
    private SkillNodeVisual unlockedVisual;

    private SkillConnection skillConnection;

    private SkillID skillID;
    private Skill skill;

    private SkillNodeState state;
    private SkillNodeState State 
    { 
        get => state;
        set
        {
            switch (value)
            {
                case SkillNodeState.NotUnlockable:
                    SetVisual(notUnlockableVisual);
                    break;
                case SkillNodeState.Unaffordable:
                    SetVisual(notAffordableVisual);
                    break;
                case SkillNodeState.Unlockable:
                    SetVisual(unlockableVisual);
                    break;
                case SkillNodeState.Unlocked:
                    SetVisual(unlockedVisual);
                    break;
            }
            state = value;
        } 
    }

    public void Initialise(SkillID skillID, GameObject requiredNode)
    {
        this.skillID = skillID;
        this.skill = SkillTree.skills[this.skillID];

        if (requiredNode != null)
        {
            // Has a requirement - create the connection
            skillConnection = Instantiate(connectionPrefab, transform.parent).GetComponent<SkillConnection>();
            skillConnection.Initialise(requiredNode, gameObject);
            State = SkillNodeState.NotUnlockable;
        }
        else
        {
            // Does not have a requirement - set to be unlockable
            State = ResourceTracker.Money >= skill.cost ? SkillNodeState.Unlockable : SkillNodeState.Unaffordable;
        }

        SkillTracker.OnSkillChange += (oldSkills, newSkills) =>
        {
            SkillID addedSkill = newSkills ^ oldSkills;
            if (addedSkill == this.skillID)
                State = SkillNodeState.Unlocked;
            else if (addedSkill == this.skill.required)
                State = ResourceTracker.Money >= skill.cost ? SkillNodeState.Unlockable : SkillNodeState.Unaffordable;
        };

        ResourceTracker.OnMoneyChange += (oldMoney, newMoney) =>
        {
            switch (State)
            {
                case SkillNodeState.Unaffordable when newMoney >= skill.cost:
                    State = SkillNodeState.Unlockable;
                    break;
                case SkillNodeState.Unlockable when newMoney < skill.cost:
                    State = SkillNodeState.Unaffordable;
                    break;
            }
        };

        titleText.text = this.skill.title;
        costText.text = $"${this.skill.cost}";
    }

    private void SetVisual(SkillNodeVisual visual)
    {
        titleText.color = visual.textColor;
        costText.color = visual.costColor;
        backgroundImage.color = visual.backgroundColor;
        // TODO: Stop get component pls
        if (skillConnection != null)
            skillConnection.GetComponent<Image>().color = visual.connectionColor;
        costText.gameObject.SetActive(visual.displayCost);
        if (visual.glow)
            StartCoroutine(Glow());
        else
        {
            StopAllCoroutines();
            transform.localScale = Vector3.one;
        }
    }

    private IEnumerator Glow()
    {
        float glowSpeed = Random.Range(10/7f, 14/7f);
        float glowFactor = Random.Range(0.1f, 0.25f);
        float baseSizeIncrease = 1.3f;

        float duration = 0;
        while (true)
        {
            duration += Time.deltaTime;
            backgroundImage.transform.localScale = Vector3.one * baseSizeIncrease + Vector3.one * Mathf.Sin(duration * glowSpeed) * glowFactor;
            yield return null;
        }
    }

    public void Click()
    {
        switch (State)
        {
            case SkillNodeState.NotUnlockable:
                break;
            case SkillNodeState.Unaffordable:
                break;
            case SkillNodeState.Unlockable:
                SkillTracker.UnlockSkill(skillID);
                break;
            case SkillNodeState.Unlocked:
                break;
        }
        Debug.Log($"Now have {SkillTracker.UnlockedSkills:G} unlocked");
    }
}