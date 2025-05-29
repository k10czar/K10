using UnityEngine;
using System.Collections.Generic;
using System;

public class NotificationConsole : MonoBehaviour
{
    static NotificationConsole _instance;

    [SerializeField] Rect _area = new Rect(30, 150, 1000, 1000);

    [SerializeField] List<LabelData> _labelDraws = new()
    {
        new LabelData( Color.black, new Vector2(2, 2) ),
        new LabelData( Colors.ElectricLime, new Vector2(0, 0) )
    };

    GUIStyle _style;

    List<Notification> _notifications = new();
    [SerializeField] bool _isDirty = false;
    float _nextVanish = float.MaxValue;
    [SerializeField,TextArea(5,25)] string _currentMessage = string.Empty;

    public static void Notify(string message, float notificationSeconds = 5f)
    {
        if (_instance == null)
        {
            _instance = new GameObject( "NotificationConsole" ).AddComponent<NotificationConsole>();
        }
        _instance.LocalNotify(message, notificationSeconds);
    }

    private void LocalNotify(string message, float notificationSeconds )
    {
        var refTime = Time.timeSinceLevelLoad;
        if (_notifications == null) _notifications = new();
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

        _currentMessage = SB.ReturnToPoolAndCast();
    }

    void OnGUI()
    {
        TryBuildMessage();
        if (string.IsNullOrEmpty(_currentMessage)) return;

        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label);
            _style.fontSize = 20;
            _style.normal.textColor = Color.white;
        }

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