// © 2026 Lacyway All Rights Reserved

using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Diz.Utils;
using UnityEngine;

namespace QuestPresenceDetector.Bundles;

internal sealed class InternalBundleLoader
{
    internal static InternalBundleLoader Instance { get; private set; }

    private AssetBundle _masterBundle;

    internal InternalBundleLoader()
    {
        Task.Run(LoadBundles);
        Instance = this;
    }

    internal async Task LoadBundles()
    {
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var name in assembly.GetManifestResourceNames())
        {
            await using var stream = assembly.GetManifestResourceStream(name);
            await using MemoryStream memoryStream = new();

            var bundleName = name.Replace("QuestPresenceDetector.Bundles.", "")
                .Replace(".bundle", "");

            if (bundleName == "qpd_assets_all")
            {
                await stream.CopyToAsync(memoryStream);
                var assetBundle = AssetBundle.LoadFromMemoryAsync(memoryStream.ToArray());
                while (!assetBundle.isDone)
                {
                    await Task.Yield();
                }

                _masterBundle = assetBundle.assetBundle;
            }
            else
            {
                AsyncWorker.RunInMainTread(Application.Quit);
            }
        }
    }

    internal GameObject GetAsset()
    {
        return _masterBundle.LoadAsset<GameObject>("QPD.prefab");
    }
}