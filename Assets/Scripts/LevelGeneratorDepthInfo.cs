using System.Collections;
using UnityEngine;

[System.Serializable]
public struct LevelGeneratorDepthInfo
{
    [Range(0, 30)]
    public int chunkEnd;
    public HookableInfo[] hookables;
    [Range(0, 1)]
    public float density;
}

[System.Serializable]
public struct HookableInfo
{
    public GameObject hookablePrefab;
    public int weight;
    public bool unique;
}