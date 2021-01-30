using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundConfiguration", menuName = "ScriptableObjects/SoundConfiguration", order = 1)]
public class SoundConfiguration : ScriptableObject
{
    [SerializeField]
    private SFXSounds[] sfxSounds;
    public Dictionary<SFX, AudioClip[]> ClipMap()
    {
        var result = new Dictionary<SFX, AudioClip[]>();
        foreach (SFXSounds sfxSound in sfxSounds)
            result.Add(sfxSound.sfx, sfxSound.clips);
        return result;
    }
}