using System;
using UnityEngine;
using UnityEngine.UI;

namespace QuestPresenceDetector.Components;

public sealed class QPDInterface : MonoBehaviour
{
    internal static QPDInterface Instance;

    private Image _circle;
    private Image _arrow;

    private bool _showArrow;

    private void Awake()
    {
        var circleObject = gameObject.transform.GetChild(0).transform.GetChild(0);
        if (!circleObject.TryGetComponent<Image>(out var circle))
        {
            QPD_Plugin.QPD_Logger.LogError("COULD NOT FIND CIRCLE");
            Destroy(gameObject);
            return;
        }

        _circle = circle;

        if (!circleObject.transform.GetChild(0).TryGetComponent<Image>(out var arrow))
        {
            QPD_Plugin.QPD_Logger.LogError("COULD NOT FIND ARROW");
            Destroy(gameObject);
            return;
        }

        _arrow = arrow;

        _showArrow = QPD_Plugin.ShowArrow.Value;

        Instance = this;

        QPD_Component.OnQPDEnter += OnEnter;
        QPD_Component.OnQPDValueChanged += OnValueChanged;
        QPD_Component.OnQPDArrowValueChanged += OnArrowValueChanged;
        QPD_Plugin.ShowArrow.SettingChanged += ShowArrow_SettingChanged;

        _circle.enabled = false;
        _arrow.enabled = false;

        QPD_Plugin.QPD_Logger.LogInfo("Interface loaded!");
    }

    private void ShowArrow_SettingChanged(object sender, EventArgs e)
    {
        _showArrow = QPD_Plugin.ShowArrow.Value;
    }

    private void OnArrowValueChanged(Vector3 worldPos)
    {
        var localTarget = Camera.main.transform.InverseTransformPoint(worldPos);

        var x = localTarget.x;
        var z = localTarget.z;

        var angleRadians = Mathf.Atan2(x, z);
        var angleDegrees = angleRadians * Mathf.Rad2Deg;

        _arrow.rectTransform.localRotation = Quaternion.Euler(0, 0, -angleDegrees);
    }

    private void OnValueChanged(float value)
    {
        _circle.fillAmount = value;
    }

    private void OnEnter(bool enabled)
    {
        _circle.enabled = enabled;
        _arrow.enabled = enabled && _showArrow;

        if (!enabled)
        {
            _circle.fillAmount = 0f;
        }
    }

    private void OnDestroy()
    {
        Instance = null;

        QPD_Component.OnQPDEnter -= OnEnter;
        QPD_Component.OnQPDValueChanged -= OnValueChanged;
        QPD_Component.OnQPDArrowValueChanged -= OnArrowValueChanged;
        QPD_Plugin.ShowArrow.SettingChanged -= ShowArrow_SettingChanged;
    }
}