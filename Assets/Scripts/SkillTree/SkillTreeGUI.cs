using System.Collections.Generic;
using UnityEngine;

public class SkillTreeGUI : MonoBehaviour
{
    [SerializeField]
    private GameObject skillNodePrefab;

    private float horizontalSpacing = 200;
    private float verticalSpacing = -80;

    private SkillTree skillTree;

    void Start()
    {
        skillTree = new SkillTree();
        Dictionary<SkillID, List<SkillID>> skillChildren = new Dictionary<SkillID, List<SkillID>>();
        List<SkillID> startingSkills = new List<SkillID>();

        foreach (var item in skillTree.skills)
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
            height = RecursivelyPlace(startingSkill, skillChildren, skillTree, 0, height, null);
        }
    }

    int RecursivelyPlace(SkillID skill, Dictionary<SkillID, List<SkillID>> skillChildren, SkillTree skillTree, int currentTier, int currentHeight, GameObject requiredNode)
    {
        // Place at currentTier, currentHeight
        Debug.Log($"{skill} - {currentTier} - {currentHeight}");
        SkillNode skillNode = GameObject.Instantiate(skillNodePrefab, transform).GetComponent<SkillNode>();
        skillNode.GetComponent<RectTransform>().anchoredPosition = new Vector2(currentTier * horizontalSpacing, currentHeight * verticalSpacing);
        skillNode.Initialise(skillTree[skill], requiredNode);

        if (!skillChildren.ContainsKey(skill))
            return currentHeight + 1;

        foreach (var childSkill in skillChildren[skill])
        {
            currentHeight = RecursivelyPlace(childSkill, skillChildren, skillTree, currentTier + 1, currentHeight, skillNode.gameObject);
        }

        return currentHeight;
    }
}