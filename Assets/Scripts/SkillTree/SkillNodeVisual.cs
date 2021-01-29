using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Visual", menuName = "ScriptableObjects/SkillNodeVisual", order = 1)]
public class SkillNodeVisual : ScriptableObject
{
    public Color textColor;
    public Color costColor;
    public Color backgroundColor;
    public Color connectionColor;
    public bool displayTooltip;
    public bool displayCost;
    public bool glow;
}