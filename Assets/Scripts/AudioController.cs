﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    [SerializeField]
    private AudioMixer audioMixer;

    [SerializeField]
    private AudioMixerSnapshot[] snapshots;

    [SerializeField]
    private AudioSource sfxSource;

    [SerializeField]
    private AudioClip[] testSounds;

    private static AudioController instance;

    private void Start()
    {
        instance = this;
        StartCoroutine(WaitForInput());
    }

    private IEnumerator WaitForInput()
    {
        while (true)
        {
            if (Input.GetMouseButton(0))
            {
                yield return null;
                StartPlaying();
                break;
            }
            yield return null;
        }
    }

    private void StartPlaying()
    {
        foreach (var audioSource in GetComponents<AudioSource>())
        {
            if (audioSource != sfxSource)
                audioSource.Play();
        }
        PlayRandomSoundClip(testSounds);
    }

    public static void MoveToSnapshot(int snapshotIndex, float timeToReach)
    {
        instance.audioMixer.TransitionToSnapshots(new AudioMixerSnapshot[] { instance.snapshots[snapshotIndex] }, new float[] { 1 }, timeToReach);
    }

    private static void PlayRandomSoundClip(AudioClip[] clips)
    {
        instance.sfxSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}
