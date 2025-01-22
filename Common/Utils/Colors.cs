#if UNITY_EDITOR
using K10.Reflection.Extensions;
#endif
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EConsoleColor { Primary, Secondary, Info, Success, Warning, Danger, Support, Special }

public static class Colors
{
    private const float BYTE_TO_FLOAT = 1f / 255f;
    public static Color From( byte r, byte g, byte b ) => new Color( r * BYTE_TO_FLOAT, g * BYTE_TO_FLOAT, b * BYTE_TO_FLOAT );

    [LazyConst] private static Dictionary<string,Color> ALL_COLORS = null;

    public static Color Get(string colorName)
    {
        if( string.IsNullOrEmpty( colorName ) ) return Color.white;
        if( colorName[0] == '#' )
        {
            ColorUtility.TryParseHtmlString( colorName, out var htmlColor );
            return htmlColor;
        }
        if( All.TryGetValue( colorName, out var color ) ) return color;
        return Color.white;
    }

    public static Dictionary<string, Color> All
    {
        get
        {
            if (ALL_COLORS != null) return ALL_COLORS;

            ALL_COLORS = new();
// #if UNITY_EDITOR
            var allColors = typeof(Colors).GetFields(FLAGS);
            foreach (var colorField in allColors)
            {
                var colorValue = colorField.GetValue(null);
                ALL_COLORS.Add(colorField.Name, (Color)colorValue);
            }
// #endif

            return ALL_COLORS;
        }
    }

    [LazyConst] private static Color[] optionsSequence;

    public static Color[] OptionsSequence
    {
        get
        {
            if (optionsSequence != null) return optionsSequence;

            optionsSequence = new[] { Azure, Fern, Coral, Cyan, Salmon, Goldenrod, DeepPink, Khaki, Cerulean, Crimson, LawnGreen, MediumSlateBlue, OrangeRed };

            return optionsSequence;
        }
    }

    [LazyConst] private static Color[] statusSequence;

    public static Color[] StatusSequence
    {
        get
        {
            if (statusSequence != null) return statusSequence;

            statusSequence = new [] { MintGreen, Orange, LightCoral, OrangeRed };

            return statusSequence;
        }
    }

    public static Color FromSequence<T>(T value, bool isStatus = false, bool loop = false) where T : Enum => FromSequence((int)(object)value, isStatus, loop);
    public static Color FromSequence(int index, bool isStatus = false, bool loop = false)
    {
        var sequence = isStatus ? StatusSequence : OptionsSequence;
        index = loop ? index % sequence.Length : Mathf.Clamp(index, 0, sequence.Length);
        return sequence[index];
    }

    private const System.Reflection.BindingFlags FLAGS = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;

#if UNITY_EDITOR
    [UnityEditor.MenuItem("K10/Colors/Log")]
    private static void EDITOR_Log()
    {
        var binding = FLAGS;
        typeof(Color).ReflectListMembers<Color>( EDITOR_DebugColor, binding, 0 ).Log();
        typeof(Colors).ReflectListMembers<Color>( EDITOR_DebugColor, binding, 0 ).Log();
        typeof(Console).ReflectListMembers<Color>( EDITOR_DebugColor, binding, 0 ).Log();
    }

    [UnityEditor.MenuItem("K10/Colors/Log Codes")]
    private static void EDITOR_LogCodes()
    {
        var binding = FLAGS;
        typeof(Color).ReflectListMembers<Color>( EDITOR_DebugColorCode, binding, 0 ).Log();
        typeof(Colors).ReflectListMembers<Color>( EDITOR_DebugColorCode, binding, 0 ).Log();
        typeof(Console).ReflectListMembers<Color>( EDITOR_DebugColorCode, binding, 0 ).Log();
    }

    private static string EDITOR_DebugColor( Color color, string name ) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{name} █  </color>";
    private static string EDITOR_DebugColorCode( Color color, string name ) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>#{ColorUtility.ToHtmlStringRGB(color)}█</color>";
#endif

