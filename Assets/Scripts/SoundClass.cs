using System.Collections;
using UnityEngine;

public enum SFX
{
    HookLaunch,
    Dawn,
    Coin,
    Quantum,
    Anvil,
    Unlock,
    Reel,
    Footstep,
    Nirn,
    HookGrab,
    Rumble,
    Happy
}

[System.Serializable]
public struct SFXSounds
{
    public SFX sfx;
    public AudioClip[] clips;
}