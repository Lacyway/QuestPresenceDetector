using System;
using EFT;
using EFT.Communications;
using EFT.Interactive;
using UnityEngine;

namespace QuestPresenceDetector.Components;

internal sealed class QPD_Component : TriggerWithId
{
    public static Action<float> OnQPDValueChanged;
    public static Action<Vector3> OnQPDArrowValueChanged;
    public static Action<bool> OnQPDEnter;

    private LocalPlayer _player;
    private GameObject _item;
    /// <summary>
    /// Workaround for a <see cref="NullReferenceException"/> when destroying the component
    /// </summary>
    private bool _destroyed;
    private float _radius;
    private string _name;
    private bool _updateArrow;

    public void SetItem(GameObject item, string name)
    {
        _item = item;
        _name = name;
    }

    public override void Awake()
    {
        var trigger = gameObject.AddComponent<BoxCollider>();

        trigger.isTrigger = true;
        var areaSize = QPD_Plugin.AreaSize.Value * 2f;
        trigger.size = new Vector3(areaSize, 2f, areaSize);
        trigger.center = Vector3.zero;

        _radius = trigger.size.x * 0.5f;

        base.Awake();
        QPD_Plugin.QPD_Logger.LogInfo($"QPD_Component added to {gameObject.name}");
        SetId($"{gameObject.name}_qpd_tracker");
        _updateArrow = QPD_Plugin.ShowArrow.Value;
        enabled = false;

        QPD_Plugin.ShowArrow.SettingChanged += ShowArrow_SettingChanged;
    }

    private void ShowArrow_SettingChanged(object sender, EventArgs e)
    {
        _updateArrow = QPD_Plugin.ShowArrow.Value;
    }

    public override void TriggerEnter(Player player)
    {
        if (_destroyed)
        {
            OnQPDEnter?.Invoke(false);
            return;
        }

        if (player != null)
        {
#if DEBUG
            QPD_Plugin.QPD_Logger.LogInfo($"Entered: {player.name}");
#endif
            if (player.IsYourPlayer)
            {
#if DEBUG
                QPD_Plugin.QPD_Logger.LogInfo($"{player.Profile.GetCorrectedNickname()} entered component");
#endif
                enabled = true;
                _player = player as LocalPlayer;
                OnQPDEnter?.Invoke(true);
                NotificationManagerClass.DisplayMessageNotification($"Nearing objective item '{_name}'", iconType: ENotificationIconType.Quest);
            }
        }
    }

    public override void TriggerExit(Player player)
    {
        if (_destroyed)
        {
            OnQPDEnter?.Invoke(false);
            return;
        }

        if (player != null)
        {
#if DEBUG
            QPD_Plugin.QPD_Logger.LogInfo($"Left: {player.name}");
#endif
            if (player.IsYourPlayer)
            {
#if DEBUG
                QPD_Plugin.QPD_Logger.LogInfo($"{player.Profile.GetCorrectedNickname()} left component");
#endif
                enabled = false;
                _player = null;
                OnQPDEnter?.Invoke(false);
            }
        }
    }

    private void LateUpdate()
    {
        if (_item == null || !_item.activeSelf)
        {
            _destroyed = true;
            Destroy(gameObject);
            return;
        }

        UpdateFill();
        if (_updateArrow)
        {
            UpdateArrow();
        }
    }

    private void UpdateArrow()
    {
        OnQPDArrowValueChanged?.Invoke(transform.position);
    }

    private void UpdateFill()
    {
        var offset = transform.position - _player.Position;
        var sqrDist = offset.sqrMagnitude;
        var dist = Mathf.Sqrt(sqrDist);

        if (dist > _radius)
        {
            return;
        }

        const float fullScaleDistance = 1.2f;
        var percentage = Mathf.Clamp01((_radius - dist) / (_radius - fullScaleDistance));

        OnQPDValueChanged?.Invoke(percentage);

#if DEBUG
        QPD_Plugin.QPD_Logger.LogInfo($"Proximity: {percentage:P1}");
#endif
    }

    private void OnDestroy()
    {
        QPD_Plugin.ShowArrow.SettingChanged -= ShowArrow_SettingChanged;
    }
}
