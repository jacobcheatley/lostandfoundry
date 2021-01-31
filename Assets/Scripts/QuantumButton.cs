using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class QuantumButton : MonoBehaviour
{
    [SerializeField]
    private Image image;

    void Start()
    {
        image.color = new Color(1, 1, 1, 0);
        SkillTracker.OnSkillChange += SkillTracker_OnSkillChange;
        HookLauncher.primaryHook.OnFinishedRetracting += PrimaryHook_OnFinishedRetracting;
    }

    private void PrimaryHook_OnFinishedRetracting()
    {
        StopAllCoroutines();
        image.fillAmount = 1;
    }

    private void SkillTracker_OnSkillChange(SkillID oldVal, SkillID newVal)
    {
        if ((newVal & SkillID.QuantumTunnel) == SkillID.QuantumTunnel)
            image.color = Color.white;
    }

    public void Click()
    {
        if (HookLauncher.primaryHook.quantumTunnelCooldownProgress >= 1f && DayNightSwitcher.IsDay())
        {
            HookLauncher.primaryHook.DoQuantumTunnel();
            StartCoroutine(WatchCooldown(HookLauncher.primaryHook));
        }
    }

    private IEnumerator WatchCooldown(Hook hook)
    {
        while (hook.quantumTunnelCooldownProgress < 0.99f)
        {
            image.fillAmount = hook.quantumTunnelCooldownProgress;
            yield return null;
        }
        image.fillAmount = 1f;
    }
}
