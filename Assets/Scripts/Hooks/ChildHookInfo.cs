using System.Collections;
using UnityEngine;

class ChildHookInfo
{
    public Hook childHook;
    public int ropeRendererIndex;

    public ChildHookInfo(Hook _childHook, int _ropeRendererIndex)
    {
        childHook = _childHook;
        ropeRendererIndex = _ropeRendererIndex;
    }

    public override string ToString() {
        return $"HookInfo({childHook.gameObject.name},{ropeRendererIndex})";
    }
}