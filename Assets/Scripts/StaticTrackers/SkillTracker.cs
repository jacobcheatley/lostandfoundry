using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkillTracker
{
    public delegate void OnSkillChangeDelegate(SkillID oldVal, SkillID newVal);
    public static event OnSkillChangeDelegate OnSkillChange;
    private static SkillID unlockedSkills = 0;
    public static SkillID UnlockedSkills { get => unlockedSkills; set { OnSkillChange?.Invoke(unlockedSkills, value); unlockedSkills = value; } }

    public static bool HasRequiredSkill(SkillID skillID)
    {
        return IsSkillUnlocked(SkillTree.skills[skillID].required);
    }

    public static bool CanAffordSkill(SkillID skillID)
    {
        return ResourceTracker.Money >= SkillTree.skills[skillID].cost;
    }

    public static bool CanUnlockSkill(SkillID skillID)
    {
        return HasRequiredSkill(skillID) && CanAffordSkill(skillID);
    }

    public static void UnlockSkill(SkillID skillID)
    {
        AudioController.PlayRandomSoundClip(SFX.Unlock, volume: 0.1f);
        ResourceTracker.Money -= SkillTree.skills[skillID].cost;
        UnlockedSkills |= skillID;
    }

    public static bool IsSkillUnlocked(SkillID skillID)
    {
        return (UnlockedSkills & skillID) == skillID;
    }
}
