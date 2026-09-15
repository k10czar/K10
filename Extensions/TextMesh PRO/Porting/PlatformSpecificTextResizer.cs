using UnityEngine;
using K10.Platforms;
using TMPro;
using System.Collections.Generic;
using System;
#if NAUGHTY_ATTRIBUTES
using NaughtyAttributes;
#endif

namespace K10.Platforms
{
    public class PlatformSpecificTextResizer : PlatformSpecificBehaviour
    {
        [Space(5)]
        [Header("Text Config")]
        [SerializeField] bool _dontChangeOnDefaultPlatforms = true;

    #if NAUGHTY_ATTRIBUTES
        [HideIf(nameof(_dontChangeOnDefaultPlatforms))]
    #endif
        [SerializeField] TextSize _defaultTextSize;
        [SerializeField] TextSize _adjustedTextSize;

        [SerializeField] List<TextMeshProUGUI> _texts;

        protected override void HandlePlatformsSpecificBehaviours(bool isContextValid)
        {
            if (!isContextValid && _dontChangeOnDefaultPlatforms)
                return;
            
            TextSize textSize = isContextValid ? _adjustedTextSize : _defaultTextSize;
            foreach (var text in _texts)
                ApplyTextSize(text, textSize);
        }

        private void ApplyTextSize(TextMeshProUGUI text, TextSize textSize)
        {
            if (text == null || textSize == null)
                return;

            text.enableAutoSizing = textSize.AutoSize;
            text.fontSize = textSize.FontSize;
            text.fontSizeMin = textSize.FontSizeMin;
            text.fontSizeMax = textSize.FontSizeMax;
        }

        [Serializable]
        public class TextSize
        {
            [SerializeField] bool _autoSize;
    #if NAUGHTY_ATTRIBUTES
            [HideIf(nameof(_autoSize)), AllowNesting] 
    #endif
            [SerializeField] float _fontSize;
    #if NAUGHTY_ATTRIBUTES
            [ShowIf(nameof(_autoSize)), AllowNesting]
    #endif
            [SerializeField] float _fontSizeMin;
    #if NAUGHTY_ATTRIBUTES
            [ShowIf(nameof(_autoSize)), AllowNesting]
    #endif
            [SerializeField] float _fontSizeMax;

            public bool AutoSize => _autoSize;
            public float FontSize => _fontSize;
            public float FontSizeMin => _fontSizeMin;
            public float FontSizeMax => _fontSizeMax;
        }
    }
}