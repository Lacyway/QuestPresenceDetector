using System.Reflection;
using EFT.Interactive;
using EFT.InventoryLogic;
using QuestPresenceDetector.Components;
using SPT.Reflection.Patching;
using UnityEngine;

namespace QuestPresenceDetector.Patches;

internal sealed class QPD_LootItem_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LootItem)
            .GetMethod(nameof(LootItem.Init));
    }

    [PatchPostfix]
    public static void Postfix(LootItem __instance, Item item)
    {
        if (item != null && item.QuestItem && __instance.isActiveAndEnabled)
        {
            var newObj = new GameObject($"{__instance.gameObject.name}_qpd_tracker");
            var comp = newObj.AddComponent<QPD_Component>();
            comp.SetItem(__instance.gameObject, item.LocalizedName());
            newObj.transform.SetPositionAndRotation(__instance.gameObject.transform.position, __instance.gameObject.transform.rotation);
        }
    }
}
