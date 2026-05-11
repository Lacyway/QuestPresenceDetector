using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using QuestPresenceDetector.Bundles;
using QuestPresenceDetector.Patches;

namespace QuestPresenceDetector;

[BepInPlugin("com.lacyway.qpd", "QuestPresenceDetector", _version)]
internal sealed class QPD_Plugin : BaseUnityPlugin
{
    private const string _version = "1.0.0";
    internal static ManualLogSource QPD_Logger;
    internal static InternalBundleLoader QPD_InternalBundleLoader;

    public static ConfigEntry<bool> ShowArrow { get; private set; }
    public static ConfigEntry<float> AreaSize { get; private set; }

    private void Awake()
    {
        QPD_Logger = Logger;
        QPD_InternalBundleLoader = new();

        ShowArrow = Config.Bind("Main", "Show Arrow", true,
            new ConfigDescription("If an arrow should be shown that points to the quest item"));
        AreaSize = Config.Bind("Main", "Area Size", 5f,
            new ConfigDescription("The size of the area around the quest item in meters\nCannot be changed mid-raid",
            new AcceptableValueRange<float>(5f, 25f)));

        QPD_Logger.LogInfo($"{nameof(QPD_Plugin)} v{_version} has been loaded.");
        new QPD_LootItem_Patch().Enable();
        new QPD_Player_Patch().Enable();
    }
}
