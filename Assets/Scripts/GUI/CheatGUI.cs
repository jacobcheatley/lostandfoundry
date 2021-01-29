using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatGUI : MonoBehaviour
{
    public void GainMoney(int value)
    {
        ResourceTracker.Money += value;
    }
}
