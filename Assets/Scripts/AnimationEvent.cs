using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public void Footstep()
    {
        AudioController.PlayRandomSoundClip(SFX.Footstep, 0.5f);
    }

    public void Hammer()
    {
        AudioController.PlayRandomSoundClip(SFX.Anvil, 0.2f);
    }
}
