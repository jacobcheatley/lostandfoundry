using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValuableItemIndicator : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer sprite;

    private void Update()
    {
        if (SkillTracker.IsSkillUnlocked(SkillID.ValuablesRadar))
        {
            sprite.enabled = true;
            Vector3 target = LevelGenerator.instance.valuableTargetLocation;
            Vector3 from = transform.parent.position;
            Vector3 to = target - from;
            float angle = Mathf.Atan2(to.y, to.x);

            transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        }
        else
        {
            sprite.enabled = false;
        }
    }
}