    public static class Console
    {
        [LazyConst] private static Dictionary<string,Color> ALL_COLORS = null;
        public static Dictionary<string,Color> All
        {
            get
            {
                if( ALL_COLORS == null )
                {
                    ALL_COLORS = new();
#if UNITY_EDITOR
                    var allColors = typeof(Console).GetFields( FLAGS );
                    foreach( var colorField in allColors )
                    {
                        var colorValue = colorField.GetValue( null );
                        ALL_COLORS.Add( colorField.Name, (Color)colorValue );
                    }
#endif
                }
                return ALL_COLORS;
            }
        }

        [LazyConst] private static Color[] sequence;

        public static Color[] Sequence
        {
            get
            {
                if (sequence != null) return sequence;

                sequence = new [] { Primary, Secondary, Info, Success, Warning, Danger, Support, Special };

                return sequence;
            }
        }


        public static Color Get<T>(T value, bool loop = false) where T : Enum => Get((int)(object)value, loop);
        public static Color Get(int index, bool loop = false)
        {
            index = loop ? index % Sequence.Length : Mathf.Clamp(index, 0, Sequence.Length);
            return Sequence[index];
        }

        [ConstLike] public static readonly Color Danger = Crimson;
        [ConstLike] public static readonly Color Negation = OrangeRed;
        [ConstLike] public static readonly Color Interfaces = Coral;
        [ConstLike] public static readonly Color TypeName = Khaki;
        [ConstLike] public static readonly Color Names = Gold;
        [ConstLike] public static readonly Color Info = PaleGreen;
        [ConstLike] public static readonly Color Keyword = LawnGreen;
        [ConstLike] public static readonly Color Verbs = Goldenrod;
        [ConstLike] public static readonly Color Numbers = LightSkyBlue;
        [ConstLike] public static readonly Color Abstraction = Azure;
        [ConstLike] public static readonly Color Fields = MediumSlateBlue;
        [ConstLike] public static readonly Color EventName = DeepPink;
        [ConstLike] public static readonly Color Punctuations = HotPink;
        [ConstLike] public static readonly Color Success = MintGreen;
        [ConstLike] public static readonly Color Warning = Orange;
        [ConstLike] public static readonly Color LightDanger = LightCoral;

        [ConstLike] public static readonly Color GrayOut = LightGray;
        [ConstLike] public static readonly Color DarkerGrayOut = Silver;
        [ConstLike] public static readonly Color Dark = Charcoal;
        [ConstLike] public static readonly Color DarkerDark = DarkCharcoal;
        [ConstLike] public static readonly Color DarkBackground = AlmostBlack.WithAlpha(.4f);

        [ConstLike] public static readonly Color SuccessBackground = Avocado;
        [ConstLike] public static readonly Color WarningBackground = Tomato;

        [ConstLike] public static readonly Color Primary = Azure;
        [ConstLike] public static readonly Color Secondary = Cerulean;
        [ConstLike] public static readonly Color Light = Seashell;
        [ConstLike] public static readonly Color Special = Purple.WithAlpha(.6f);
        [ConstLike] public static readonly Color Support = Silver;
    }

    //Red colors
    [ConstLike] public static readonly Color LightSalmon = From( 255, 160, 122);
    [ConstLike] public static readonly Color Salmon = From( 250, 128, 114);
    [ConstLike] public static readonly Color DarkSalmon = From( 233, 150, 122);
    [ConstLike] public static readonly Color LightCoral = From( 240, 128, 128);
    [ConstLike] public static readonly Color IndianRed = From( 205, 92, 92);
    [ConstLike] public static readonly Color Crimson = From( 220, 20, 60);
    [ConstLike] public static readonly Color FireBrick = From( 178, 34, 34);
    [ConstLike] public static readonly Color DarkRed = From( 139, 0, 0);
    [ConstLike] public static readonly Color Red = From( 255, 0, 0);
    //Orange colors
    [ConstLike] public static readonly Color OrangeRed = From( 255, 69, 0);
    [ConstLike] public static readonly Color Tomato = From( 255, 99, 71);
    [ConstLike] public static readonly Color Coral = From( 255, 127, 80);
    [ConstLike] public static readonly Color DarkOrange = From( 255, 140, 0);
    [ConstLike] public static readonly Color Orange = From( 255, 165, 0);
    [ConstLike] public static readonly Color Gold = From( 255, 215, 0);
    //Yellow colors
    [ConstLike] public static readonly Color Yellow = From( 255, 255, 0);
    [ConstLike] public static readonly Color LightYellow = From( 255, 255, 224);
    [ConstLike] public static readonly Color LemonChiffon = From( 255, 250, 205);
    [ConstLike] public static readonly Color LightGoldenrodYellow = From( 250, 250, 210);
    [ConstLike] public static readonly Color PapayaWhip = From( 255, 239, 213);
    [ConstLike] public static readonly Color Moccasin = From( 255, 228, 181);
    [ConstLike] public static readonly Color PeachPuff = From( 255, 218, 185);
    [ConstLike] public static readonly Color PaleGoldenrod = From( 238, 232, 170);
    [ConstLike] public static readonly Color Khaki = From( 240, 230, 140);
    [ConstLike] public static readonly Color DarkKhaki = From( 189, 183, 107);


