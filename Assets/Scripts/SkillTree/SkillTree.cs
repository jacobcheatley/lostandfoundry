using System;
using System.Collections.Generic;

[Flags]
public enum SkillID
{
    None = 0,
    HookDuration1 = 1,
    HookDuration2 = 2,
    HookDuration3 = 4,
    HookDuration4 = 8,
    CameraSize1 = 16,
    CameraSize2 = 32,
    Value1 = 64,
    Value2 = 128,
    Value3 = 256,
    HookSpeed1 = 512,
    HookSpeed2 = 1024,
    HookSpeed3 = 2048,
    HookSpeed4 = 4096,
    LaunchAgain1 = 8192,
    LaunchAgain2 = 8192 * 2,
    LaunchAgain3 = 8192 * 4,
    Pierce1 = 8192 * 8,
    Pierce2 = 8192 * 16,
    Pierce3 = 8192 * 32,
    Pierce4 = 8192 * 64,
    TripleShot = 8192 * 128,
    PentaShot = 8192 * 256,
    SeptaShot = 8192 * 512,
    ValuablesRadar = 8192 * 1024,
    SlowerSwingSpeed = 8192 * 2048,
    TargetedLaunch = 8192 * 4096,
    Redirect = 8192 * 8192,
    HomingExtraShots = 8192 * 8192 * 2,
    QuantumTunnel = 8192 * 8192 * 4,
    WaterImmunity = 8192 * 8192 * 8,
    MagmaImmunity = 8192 * 8192 * 16  // Any more and we overflow :)
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
        { SkillID.HookDuration1, new Skill(SkillID.None, "Hook Duration +1", "TODO DESC", "", 150) },
        { SkillID.HookDuration2, new Skill(SkillID.HookDuration1, "Hook Duration +1", "TODO DESC", "", 500) },
        { SkillID.HookDuration3, new Skill(SkillID.HookDuration2, "Hook Duration +1", "TODO DESC", "", 1500) },
        { SkillID.HookDuration4, new Skill(SkillID.None, "Hook Duration +1", "TODO DESC", "", 1000) },
        { SkillID.CameraSize1, new Skill(SkillID.HookDuration2, "Camera Size +1", "TODO DESC", "", 1000) },
        { SkillID.CameraSize2, new Skill(SkillID.HookDuration3, "Camera Size +1", "TODO DESC", "", 2000) },
        { SkillID.Value1, new Skill(SkillID.ValuablesRadar, "Value +25%", "TODO DESC", "", 4000) },
        { SkillID.Value2, new Skill(SkillID.Value1, "Value +25%", "TODO DESC", "", 8000) },
        { SkillID.Value3, new Skill(SkillID.None, "Value +25%", "TODO DESC", "", 500) },
        { SkillID.HookSpeed1, new Skill(SkillID.None, "Hook Speed +1", "TODO DESC", "", 150) },
        { SkillID.HookSpeed2, new Skill(SkillID.LaunchAgain1, "Hook Speed +1", "TODO DESC", "", 800) },
        { SkillID.HookSpeed3, new Skill(SkillID.TripleShot, "Hook Speed +1", "TODO DESC", "", 1500) },
        { SkillID.HookSpeed4, new Skill(SkillID.None, "Hook Speed +1", "TODO DESC", "", 1000) },
        { SkillID.LaunchAgain1, new Skill(SkillID.HookSpeed1, "Launch Again", "TODO DESC", "", 500) },
        { SkillID.LaunchAgain2, new Skill(SkillID.LaunchAgain1, "Launch Again Again", "TODO DESC", "", 3000) },
        { SkillID.LaunchAgain3, new Skill(SkillID.LaunchAgain2, "Launch Again Again Again", "TODO DESC", "", 10000) },
        { SkillID.Pierce1, new Skill(SkillID.None, "Pierce +1", "TODO DESC", "", 200) },
        { SkillID.Pierce2, new Skill(SkillID.Pierce1, "Pierce +1", "TODO DESC", "", 800) },
        { SkillID.Pierce3, new Skill(SkillID.Pierce2, "Pierce +1", "TODO DESC", "", 3000) },
        { SkillID.Pierce4, new Skill(SkillID.Pierce3, "Pierce +1", "TODO DESC", "", 8000) },
        { SkillID.TripleShot, new Skill(SkillID.HookSpeed1, "Triple Shot", "TODO DESC", "", 1500) },
        { SkillID.PentaShot, new Skill(SkillID.TripleShot, "Penta Shot", "TODO DESC", "", 6000) },
        { SkillID.SeptaShot, new Skill(SkillID.PentaShot, "Septa Shot", "TODO DESC", "", 10000) },
        { SkillID.ValuablesRadar, new Skill(SkillID.HookDuration2, "Valuables Radar", "TODO DESC", "", 2000) },
        { SkillID.SlowerSwingSpeed, new Skill(SkillID.ValuablesRadar, "Slower Swing Speed", "TODO DESC", "", 3000) },
        { SkillID.TargetedLaunch, new Skill(SkillID.SlowerSwingSpeed, "Targeted Launch", "TODO DESC", "", 5000) },
        { SkillID.Redirect, new Skill(SkillID.ValuablesRadar, "Targeted Redirect", "TODO DESC", "", 5000) },
        { SkillID.HomingExtraShots, new Skill(SkillID.HookSpeed3, "Homing Extra Shots", "TODO DESC", "", 15000) },
        { SkillID.QuantumTunnel, new Skill(SkillID.Pierce2, "Quantum Tunnel", "TODO DESC", "", 2500) },
        //{ SkillID.WaterImmunity, new Skill(SkillID.Pierce2, "Water Immunity", "TODO DESC", "", 300) }, // UNIMPLEMENTED
        //{ SkillID.MagmaImmunity, new Skill(SkillID.Pierce3, "Magma Immunity", "TODO DESC", "", 1000) } // UNIMPLEMENTED
    };
}
