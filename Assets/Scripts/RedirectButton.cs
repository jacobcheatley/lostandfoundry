using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RedirectButton : MonoBehaviour
{
    [SerializeField]
    private Image image;

    void Start()
    {
        image.color = new Color(1, 1, 1, 0);
        SkillTracker.OnSkillChange += SkillTracker_OnSkillChange;
        HookLauncher.primaryHook.OnFinishedRetracting += PrimaryHook_OnFinishedRetracting;
        HookLauncher.primaryHook.OnRedirect += PrimaryHook_OnRedirect;
    }

    private void PrimaryHook_OnRedirect()
    {
        Debug.Log("REDIRTECT");
        StartCoroutine(WatchCooldown(HookLauncher.primaryHook));
    }

    private void PrimaryHook_OnFinishedRetracting()
    {
        StopAllCoroutines();
        image.fillAmount = 1;
    }

    private void SkillTracker_OnSkillChange(SkillID oldVal, SkillID newVal)
    {
        if ((newVal & SkillID.Redirect) == SkillID.Redirect)
            image.color = Color.white;
    }

    private IEnumerator WatchCooldown(Hook hook)
    {
        while (hook.targetedRedirectCooldownProgress < 0.99f)
        {
            image.fillAmount = hook.targetedRedirectCooldownProgress;
            yield return null;
        }
        image.fillAmount = 1f;
    }
}