    //Limes
    [ConstLike] public static readonly Color Peridot = From( 230, 226, 0 );
    [ConstLike] public static readonly Color Volt = From( 206, 255, 0 );
    [ConstLike] public static readonly Color Lime = From( 192, 255, 0 );
    [ConstLike] public static readonly Color LemonLime = From( 227, 255, 0 );
    [ConstLike] public static readonly Color ArcticLime = From( 208, 255, 20 );
    [ConstLike] public static readonly Color KeyLime = From( 232, 244, 140 );
    [ConstLike] public static readonly Color ElectricLime = From( 204, 255, 0 );
    [ConstLike] public static readonly Color FrenchLime = From( 158, 253, 56 );
    [ConstLike] public static readonly Color BrightLime = From( 114, 254, 0 );
    [ConstLike] public static readonly Color Chartreuse = From( 178, 214, 63 );
    [ConstLike] public static readonly Color ChartreuseTraditional = From( 223, 255, 0 );
    //Brown colors
    [ConstLike] public static readonly Color Cornsilk = From( 255, 248, 220);
    [ConstLike] public static readonly Color BlanchedAlmond = From( 255, 235, 205);
    [ConstLike] public static readonly Color Bisque = From( 255, 228, 196);
    [ConstLike] public static readonly Color NavajoWhite = From( 255, 222, 173);
    [ConstLike] public static readonly Color Wheat = From( 245, 222, 179);
    [ConstLike] public static readonly Color BurlyWood = From( 222, 184, 135);
    [ConstLike] public static readonly Color Tan = From( 210, 180, 140);
    [ConstLike] public static readonly Color RosyBrown = From( 188, 143, 143);
    [ConstLike] public static readonly Color SandyBrown = From( 244, 164, 96);
    [ConstLike] public static readonly Color Goldenrod = From( 218, 165, 32);
    [ConstLike] public static readonly Color DarkGoldenrod = From( 184, 134, 11);
    [ConstLike] public static readonly Color Peru = From( 205, 133, 63);
    [ConstLike] public static readonly Color Chocolate = From( 210, 105, 30);
    [ConstLike] public static readonly Color SaddleBrown = From( 139, 69, 19);
    [ConstLike] public static readonly Color Sienna = From( 160, 82, 45);
    [ConstLike] public static readonly Color Brown = From( 165, 42, 42);
    [ConstLike] public static readonly Color Maroon = From( 128, 0, 0);
    //Green colors
    [ConstLike] public static readonly Color DarkOliveGreen = From( 85, 107, 47);
    [ConstLike] public static readonly Color Olive = From( 128, 128, 0);
    [ConstLike] public static readonly Color OliveDrab = From( 107, 142, 35);
    [ConstLike] public static readonly Color YellowGreen = From( 154, 205, 50);
    [ConstLike] public static readonly Color LimeGreen = From( 50, 205, 50);
    [ConstLike] public static readonly Color FullGreen = From( 0, 255, 0);
    [ConstLike] public static readonly Color LawnGreen = From( 124, 252, 0);
    // [ConstLike] public static readonly Color Chartreuse = From( 127, 255, 0);
    [ConstLike] public static readonly Color GreenYellow = From( 173, 255, 47);
    [ConstLike] public static readonly Color SpringGreen = From( 0, 255, 127);
    [ConstLike] public static readonly Color MediumSpringGreen = From( 0, 250, 154);
    [ConstLike] public static readonly Color LightGreen = From( 144, 238, 144);
    [ConstLike] public static readonly Color PaleGreen = From( 152, 251, 152);
    [ConstLike] public static readonly Color DarkSeaGreen = From( 143, 188, 143);
    [ConstLike] public static readonly Color MediumSeaGreen = From( 60, 179, 113);
    [ConstLike] public static readonly Color SeaGreen = From( 46, 139, 87);
    [ConstLike] public static readonly Color ForestGreen = From( 34, 139, 34);
    [ConstLike] public static readonly Color Green = From( 0, 128, 0);
    [ConstLike] public static readonly Color DarkGreen = From( 0, 100, 0);

