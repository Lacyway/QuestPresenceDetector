using System;
using EFT;
using UnityEngine;
using UnityEngine.UI;

namespace QuestPresenceDetector.Components;

internal sealed class QPDInterface : MonoBehaviour
{
    internal static QPDInterface Instance { get; private set; }

    private Image _circle;
    private Image _arrow;

    private bool _showArrow;
    private Transform _currentItem;
    private Player _player;
    private float _radius;

    private void Awake()
    {
        var circleObject = gameObject.transform.GetChild(0).transform.GetChild(0);
        if (!circleObject.TryGetComponent<Image>(out var circle))
        {
            QPDPlugin.QPD_Logger.LogError("COULD NOT FIND CIRCLE");
            Destroy(gameObject);
            return;
        }

        _circle = circle;

        if (!circleObject.transform.GetChild(0).TryGetComponent<Image>(out var arrow))
        {
            QPDPlugin.QPD_Logger.LogError("COULD NOT FIND ARROW");
            Destroy(gameObject);
            return;
        }

        _arrow = arrow;

        _showArrow = QPDPlugin.ShowArrow.Value;

        Instance = this;

        QPDPlugin.ShowArrow.SettingChanged += ShowArrow_SettingChanged;

        _circle.enabled = false;
        _arrow.enabled = false;

        QPDPlugin.QPD_Logger.LogInfo("Interface loaded!");
    }

    private void LateUpdate()
    {
        if (_currentItem == null || !_currentItem.gameObject.activeSelf) // inactive means it was most likely picked up
        {
            Toggle(false);
            return;
        }

        var offset = _currentItem.position - _player.Position;
        var sqrDist = offset.sqrMagnitude;
        var dist = Mathf.Sqrt(sqrDist);

        if (dist > _radius)
        {
            return;
        }

        const float fullScaleDistance = 1.2f;
        var percentage = Mathf.Clamp01((_radius - dist) / (_radius - fullScaleDistance));
        _circle.fillAmount = percentage;
#if DEBUG
        QPDPlugin.QPD_Logger.LogInfo($"Proximity: {percentage:P1}");
#endif

        if (!_showArrow)
        {
            return;
        }

        var localTarget = Camera.main.transform.InverseTransformPoint(_currentItem.position);

        var x = localTarget.x;
        var z = localTarget.z;

        var angleRadians = Mathf.Atan2(x, z);
        var angleDegrees = angleRadians * Mathf.Rad2Deg;

        _arrow.rectTransform.localRotation = Quaternion.Euler(0, 0, -angleDegrees);
    }

    public void Toggle(bool state)
    {
        enabled = state;
        _circle.enabled = state;
        _arrow.enabled = state && _showArrow;

        if (!state)
        {
            _circle.fillAmount = 0f;
            ClearData();
        }
    }

    private void ClearData()
    {
        _player = null;
        _currentItem = null;
        _radius = 0f;
    }

    public void SetData(Player player, Transform item, float radius)
    {
        _player = player;
        _currentItem = item;
        _radius = radius;
    }

    private void ShowArrow_SettingChanged(object sender, EventArgs e)
    {
        _showArrow = QPDPlugin.ShowArrow.Value;
    }

    private void OnDestroy()
    {
        Instance = null;
        QPDPlugin.ShowArrow.SettingChanged -= ShowArrow_SettingChanged;
    }
}