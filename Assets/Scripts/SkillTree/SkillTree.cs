using System;
using System.Collections.Generic;

[Flags]
public enum SkillID
{
    None = 0,
    Free = 1,
    Tier1 = 2,
    Tier2 = 4,
    Tier3 = 8,
    Tier4 = 16,
    Branch1 = 32,
    Branch2 = 64,
    Branch11 = 128,
    Branch12 = 256,
    Branch21 = 512,
    Branch22 = 1024,
    Tier4A = 2048,
    Tier4B = 4096,
    Tier4C = 8192,
    Branch11X = 16384,
    Branch12X = 16384 * 2,
    Branch21X = 16384 * 4,
    Branch22X = 16384 * 8,
    Split3 = 16384 * 16
}

public class Skill
{
    public SkillID required;
    public string title;
    public string description;
    public string imageResource;
    public int cost;

    public Skill(SkillID required, string title, string description, string imageResource, int cost)
    {
        this.required = required;
        this.title = title;
        this.description = description;
        this.imageResource = imageResource;
        this.cost = cost;
    }
}

public static class SkillTree
{
    public static readonly Dictionary<SkillID, Skill> skills = new Dictionary<SkillID, Skill> {
        { SkillID.Free, new Skill(SkillID.None, "Free skill", "Free skill! Amazing", "", 0) },
        { SkillID.Branch1, new Skill(SkillID.Free, "Branch 1", "Description", "", 15) },
        { SkillID.Branch11, new Skill(SkillID.Branch1, "Branch 1-1", "Description", "", 30) },
        { SkillID.Branch12, new Skill(SkillID.Branch1, "Branch 1-2", "Description", "", 30) },
        { SkillID.Branch2, new Skill(SkillID.Free, "Branch 2", "Description", "", 15) },
        { SkillID.Branch21, new Skill(SkillID.Branch2, "Branch 2-1", "Description", "", 30) },
        { SkillID.Branch22, new Skill(SkillID.Branch2, "Branch 2-2", "Description", "", 30) },
        { SkillID.Tier1, new Skill(SkillID.None, "T1", "Description", "", 10) },
        { SkillID.Tier2, new Skill(SkillID.Tier1, "T2", "Description", "", 10) },
        { SkillID.Tier3, new Skill(SkillID.Tier2, "T3", "Description", "", 10) },
        { SkillID.Tier4, new Skill(SkillID.Tier3, "T4", "Description", "", 10) },
        { SkillID.Tier4A, new Skill(SkillID.Tier3, "T4A", "Description", "", 10) },
        { SkillID.Tier4B, new Skill(SkillID.Tier3, "T4B", "Description", "", 10) },
        { SkillID.Tier4C, new Skill(SkillID.Tier4, "T4C", "Description", "", 10) },
        { SkillID.Split3, new Skill(SkillID.Tier2, "Three-Way Split", "Hook splits into three new hooks whenever an item is grabbed", "", 10) },
        { SkillID.Branch11X, new Skill(SkillID.Branch11, "Branch 11X", "Description", "", 150) },
        { SkillID.Branch12X, new Skill(SkillID.Branch12, "Branch 12X", "Description", "", 150) },
        { SkillID.Branch21X, new Skill(SkillID.Branch21, "Branch 21X", "Description", "", 150) },
        { SkillID.Branch22X, new Skill(SkillID.Branch22, "Branch 22X", "Description", "", 150) },
    };
}
