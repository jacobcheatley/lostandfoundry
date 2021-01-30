using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenShip : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer spriteRenderer;
    [SerializeField]
    Sprite[] stages;

    void Start()
    {
        ShipTracker.OnShipPieces += ShipTracker_OnShipPieces;
    }

    private void ShipTracker_OnShipPieces(int oldVal, int newVal)
    {
        spriteRenderer.sprite = stages[newVal];
    }
}
