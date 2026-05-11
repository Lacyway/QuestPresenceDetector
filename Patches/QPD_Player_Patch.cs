using System.Reflection;
using Comfort.Common;
using EFT;
using QuestPresenceDetector.Bundles;
using QuestPresenceDetector.Components;
using SPT.Reflection.Patching;
using UnityEngine;

namespace QuestPresenceDetector.Patches;

internal sealed class QPD_Player_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Player)
            .GetMethod(nameof(Player.method_83));
    }

    [PatchPostfix]
    public static void Postfix(Player __instance)
    {
        if (__instance.IsYourPlayer && Singleton<GameWorld>.Instance is not HideoutGameWorld)
        {
            var prefab = InternalBundleLoader.Instance.GetAsset();
            var newObj = GameObject.Instantiate(prefab);
            newObj.AddComponent<QPDInterface>();
        }
    }
}
