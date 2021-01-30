using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnchorDayUpgrades : MonoBehaviour
{
    void Start()
    {
        SkillTracker.OnSkillChange += SkillTracker_OnSkillChange;
    }

    private void SkillTracker_OnSkillChange(SkillID oldVal, SkillID newVal)
    {
        int bonus = 0;
        if ((oldVal & SkillID.CameraSize1) != SkillID.CameraSize1 &&
            (newVal & SkillID.CameraSize1) == SkillID.CameraSize1)
        {
            bonus += 1;
        }
        if ((oldVal & SkillID.CameraSize2) != SkillID.CameraSize2 &&
            (newVal & SkillID.CameraSize2) == SkillID.CameraSize2)
        {
            bonus += 1;
        }
        DayNightSwitcher.instance.cameraSizeDayUpgradeBonus += bonus;
        transform.position += Vector3.down * bonus;
    }
}
