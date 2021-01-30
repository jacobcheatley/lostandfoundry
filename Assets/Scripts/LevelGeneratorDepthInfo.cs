using System.Collections;
using UnityEngine;

[System.Serializable]
public struct LevelGeneratorDepthInfo
{
    [Range(0, 60)]
    public int chunkEnd;
    public HookableInfo[] hookables;
    [Range(0, 1)]
    public float density;
    public GameObject backgroundPrefab;
}

[System.Serializable]
public struct HookableInfo
{
    public GameObject hookablePrefab;
    public int weight;
    public bool unique;
}