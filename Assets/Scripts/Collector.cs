using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collector : MonoBehaviour
{
    [SerializeField]
    private float valueMultiplierPerLevel = 1.25f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hookable"))
        {
            Hookable other = collision.GetComponent<Hookable>();
            other.Collected(gameObject);
            float value = other.value;

            new List<bool>()
            {
                SkillTracker.IsSkillUnlocked(SkillID.Value1),
                SkillTracker.IsSkillUnlocked(SkillID.Value2),
                SkillTracker.IsSkillUnlocked(SkillID.Value3),
            }.FindAll(u => u).ForEach(u => value *= valueMultiplierPerLevel);

            ResourceTracker.Money += (int)value;

            if (other.isShipPart)
                AudioController.PlayRandomSoundClip(SFX.Happy, 0.2f);
        }
    }
}
