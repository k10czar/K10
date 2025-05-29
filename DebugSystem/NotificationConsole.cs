using UnityEngine;
using System.Collections.Generic;
using System;

public class NotificationConsole : MonoBehaviour
{
    [SerializeField] Rect _area = new Rect(20, 300, 500, 500);
    [SerializeField] List<LabelData> _labelDraws = new()
    {
        new LabelData( Color.black, new Vector2(2, 2) ),
        new LabelData( Colors.ElectricLime, new Vector2(0, 0) )
    };
    [SerializeField] GUIStyle _style = GUI.skin.label;
    [SerializeField] List<Notification> _notifications = new();
    [SerializeField] bool _isDirty = false;
    [SerializeField] float _nextVanish = float.MaxValue;
    [SerializeField] string _currentMessage = string.Empty;

    public static void Notify(string message, float notificationSeconds = 5f)
    {
        Guaranteed<NotificationConsole>.Instance.LocalNotify(message, notificationSeconds);
    }

    private void LocalNotify(string message, float notificationSeconds )
    {
        var refTime = Time.timeSinceLevelLoad;
        _notifications.Add(new Notification(message, refTime + notificationSeconds));
        _nextVanish = MathAdapter.min(_nextVanish, refTime);
        _isDirty = true;
        Debug.Log( $"{"NotificationConsole".Colorfy(Colors.Erin)}: {message}\nwill last {notificationSeconds.ToStringColored(Colors.Console.Numbers)}s" );
    }

    void TryBuildMessage()
    {
        var refTime = Time.timeSinceLevelLoad;
        if (!_isDirty && refTime < _nextVanish) return;

        var SB = StringBuilderPool.RequestEmpty();
        _nextVanish = float.MaxValue;

        for (int i = _notifications.Count - 1; i >= 0; i--)
        {
            var n = _notifications[i];
            if (refTime > n.vanishTime)
            {
                _notifications.RemoveAt(i);
                i--;
                continue;
            }
            _nextVanish = MathAdapter.min(_nextVanish, n.vanishTime);
            SB.AppendLine(n.message);
        }
    }

    void OnGUI()
    {
        TryBuildMessage();
        if (string.IsNullOrEmpty(_currentMessage)) return;

        GUI.Box(_area, GUIContent.none);

        var initialColor = GUI.color;
        foreach (var draw in _labelDraws)
        {
            GUI.color = draw.color;
            GUI.Label(_area.Move(draw.offset), _currentMessage, _style);
        }
        GUI.color = initialColor;
    }

    [Serializable]
    public struct Notification
    {
        public string message;
        public float vanishTime;

        public Notification(string message, float vanishTime)
        {
            this.message = message;
            this.vanishTime = vanishTime;
        }
    }

    [Serializable]
    public struct LabelData
    {
        public Color color;
        public Vector2 offset;

        public LabelData(Color color, Vector2 offset)
        {
            this.color = color;
            this.offset = offset;
        }
    }
}