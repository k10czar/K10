using UnityEngine;
using System.Collections.Generic;
using System;

public class NotificationConsole : MonoBehaviour
{
    const float TOP_MARGIN = 160;
    const float SIDE_MARGIN = 150;
    const int NORMAL_FONT_SIZE = 20;
    const int MOBILE_FONT_SIZE = 26;

    static NotificationConsole _instance;

    [SerializeField] Rect _area = new Rect(SIDE_MARGIN, TOP_MARGIN, 1000, 1000);

    [SerializeField] List<LabelData> _labelDraws = new()
    {
        new LabelData( Colors.AlmostBlack, new Vector2(2, 2), false ),
        new LabelData( Colors.KeyLime, new Vector2(0, 0) )
    };

    GUIStyle _style;

    List<Notification> _notifications = new();
    [SerializeField] bool _isDirty = false;
    double _nextVanish = double.MaxValue;
    [SerializeField,TextArea(5,25)] string _currentMessage = string.Empty;
    string _currentMessageNoColorTag = string.Empty;

    public static void Notify(string message, float notificationSeconds = 5f, bool alsoLogOnUnityConsole = true)
    {
        if (_instance == null)
            _instance = ComponentsAggregator.AtRuntime<NotificationConsole>();
            
        _instance.LocalNotify(message, notificationSeconds, alsoLogOnUnityConsole);
    }

    private void LocalNotify(string message, float notificationSeconds, bool alsoLogOnUnityConsole = true )
    {
        var refTime = Time.realtimeSinceStartupAsDouble;
        if (_notifications == null) _notifications = new();
        _notifications.Add(new Notification(message, refTime + notificationSeconds));
        if( refTime < _nextVanish ) _nextVanish = refTime;
        _isDirty = true;
        if( alsoLogOnUnityConsole ) Debug.Log( $"{"NotificationConsole".Colorfy(Colors.Erin)}: {message}\nwill last {notificationSeconds.ToStringColored(Colors.Console.Numbers)}s" );
    }

    void TryBuildMessage()
    {
        var refTime = Time.realtimeSinceStartupAsDouble;
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
            if( n.vanishTime < _nextVanish ) _nextVanish = n.vanishTime;
            SB.AppendLine(n.message);
        }

        _currentMessage = SB.ReturnToPoolAndCast();
        _currentMessageNoColorTag = _currentMessage.WithoutColorTags();
    }

    void OnGUI()
    {
        TryBuildMessage();
        if (string.IsNullOrEmpty(_currentMessage)) return;

        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label);
#if UNITY_ANDROID || UNITY_IOS
            _style.fontSize = MOBILE_FONT_SIZE;
#else
            _style.fontSize = NORMAL_FONT_SIZE;
#endif
            _style.alignment = TextAnchor.UpperRight;
            _style.normal.textColor = Color.white;
        }

        var initialColor = GUI.color;
        foreach (var draw in _labelDraws)
        {
            GUI.color = draw.color;
            _area.width = Screen.width - SIDE_MARGIN * 2;
            _area.height = Screen.height - TOP_MARGIN;
            var text = draw.canUseColorTag ? _currentMessage : _currentMessageNoColorTag;
            GUI.Label(_area.Move(draw.offset), text, _style);
        }
        GUI.color = initialColor;
    }

    [Serializable]
    private struct Notification
    {
        public string message;
        public double vanishTime;

        public Notification(string message, double vanishTime)
        {
            this.message = message;
            this.vanishTime = vanishTime;
        }
    }

    [Serializable]
    private struct LabelData
    {
        public Color color;
        public Vector2 offset;
        public bool canUseColorTag;

        public LabelData(Color color, Vector2 offset, bool canUseColorTag = true)
        {
            this.color = color;
            this.offset = offset;
            this.canUseColorTag = canUseColorTag;
        }
    }
}