using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hookable"))
        {
            Hookable other = collision.GetComponent<Hookable>();
            other.Collected(gameObject);
            float multiplier = 1f * 
                (SkillTracker.IsSkillUnlocked(SkillID.Value1) ? 5f / 4 : 1) * 
                (SkillTracker.IsSkillUnlocked(SkillID.Value2) ? 5f / 4 : 1) * 
                (SkillTracker.IsSkillUnlocked(SkillID.Value3) ? 5f / 4 : 1);
            ResourceTracker.Money += (int)(other.value * multiplier);
        }
    }
}
