using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    private Transform levelParent;

    [SerializeField]
    private LevelGeneratorDepthInfo[] depthInfos;

    [SerializeField]
    private GameObject background;
    [SerializeField]
    private Transform backgroundParent;

    private int initialChunkWidth = 3;
    private Vector2 chunkSpacing = new Vector2(10, -10);
    private Vector2 initialChunkOffset = new Vector2(0, -6.5f);
    private int numSpots = 3;
    private Vector2 spotSpacing;
    private float spotWiggle;
    private Dictionary<int, List<HookableInfo>> objectWeights;

    private void Awake()
    {
        spotSpacing = chunkSpacing / numSpots;
        spotWiggle = spotSpacing.x / 4;

        objectWeights = new Dictionary<int, List<HookableInfo>>();
        foreach (LevelGeneratorDepthInfo depthInfo in depthInfos)
        {
            List<HookableInfo> weighted = new List<HookableInfo>();
            foreach (HookableInfo hookableInfo in depthInfo.hookables)
            {
                for (int i = 0; i < hookableInfo.weight; i++)
                    weighted.Add(hookableInfo);
            }
            objectWeights.Add(depthInfo.chunkEnd, weighted);
        }
    }

    private void Start()
    {
        GenerateNew();
    }

    void GenerateNew()
    {
        for (int x = -initialChunkWidth; x <= initialChunkWidth; x++)
        {
            for (int y = 0; y < depthInfos[0].chunkEnd; y++)
                GenerateChunk(x, y);
        }
    }

    void GenerateChunk(int x, int y)
    {
        Debug.Log($"Generate {x}, {y}");
        LevelGeneratorDepthInfo depthInfo = depthInfos.First(d => y < d.chunkEnd);
        List<HookableInfo> weightedHookablesThisChunk = objectWeights[depthInfo.chunkEnd];
        Vector2 chunkCentre = initialChunkOffset + new Vector2(x * chunkSpacing.x, y * chunkSpacing.y);
        Debug.Log(chunkCentre);
        Vector2 spotStart = chunkCentre - chunkSpacing / 2f + spotSpacing / 2f;
        Debug.Log(spotStart);
        GameObject chunkBg = Instantiate(background, chunkCentre, Quaternion.identity, backgroundParent);
        chunkBg.transform.localPosition = new Vector3(chunkBg.transform.position.x, chunkBg.transform.position.y, 0);
        for (int xx = 0; xx < numSpots; xx++)
        {
            for (int yy = 0; yy < numSpots; yy++)
            {
                if (Random.value <= depthInfo.density)
                {
                    Vector2 targetLocation = spotStart + new Vector2(xx * spotSpacing.x, yy * spotSpacing.y) + Random.insideUnitCircle * spotWiggle;
                    HookableInfo hookableInfo = weightedHookablesThisChunk[Random.Range(0, weightedHookablesThisChunk.Count)];
                    Instantiate(hookableInfo.hookablePrefab, targetLocation, Quaternion.Euler(0, 0, Random.Range(0, 360)), levelParent);
                }
            }
        }
    }

    void Clear()
    {
        foreach (Transform child in levelParent)
            Destroy(child.gameObject);
    }
}
