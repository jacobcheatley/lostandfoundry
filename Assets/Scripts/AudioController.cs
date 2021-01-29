using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    [SerializeField]
    private AudioMixer audioMixer;

    [SerializeField]
    private AudioMixerSnapshot[] snapshots;

    private static AudioController instance;

    private void Start()
    {
        instance = this;
    }

    public static void MoveToSnapshot(int snapshotIndex, float timeToReach)
    {
        instance.audioMixer.TransitionToSnapshots(new AudioMixerSnapshot[] { instance.snapshots[snapshotIndex] }, new float[] { 1 }, timeToReach);
    }
}