    //Greens
    [ConstLike] public static readonly Color Erin = From( 0, 255, 64 );
    [ConstLike] public static readonly Color Harlequin = From( 63, 255, 0 );
    [ConstLike] public static readonly Color AppleGreen = From( 138, 184, 0 );
    [ConstLike] public static readonly Color ArtichokeGreen = From( 75, 111, 68 );
    [ConstLike] public static readonly Color Evergreen = From( 5, 71, 42 );
    [ConstLike] public static readonly Color FernGreen = From( 79, 121, 66 );
    [ConstLike] public static readonly Color Fern = From( 99, 183, 108 );
    [ConstLike] public static readonly Color JungleGreen = From( 41, 171, 135 );
    [ConstLike] public static readonly Color KellyGreen = From( 78, 187, 23 );
    [ConstLike] public static readonly Color KombuGreen = From( 53, 66, 48 );
    [ConstLike] public static readonly Color LaurelGreen = From( 169, 186, 157 );
    [ConstLike] public static readonly Color Mantis = From( 116, 195, 101 );
    [ConstLike] public static readonly Color MossGreen = From( 138, 154, 91 );
    [ConstLike] public static readonly Color MintGreen = From( 152, 251, 152 );
    [ConstLike] public static readonly Color Myrtle = From( 33, 66, 30 );
    [ConstLike] public static readonly Color Olivine = From( 154, 185, 115 );
    [ConstLike] public static readonly Color PineGreen = From( 1, 121, 111 );
    [ConstLike] public static readonly Color ResedaChartreuse = From( 108, 124, 89 );
    [ConstLike] public static readonly Color SapGreen = From( 80, 125, 42 );
    [ConstLike] public static readonly Color TeaGreen = From( 208, 240, 192 );
    [ConstLike] public static readonly Color Emerald = From( 80, 200, 120 );
    [ConstLike] public static readonly Color GreenEarth = From( 218, 221, 152 );
    [ConstLike] public static readonly Color HookersGreen = From( 73, 121, 107 );
    [ConstLike] public static readonly Color Jade = From( 0, 168, 107 );
    [ConstLike] public static readonly Color Malachite = From( 11, 218, 81 );
    // [ConstLike] public static readonly Color SeaGreen = From( 11, 218, 81 );
    [ConstLike] public static readonly Color Celadon = From( 172, 225, 175 );
    [ConstLike] public static readonly Color HunterGreen = From( 53, 94, 59 );
    [ConstLike] public static readonly Color PersianGreen = From( 0, 166, 147 );
    [ConstLike] public static readonly Color RifleGreen = From( 68, 76, 56 );
    [ConstLike] public static readonly Color Pistachio = From( 147, 197, 114 );
    [ConstLike] public static readonly Color Avocado = From( 86, 130, 3 );
    [ConstLike] public static readonly Color Asparagus = From( 135, 169, 107 );
    [ConstLike] public static readonly Color Artichoke = From( 143, 151, 121 );
    //Cyan colors
    [ConstLike] public static readonly Color MediumAquamarine = From( 102, 205, 170);
    [ConstLike] public static readonly Color Cyan = From( 0, 255, 255);
    [ConstLike] public static readonly Color FluorescentBlue = From(21, 244, 238);
    [ConstLike] public static readonly Color LightCyan = From( 224, 255, 255);
    [ConstLike] public static readonly Color Celeste = From( 178, 255, 255 );
    [ConstLike] public static readonly Color PaleTurquoise = From( 175, 238, 238);
    [ConstLike] public static readonly Color Aquamarine = From( 127, 255, 212);
    [ConstLike] public static readonly Color Turquoise = From( 64, 224, 208);
    [ConstLike] public static readonly Color MediumTurquoise = From( 72, 209, 204);
    [ConstLike] public static readonly Color DarkTurquoise = From( 0, 206, 209);
    [ConstLike] public static readonly Color LightSeaGreen = From( 32, 178, 170);
    [ConstLike] public static readonly Color CadetBlue = From( 95, 158, 160);
    [ConstLike] public static readonly Color DarkCyan = From( 0, 139, 139);
    [ConstLike] public static readonly Color Teal = From(54, 117, 136);
    //Blue colors
    [ConstLike] public static readonly Color LightSteelBlue = From( 176, 196, 222);
    [ConstLike] public static readonly Color PowderBlue = From( 176, 224, 230);
    [ConstLike] public static readonly Color LightBlue = From( 173, 216, 230);
    [ConstLike] public static readonly Color SkyBlue = From( 135, 206, 235);
    [ConstLike] public static readonly Color LightSkyBlue = From( 135, 206, 250);
    [ConstLike] public static readonly Color Periwinkle = From( 204, 204, 255 );
    [ConstLike] public static readonly Color DeepSkyBlue = From( 0, 191, 255);
    [ConstLike] public static readonly Color Capri = From( 0, 191, 255 );
    [ConstLike] public static readonly Color DodgerBlue = From( 30, 144, 255);
    [ConstLike] public static readonly Color Azure = From( 0, 128, 255 );
    [ConstLike] public static readonly Color CornflowerBlue = From( 100, 149, 237);
    [ConstLike] public static readonly Color OriginalBlurple = From(114, 137, 218);
    [ConstLike] public static readonly Color SteelBlue = From( 70, 130, 180);
    [ConstLike] public static readonly Color RoyalBlue = From( 65, 105, 225);
    [ConstLike] public static readonly Color Blurple = From(88, 101, 242);
    [ConstLike] public static readonly Color NeonBlue = From(77, 77, 255);
    [ConstLike] public static readonly Color Bluebonnet = From(28, 28, 240);
    [ConstLike] public static readonly Color Blue = From( 0, 0, 255);
    [ConstLike] public static readonly Color MediumBlue = From( 0, 0, 205);
    [ConstLike] public static readonly Color DarkBlue = From( 0, 0, 139);
    [ConstLike] public static readonly Color Navy = From( 0, 0, 128);
    [ConstLike] public static readonly Color Ultramarine = From( 18, 10, 143 );
    [ConstLike] public static readonly Color MidnightBlue = From( 25, 25, 112);
    [ConstLike] public static readonly Color Sapphire = From(8, 37, 103);

