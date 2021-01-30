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

    [SerializeField]
    private AudioSource sfxSource;

    [SerializeField]
    private AudioClip[] testSounds;

    [SerializeField]
    private SoundConfiguration soundConfiguration;

    [SerializeField]
    private Dictionary<SFX, AudioClip[]> clipMap;

    [SerializeField]
    private LevelGenerator levelGenerator;

    private static AudioController instance;
    private int currentMaxY = 0;

    private void Start()
    {
        instance = this;
        clipMap = soundConfiguration.ClipMap();
        StartCoroutine(WaitForInput());
    }

    public static void StartDepthAudio()
    {
        instance.currentMaxY = 0;
        instance.levelGenerator.OnChunkPlaced += instance.HandlePlacementEvent;
        MoveToSnapshot(2, 4f);
    }

    public static void EndDepthAudio()
    {
        instance.levelGenerator.OnChunkPlaced -= instance.HandlePlacementEvent;
        MoveToSnapshot(0, 4f);
    }

    private void HandlePlacementEvent(Vector2 chunkCentre, int x, int y, int depthIndex, bool deepestSinceRegen, bool initialGen)
    {
        if (deepestSinceRegen && !initialGen && y > currentMaxY)
        {
            currentMaxY = y;
            MoveToSnapshot(depthIndex + 2, 2f);
        }
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
    }

    public static void MoveToSnapshot(int snapshotIndex, float timeToReach)
    {
        instance.audioMixer.TransitionToSnapshots(new AudioMixerSnapshot[] { instance.snapshots[snapshotIndex] }, new float[] { 1 }, timeToReach);
    }

    private static void PlayRandomSoundClip(AudioClip[] clips, float volume = 1f)
    {
        instance.sfxSource.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
    }

    public static void PlayRandomSoundClip(SFX sfx, float volume = 1f)
    {
        PlayRandomSoundClip(instance.clipMap[sfx], volume);
    }
}
