using System;
using EFT;
using EFT.Communications;
using EFT.Interactive;
using UnityEngine;

namespace QuestPresenceDetector.Components;

internal sealed class QPDComponent : TriggerWithId
{
    private GameObject _item;
    /// <summary>
    /// Workaround for a <see cref="NullReferenceException"/> when destroying the component
    /// </summary>
    private float _radius;
    private string _name;

    public void SetItem(GameObject item, string name)
    {
        _item = item;
        _name = name;
    }

    public override void Awake()
    {
        var trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        var areaSize = QPDPlugin.AreaSize.Value * 2f;
        trigger.radius = areaSize * 0.5f; // radius is half the diameter
        trigger.center = Vector3.zero;

        _radius = trigger.radius;

        base.Awake();
        QPDPlugin.QPD_Logger.LogInfo($"QPD_Component added to {gameObject.name}");
        SetId($"{gameObject.name}_qpd_tracker");
    }

    public override void TriggerEnter(Player player)
    {
        if (_item == null || !_item.gameObject.activeSelf)
        {
            QPDInterface.Instance.Toggle(false);
#if DEBUG
            QPDPlugin.QPD_Logger.LogWarning("Destroying self, no item");
#endif
            Destroy(this);
            return;
        }

        if (player != null)
        {
#if DEBUG
            QPDPlugin.QPD_Logger.LogInfo($"Entered: {player.name}");
#endif
            if (player.IsYourPlayer)
            {
#if DEBUG
                QPDPlugin.QPD_Logger.LogInfo($"{player.Profile.GetCorrectedNickname()} entered component");
#endif
                QPDInterface.Instance.Toggle(true);
                QPDInterface.Instance.SetData(player, _item.transform, _radius);
                if (QPDPlugin.ShowNotification.Value)
                {
                    NotificationManagerClass.DisplayMessageNotification($"Nearing objective item '{_name}'", iconType: ENotificationIconType.Quest);
                }
            }
        }
    }

    public override void TriggerExit(Player player)
    {
        if (_item == null || !_item.gameObject.activeSelf)
        {
            QPDInterface.Instance.Toggle(false);
#if DEBUG
            QPDPlugin.QPD_Logger.LogWarning("Destroying self, no item");
#endif
            Destroy(this);
            return;
        }

        if (player != null)
        {
#if DEBUG
            QPDPlugin.QPD_Logger.LogInfo($"Left: {player.name}");
#endif
            if (player.IsYourPlayer)
            {
#if DEBUG
                QPDPlugin.QPD_Logger.LogInfo($"{player.Profile.GetCorrectedNickname()} left component");
#endif
                QPDInterface.Instance.Toggle(false);
            }
        }
    }
}