    [ConstLike] public static readonly Color Cerulean = From( 0, 123, 167 );
    [ConstLike] public static readonly Color DuckBlue = From(0, 119, 145);
    [ConstLike] public static readonly Color CeruleanBlue = From( 42, 82, 190 );
    [ConstLike] public static readonly Color SavoyBlue = From( 75, 97, 209 );
    [ConstLike] public static readonly Color Liberty = From( 84, 90, 167 );
    [ConstLike] public static readonly Color PicoteeBlue = From(46, 39, 135);
    [ConstLike] public static readonly Color Independence = From(76, 81, 109);
    [ConstLike] public static readonly Color DelftBlue = From( 31, 48, 94 );
    //Purple colors
    [ConstLike] public static readonly Color Lavender = From( 230, 230, 250);
    [ConstLike] public static readonly Color Thistle = From( 216, 191, 216);
    [ConstLike] public static readonly Color Plum = From( 221, 160, 221);
    [ConstLike] public static readonly Color Violet = From( 238, 130, 238);
    [ConstLike] public static readonly Color Orchid = From( 218, 112, 214);
    [ConstLike] public static readonly Color Magenta = From( 255, 0, 255);
    [ConstLike] public static readonly Color MediumOrchid = From( 186, 85, 211);
    [ConstLike] public static readonly Color MediumPurple = From( 147, 112, 219);
    [ConstLike] public static readonly Color BlueViolet = From( 138, 43, 226);
    [ConstLike] public static readonly Color DarkViolet = From( 148, 0, 211);
    [ConstLike] public static readonly Color DarkOrchid = From( 153, 50, 204);
    [ConstLike] public static readonly Color DarkMagenta = From( 139, 0, 139);
    [ConstLike] public static readonly Color Purple = From( 128, 0, 128);
    [ConstLike] public static readonly Color Indigo = From( 75, 0, 130);
    [ConstLike] public static readonly Color DarkSlateBlue = From( 72, 61, 139);
    [ConstLike] public static readonly Color SlateBlue = From( 106, 90, 205);
    [ConstLike] public static readonly Color MediumSlateBlue = From( 123, 104, 238);
    //Pink colors
    [ConstLike] public static readonly Color Pink = From( 255, 192, 203);
    [ConstLike] public static readonly Color LightPink = From( 255, 182, 193);
    [ConstLike] public static readonly Color HotPink = From( 255, 105, 180);
    [ConstLike] public static readonly Color DeepPink = From( 255, 20, 147);
    [ConstLike] public static readonly Color PaleVioletRed = From( 219, 112, 147);
    [ConstLike] public static readonly Color MediumVioletRed = From( 199, 21, 133);
    //White/Gray/Black colors
    [ConstLike] public static readonly Color White = From( 255, 255, 255);
    [ConstLike] public static readonly Color Snow = From( 255, 250, 250);
    [ConstLike] public static readonly Color Honeydew = From( 240, 255, 240);
    [ConstLike] public static readonly Color MintCream = From( 245, 255, 250);
    // [ConstLike] public static readonly Color Azure = From( 240, 255, 255);
    [ConstLike] public static readonly Color AliceBlue = From( 240, 248, 255);
    [ConstLike] public static readonly Color GhostWhite = From( 248, 248, 255);
    [ConstLike] public static readonly Color WhiteSmoke = From( 245, 245, 245);
    [ConstLike] public static readonly Color Seashell = From( 255, 245, 238);
    [ConstLike] public static readonly Color Beige = From( 245, 245, 220);
    [ConstLike] public static readonly Color OldLace = From( 253, 245, 230);
    [ConstLike] public static readonly Color FloralWhite = From( 255, 250, 240);
    [ConstLike] public static readonly Color Ivory = From( 255, 255, 240);
    [ConstLike] public static readonly Color AntiqueWhite = From( 250, 235, 215);
    [ConstLike] public static readonly Color Linen = From( 250, 240, 230);
    [ConstLike] public static readonly Color LavenderBlush = From( 255, 240, 245);
    [ConstLike] public static readonly Color MistyRose = From( 255, 228, 225);
    [ConstLike] public static readonly Color Gainsboro = From( 220, 220, 220);
    [ConstLike] public static readonly Color LightGray = From( 211, 211, 211);
    [ConstLike] public static readonly Color Silver = From( 192, 192, 192);
    [ConstLike] public static readonly Color DarkGray = From( 169, 169, 169);
    [ConstLike] public static readonly Color Gray = From( 128, 128, 128);
    [ConstLike] public static readonly Color DimGray = From( 105, 105, 105);
    [ConstLike] public static readonly Color LightSlateGray = From( 119, 136, 153);
    [ConstLike] public static readonly Color SlateGray = From( 112, 128, 144);
    [ConstLike] public static readonly Color DarkSlateGray = From( 47, 79, 79);
    [ConstLike] public static readonly Color Charcoal = From( 70, 70, 70);
    [ConstLike] public static readonly Color DarkCharcoal = From( 60, 60, 60);
    [ConstLike] public static readonly Color AlmostBlack = From( 25, 25, 25);
    [ConstLike] public static readonly Color Black = From( 0, 0, 0);


    //Old Dark Tones
    [ConstLike] public static readonly Color OliveDrabCamouflage = From( 84, 79, 61 );
    [ConstLike] public static readonly Color BlackOlive = From( 59, 60, 54 );
}