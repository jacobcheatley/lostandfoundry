using System.Collections.Generic;
using UnityEngine;

public class SkillTreeGUI : MonoBehaviour
{
    [SerializeField]
    private GameObject skillNodePrefab;

    private readonly float horizontalSpacing = 200;
    private readonly float verticalSpacing = -80;

    void Start()
    {
        GenerateTree();
    }

    void GenerateTree()
    {
        Dictionary<SkillID, List<SkillID>> skillChildren = new Dictionary<SkillID, List<SkillID>>();
        List<SkillID> startingSkills = new List<SkillID>();

        foreach (var item in SkillTree.skills)
        {
            SkillID parent = item.Value.required;
            SkillID child = item.Key;

            if (!skillChildren.ContainsKey(parent))
                skillChildren.Add(parent, new List<SkillID>());

            skillChildren[parent].Add(child);

            if (parent == SkillID.None)
                startingSkills.Add(child);
        }

        int height = 0;
        foreach (var startingSkill in startingSkills)
        {
            height = RecursivelyPlace(startingSkill, skillChildren, 0, height, null);
        }
    }

    int RecursivelyPlace(SkillID skillID, Dictionary<SkillID, List<SkillID>> skillChildren, int currentTier, int currentHeight, GameObject requiredNode)
    {
        // Place at currentTier, currentHeight
        Debug.Log($"{skillID} - {currentTier} - {currentHeight}");
        SkillNode skillNode = GameObject.Instantiate(skillNodePrefab, transform).GetComponent<SkillNode>();
        skillNode.GetComponent<RectTransform>().anchoredPosition = new Vector2(currentTier * horizontalSpacing, currentHeight * verticalSpacing);
        skillNode.Initialise(skillID, requiredNode);

        if (!skillChildren.ContainsKey(skillID))
            return currentHeight + 1;

        foreach (var childSkill in skillChildren[skillID])
        {
            currentHeight = RecursivelyPlace(childSkill, skillChildren, currentTier + 1, currentHeight, skillNode.gameObject);
        }

        return currentHeight;
    }
}