﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelGenerator : MonoBehaviour
{
    public delegate void OnChunkPlacedDelegate(Vector2 chunkCentre, int x, int y, int depthIndex, bool deepestSinceRegen, bool initialGen);
    public event OnChunkPlacedDelegate OnChunkPlaced;

    // Hahaha, private variables are for chumps. I just want to grab a list of all the generated objects :)
    [SerializeField]
    public Transform levelParent;

    [SerializeField]
    private LevelGeneratorDepthInfo[] depthInfos;

    [SerializeField]
    private Transform backgroundParent;

    [SerializeField]
    private Camera camera;

    [SerializeField]
    private GameObject[] shipPieces;
    [SerializeField]
    private int maxCentreChunkOffset = 2;
    private int[] shipLocations = new int[] { 0, 1, 2, 3, 4 };
    private int[] chosenChunk;

    private int initialChunkWidth = 3;
    private int maxChunkWidth = 20;
    private int maxDepth;
    private Vector2 chunkSpacing = new Vector2(10, -10);
    private Vector2 initialChunkOffset = new Vector2(0, -6.75f);
    private int numSpots = 3;
    private Vector2 spotSpacing;
    private float spotWiggle;
    private Dictionary<int, List<HookableInfo>> objectWeights;
    // What's that? Public singletons are a bad thing? But this is a game jam!
    public static LevelGenerator instance;
    private int currentMaxY = 0;
    private bool[,] generatedChunks;

    public Vector2 valuableTargetLocation;

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
        maxDepth = depthInfos.Last().chunkEnd;
    }

    private void Start()
    {
        instance = this;
        PlaceGround();
        GenerateNew();
    }

    private void PlaceGround()
    {
        for (int x = -maxChunkWidth; x <= maxChunkWidth; x++)
        {
            for (int y = 0; y < maxDepth; y++)
            {
                Vector2 chunkCentre = ChunkCentre(x, y);
                LevelGeneratorDepthInfo depthInfo = WhereAmIAt(y).Item2;
                GameObject chunkBg = Instantiate(depthInfo.backgroundPrefab, chunkCentre, Quaternion.identity, backgroundParent);
                chunkBg.transform.localPosition = new Vector3(chunkBg.transform.position.x, chunkBg.transform.position.y, 2);
            }
        }
    }

    private System.Tuple<int, LevelGeneratorDepthInfo> WhereAmIAt(int y)
    {
        LevelGeneratorDepthInfo depthInfo = depthInfos[0];
        int depthInfoIndex;
        for (depthInfoIndex = 0; depthInfoIndex < depthInfos.Length; depthInfoIndex++)
        {
            if (y < depthInfos[depthInfoIndex].chunkEnd)
            {
                depthInfo = depthInfos[depthInfoIndex];
                break;
            }
        }
        return new System.Tuple<int, LevelGeneratorDepthInfo>(depthInfoIndex, depthInfo);
    }

    public static void GenerateNew()
    {
        // Ship part location
        int shipDepth = instance.shipLocations[ShipTracker.ShipPiecesCollected];
        Debug.Log($"Placing at depth {shipDepth}");
        int maxDepth = instance.depthInfos[shipDepth].chunkEnd - 1;
        int minDepth = shipDepth == 0 ? 0 : instance.depthInfos[shipDepth - 1].chunkEnd;
        instance.chosenChunk = new int[] { Random.Range(-instance.maxCentreChunkOffset, instance.maxCentreChunkOffset), Random.Range(minDepth, maxDepth) };

        instance.valuableTargetLocation = instance.ChunkCentre(instance.chosenChunk[0], instance.chosenChunk[1]);

        // More things
        instance.generatedChunks = new bool[2 * instance.maxChunkWidth - 1, instance.maxDepth]; // -maxChunkWidth to maxChunkWidth inclusive, 0 to maxDepth inclusive
        instance.currentMaxY = 0;
        for (int x = -instance.initialChunkWidth; x <= instance.initialChunkWidth; x++)
        {
            for (int y = 0; y < instance.depthInfos[0].chunkEnd; y++)
                instance.GenerateChunk(x, y, true);
        }
    }

    private Vector2 ChunkCentre(int x, int y)
    {
        return initialChunkOffset + new Vector2(x * chunkSpacing.x, y * chunkSpacing.y);
    }

    private System.Tuple<int, int> InverseChunkCentre(Vector2 chunkCentre)
    {
        int x = (int)((chunkCentre.x - initialChunkOffset.x) / chunkSpacing.x);
        int y = (int)((chunkCentre.y - initialChunkOffset.y) / chunkSpacing.y);
        return new System.Tuple<int, int>(x, y);
    }

    public static void PutisSpencerHere(Vector2 origin, float radius)
    {
        float jumpDistance = instance.chunkSpacing.x / 2f;

        float xSearch = -radius;
        while (xSearch < radius)
        {
            xSearch += jumpDistance;
            float ySearch = -radius;
            while (ySearch < radius)
            {
                ySearch += jumpDistance;
                System.Tuple<int, int> inverseCentre = instance.InverseChunkCentre(origin + new Vector2(xSearch, ySearch));
                instance.GenerateChunk(inverseCentre.Item1, inverseCentre.Item2);
            }
        }
    }

    private void GenerateChunk(int x, int y, bool initial=false)
    {
        if (x + maxChunkWidth < 0 || x + maxChunkWidth >= generatedChunks.GetLength(0) || y < 0 || y >= generatedChunks.GetLength(1))
            return;
        if (generatedChunks[x + maxChunkWidth, y])
            return;

        generatedChunks[x + maxChunkWidth, y] = true;

        System.Tuple<int, LevelGeneratorDepthInfo> d = WhereAmIAt(y);
        int depthInfoIndex = d.Item1;
        LevelGeneratorDepthInfo depthInfo = d.Item2;

        List<HookableInfo> weightedHookablesThisChunk = objectWeights[depthInfo.chunkEnd];
        Vector2 chunkCentre = ChunkCentre(x, y);
        Vector2 spotStart = chunkCentre - chunkSpacing / 2f + spotSpacing / 2f;

        OnChunkPlaced?.Invoke(chunkCentre, x, y, depthInfoIndex, y > currentMaxY, initial);
        currentMaxY = y > currentMaxY ? y : currentMaxY;

        bool dontCentrePlace = false;
        if (x == chosenChunk[0] && y == chosenChunk[1])
        {
            dontCentrePlace = true;
            Debug.Log($"Placing ship piece at {x}, {y}");
            GameObject shipPiece = shipPieces[ShipTracker.ShipPiecesCollected];
            Instantiate(shipPiece, (Vector3)chunkCentre + Vector3.forward, Quaternion.identity, levelParent);
        }

        for (int xx = 0; xx < numSpots; xx++)
        {
            for (int yy = 0; yy < numSpots; yy++)
            {
                if (dontCentrePlace && xx == yy && xx == numSpots / 2)
                    continue;
                if (Random.value <= depthInfo.density)
                {
                    Vector2 targetLocation = spotStart + new Vector2(xx * spotSpacing.x, yy * spotSpacing.y) + Random.insideUnitCircle * spotWiggle;
                    HookableInfo hookableInfo = weightedHookablesThisChunk[Random.Range(0, weightedHookablesThisChunk.Count)];
                    Instantiate(hookableInfo.hookablePrefab, (Vector3)targetLocation + Vector3.forward, Quaternion.Euler(0, 0, Random.Range(0, 360)), levelParent);
                }
            }
        }
    }

    public static void Clear()
    {
        foreach (Transform child in instance.levelParent)
            Destroy(child.gameObject);
    }
}
