using System.Collections;
using UnityEngine;

public static class ShipTracker
{
    public delegate void OnShipPiecesChangeDelegate(int oldVal, int newVal);
    public static event OnShipPiecesChangeDelegate OnShipPieces;
    private static int shipPiecesCollected = 0;
    public static int ShipPiecesCollected { get => shipPiecesCollected; set { OnShipPieces?.Invoke(shipPiecesCollected, value); shipPiecesCollected = value; } }
}