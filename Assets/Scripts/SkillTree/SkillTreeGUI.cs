using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeGUI : MonoBehaviour
{
    [SerializeField]
    private GameObject skillNodePrefab;
    [SerializeField]
    private GameObject starPointParent;

    void Start()
    {
        Dictionary<SkillID, GameObject> skillNodes = new Dictionary<SkillID, GameObject>();

        foreach (Transform child in starPointParent.transform)
        {
            Debug.Log(child.name);
            skillNodes[(SkillID)Enum.Parse(typeof(SkillID), child.name)] = Instantiate(skillNodePrefab, child.transform.position, Quaternion.identity, transform);
        }

        starPointParent.SetActive(false);

        foreach (var item in skillNodes)
        {
            Skill skill = SkillTree.skills[item.Key];
            SkillID parentSkill = skill.required;
            SkillNode skillNode = item.Value.GetComponent<SkillNode>();
            skillNode.Initialise(item.Key, parentSkill == SkillID.None ? null : skillNodes[parentSkill]);
        }
    }
}