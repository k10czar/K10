#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

#endif
using UnityEngine;

public static class Colors
{
    private const float BYTE_TO_FLOAT = 1f / 255f;
    public static Color From( byte r, byte g, byte b ) => new Color( r * BYTE_TO_FLOAT, g * BYTE_TO_FLOAT, b * BYTE_TO_FLOAT );

    public static class Console
    {
        public static readonly Color Numbers = LightSkyBlue;
        public static readonly Color Types = Khaki;
        public static readonly Color Interfaces = Orange;
        public static readonly Color Methods = Peridot;
        public static readonly Color Keyword = Color.LerpUnclamped( Color.green, Color.yellow, .5f );
        public static readonly Color Abstraction = Color.LerpUnclamped( Color.cyan, Color.blue, .5f );
        public static readonly Color Negation = Color.LerpUnclamped( Color.red, Color.yellow, .2f );
        public static readonly Color Verbs = Color.LerpUnclamped( Color.cyan, Color.green, .5f );
        public static readonly Color Names = Color.LerpUnclamped( Color.red, Color.yellow, .75f );
        public static readonly Color Punctuations = Color.LerpUnclamped( Color.red, Color.yellow, .75f );
    }

    public static string[] DebugLogs( int batches = 1 )
    {
        #if UNITY_EDITOR
        var allColors = typeof(Colors).GetFields( BindingFlags.Public | BindingFlags.Static );
        List<int> batchesDirector = new List<int>();
        var count = allColors.Count();
        for( int i = 1; i < batches; i++ ) batchesDirector.Add( count * i / batches );
        batchesDirector.Add( count );

        var debugs = new string[batchesDirector.Count];
        var it = 0;
        var SB = new StringBuilder();
        for( int bi = 0; bi < batchesDirector.Count; bi++ )
        {
            var stopAt = batchesDirector[bi];
            for( ; it < stopAt; it++ )
            {
                var field = allColors[it];
                var value = field.GetValue( null );
                if( value is Color color ) SB.Append( $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{field.Name}</color> " );
            }
            debugs[bi] = SB.ToString();
            SB.Clear();
        }
         
        return debugs;
        #else
        return new string[]{ "!!!NOT IMPLEMENTED OUT OF EDITOR!!!" };
        #endif
    }
    
    //Red colors
    public static readonly Color LightSalmon = From( 255, 160, 122);
    public static readonly Color Salmon = From( 250, 128, 114);
    public static readonly Color DarkSalmon = From( 233, 150, 122);
    public static readonly Color LightCoral = From( 240, 128, 128);
    public static readonly Color IndianRed = From( 205, 92, 92);
    public static readonly Color Crimson = From( 220, 20, 60);
    public static readonly Color FireBrick = From( 178, 34, 34);
    public static readonly Color DarkRed = From( 139, 0, 0);
    public static readonly Color Red = From( 255, 0, 0);
    //Orange colors
    public static readonly Color OrangeRed = From( 255, 69, 0);
    public static readonly Color Tomato = From( 255, 99, 71);
    public static readonly Color Coral = From( 255, 127, 80);
    public static readonly Color DarkOrange = From( 255, 140, 0);
    public static readonly Color Orange = From( 255, 165, 0);
    public static readonly Color Gold = From( 255, 215, 0);
    //Yellow colors
    public static readonly Color Yellow = From( 255, 255, 0);
    public static readonly Color LightYellow = From( 255, 255, 224);
    public static readonly Color LemonChiffon = From( 255, 250, 205);
    public static readonly Color LightGoldenrodYellow = From( 250, 250, 210);
    public static readonly Color PapayaWhip = From( 255, 239, 213);
    public static readonly Color Moccasin = From( 255, 228, 181);
    public static readonly Color PeachPuff = From( 255, 218, 185);
    public static readonly Color PaleGoldenrod = From( 238, 232, 170);
    public static readonly Color Khaki = From( 240, 230, 140);
    public static readonly Color DarkKhaki = From( 189, 183, 107);

    
    //Limes
    public static readonly Color Peridot = From( 230, 226, 0 );
    public static readonly Color Volt = From( 206, 255, 0 );
    public static readonly Color Lime = From( 192, 255, 0 );
    public static readonly Color LemonLime = From( 227, 255, 0 );
    public static readonly Color ArcticLime = From( 208, 255, 20 );
    public static readonly Color KeyLime = From( 232, 244, 140 );
    public static readonly Color ElectricLime = From( 204, 255, 0 );
    public static readonly Color FrenchLime = From( 158, 253, 56 );
    public static readonly Color BrightLime = From( 114, 254, 0 );
    public static readonly Color Chartreuse = From( 178, 214, 63 );
    public static readonly Color ChartreuseTraditional = From( 223, 255, 0 );
    //Brown colors
    public static readonly Color Cornsilk = From( 255, 248, 220);
    public static readonly Color BlanchedAlmond = From( 255, 235, 205);
    public static readonly Color Bisque = From( 255, 228, 196);
    public static readonly Color NavajoWhite = From( 255, 222, 173);
    public static readonly Color Wheat = From( 245, 222, 179);
    public static readonly Color BurlyWood = From( 222, 184, 135);
    public static readonly Color Tan = From( 210, 180, 140);
    public static readonly Color RosyBrown = From( 188, 143, 143);
    public static readonly Color SandyBrown = From( 244, 164, 96);
    public static readonly Color Goldenrod = From( 218, 165, 32);
    public static readonly Color DarkGoldenrod = From( 184, 134, 11);
    public static readonly Color Peru = From( 205, 133, 63);
    public static readonly Color Chocolate = From( 210, 105, 30);
    public static readonly Color SaddleBrown = From( 139, 69, 19);
    public static readonly Color Sienna = From( 160, 82, 45);
    public static readonly Color Brown = From( 165, 42, 42);
    public static readonly Color Maroon = From( 128, 0, 0);
    //Green colors
    public static readonly Color DarkOliveGreen = From( 85, 107, 47);
    public static readonly Color Olive = From( 128, 128, 0);
    public static readonly Color OliveDrab = From( 107, 142, 35);
    public static readonly Color YellowGreen = From( 154, 205, 50);
    public static readonly Color LimeGreen = From( 50, 205, 50);
    public static readonly Color FullGreen = From( 0, 255, 0);
    public static readonly Color LawnGreen = From( 124, 252, 0);
    // public static readonly Color Chartreuse = From( 127, 255, 0);
    public static readonly Color GreenYellow = From( 173, 255, 47);
    public static readonly Color SpringGreen = From( 0, 255, 127);
    public static readonly Color MediumSpringGreen = From( 0, 250, 154);
    public static readonly Color LightGreen = From( 144, 238, 144);
    public static readonly Color PaleGreen = From( 152, 251, 152);
    public static readonly Color DarkSeaGreen = From( 143, 188, 143);
    public static readonly Color MediumSeaGreen = From( 60, 179, 113);
    public static readonly Color SeaGreen = From( 46, 139, 87);
    public static readonly Color ForestGreen = From( 34, 139, 34);
    public static readonly Color Green = From( 0, 128, 0);
    public static readonly Color DarkGreen = From( 0, 100, 0);
    
    //Greens
    public static readonly Color Erin = From( 0, 255, 64 );
    public static readonly Color Harlequin = From( 63, 255, 0 );
    public static readonly Color AppleGreen = From( 138, 184, 0 );
    public static readonly Color ArtichokeGreen = From( 75, 111, 68 );
    public static readonly Color Evergreen = From( 5, 71, 42 );
    public static readonly Color FernGreen = From( 79, 121, 66 );
    public static readonly Color Fern = From( 99, 183, 108 );
    public static readonly Color JungleGreen = From( 41, 171, 135 );
    public static readonly Color KellyGreen = From( 78, 187, 23 );
    public static readonly Color KombuGreen = From( 53, 66, 48 );
    public static readonly Color LaurelGreen = From( 169, 186, 157 );
    public static readonly Color Mantis = From( 116, 195, 101 );
    public static readonly Color MossGreen = From( 138, 154, 91 );
    public static readonly Color MintGreen = From( 152, 251, 152 );
    public static readonly Color Myrtle = From( 33, 66, 30 );
    public static readonly Color Olivine = From( 154, 185, 115 );
    public static readonly Color PineGreen = From( 1, 121, 111 );
    public static readonly Color ResedaChartreuse = From( 108, 124, 89 );
    public static readonly Color SapGreen = From( 80, 125, 42 );
    public static readonly Color TeaGreen = From( 208, 240, 192 );
    public static readonly Color Emerald = From( 80, 200, 120 );
    public static readonly Color GreenEarth = From( 218, 221, 152 );
    public static readonly Color HookersGreen = From( 73, 121, 107 );
    public static readonly Color Jade = From( 0, 168, 107 );
    public static readonly Color Malachite = From( 11, 218, 81 );
    // public static readonly Color SeaGreen = From( 11, 218, 81 );
    public static readonly Color Celadon = From( 172, 225, 175 );
    public static readonly Color HunterGreen = From( 53, 94, 59 );
    public static readonly Color PersianGreen = From( 0, 166, 147 );
    public static readonly Color RifleGreen = From( 68, 76, 56 );
    public static readonly Color Pistachio = From( 147, 197, 114 );
    public static readonly Color Avocado = From( 86, 130, 3 );
    public static readonly Color Asparagus = From( 135, 169, 107 );
    public static readonly Color Artichoke = From( 143, 151, 121 );
    //Cyan colors
    public static readonly Color MediumAquamarine = From( 102, 205, 170);
    public static readonly Color Cyan = From( 0, 255, 255);
    public static readonly Color FluorescentBlue = From(21, 244, 238);
    public static readonly Color LightCyan = From( 224, 255, 255);
    public static readonly Color Celeste = From( 178, 255, 255 );
    public static readonly Color PaleTurquoise = From( 175, 238, 238);
    public static readonly Color Aquamarine = From( 127, 255, 212);
    public static readonly Color Turquoise = From( 64, 224, 208);
    public static readonly Color MediumTurquoise = From( 72, 209, 204);
    public static readonly Color DarkTurquoise = From( 0, 206, 209);
    public static readonly Color LightSeaGreen = From( 32, 178, 170);
    public static readonly Color CadetBlue = From( 95, 158, 160);
    public static readonly Color DarkCyan = From( 0, 139, 139);
    public static readonly Color Teal = From(54, 117, 136);
    //Blue colors
    public static readonly Color LightSteelBlue = From( 176, 196, 222);
    public static readonly Color PowderBlue = From( 176, 224, 230);
    public static readonly Color LightBlue = From( 173, 216, 230);
    public static readonly Color SkyBlue = From( 135, 206, 235);
    public static readonly Color LightSkyBlue = From( 135, 206, 250);
    public static readonly Color Periwinkle = From( 204, 204, 255 );
    public static readonly Color DeepSkyBlue = From( 0, 191, 255);
    public static readonly Color Capri = From( 0, 191, 255 );
    public static readonly Color DodgerBlue = From( 30, 144, 255);
    public static readonly Color Azure = From( 0, 128, 255 );
    public static readonly Color CornflowerBlue = From( 100, 149, 237);
    public static readonly Color OriginalBlurple = From(114, 137, 218);
    public static readonly Color SteelBlue = From( 70, 130, 180);
    public static readonly Color RoyalBlue = From( 65, 105, 225);
    public static readonly Color Blurple = From(88, 101, 242);
    public static readonly Color NeonBlue = From(77, 77, 255);
    public static readonly Color Bluebonnet = From(28, 28, 240);
    public static readonly Color Blue = From( 0, 0, 255);
    public static readonly Color MediumBlue = From( 0, 0, 205);
    public static readonly Color DarkBlue = From( 0, 0, 139);
    public static readonly Color Navy = From( 0, 0, 128);
    public static readonly Color Ultramarine = From( 18, 10, 143 );
    public static readonly Color MidnightBlue = From( 25, 25, 112);
    public static readonly Color Sapphire = From(8, 37, 103);
    
    public static readonly Color Cerulean = From( 0, 123, 167 );
    public static readonly Color DuckBlue = From(0, 119, 145);
    public static readonly Color CeruleanBlue = From( 42, 82, 190 );
    public static readonly Color SavoyBlue = From( 75, 97, 209 );
    public static readonly Color Liberty = From( 84, 90, 167 );
    public static readonly Color PicoteeBlue = From(46, 39, 135);
    public static readonly Color Independence = From(76, 81, 109);
    public static readonly Color DelftBlue = From( 31, 48, 94 );
    //Purple colors
    public static readonly Color Lavender = From( 230, 230, 250);
    public static readonly Color Thistle = From( 216, 191, 216);
    public static readonly Color Plum = From( 221, 160, 221);
    public static readonly Color Violet = From( 238, 130, 238);
    public static readonly Color Orchid = From( 218, 112, 214);
    public static readonly Color Magenta = From( 255, 0, 255);
    public static readonly Color MediumOrchid = From( 186, 85, 211);
    public static readonly Color MediumPurple = From( 147, 112, 219);
    public static readonly Color BlueViolet = From( 138, 43, 226);
    public static readonly Color DarkViolet = From( 148, 0, 211);
    public static readonly Color DarkOrchid = From( 153, 50, 204);
    public static readonly Color DarkMagenta = From( 139, 0, 139);
    public static readonly Color Purple = From( 128, 0, 128);
    public static readonly Color Indigo = From( 75, 0, 130);
    public static readonly Color DarkSlateBlue = From( 72, 61, 139);
    public static readonly Color SlateBlue = From( 106, 90, 205);
    public static readonly Color MediumSlateBlue = From( 123, 104, 238);
    //Pink colors
    public static readonly Color Pink = From( 255, 192, 203);
    public static readonly Color LightPink = From( 255, 182, 193);
    public static readonly Color HotPink = From( 255, 105, 180);
    public static readonly Color DeepPink = From( 255, 20, 147);
    public static readonly Color PaleVioletRed = From( 219, 112, 147);
    public static readonly Color MediumVioletRed = From( 199, 21, 133);
    //White/Gray/Black colors
    public static readonly Color White = From( 255, 255, 255);
    public static readonly Color Snow = From( 255, 250, 250);
    public static readonly Color Honeydew = From( 240, 255, 240);
    public static readonly Color MintCream = From( 245, 255, 250);
    // public static readonly Color Azure = From( 240, 255, 255);
    public static readonly Color AliceBlue = From( 240, 248, 255);
    public static readonly Color GhostWhite = From( 248, 248, 255);
    public static readonly Color WhiteSmoke = From( 245, 245, 245);
    public static readonly Color Seashell = From( 255, 245, 238);
    public static readonly Color Beige = From( 245, 245, 220);
    public static readonly Color OldLace = From( 253, 245, 230);
    public static readonly Color FloralWhite = From( 255, 250, 240);
    public static readonly Color Ivory = From( 255, 255, 240);
    public static readonly Color AntiqueWhite = From( 250, 235, 215);
    public static readonly Color Linen = From( 250, 240, 230);
    public static readonly Color LavenderBlush = From( 255, 240, 245);
    public static readonly Color MistyRose = From( 255, 228, 225);
    public static readonly Color Gainsboro = From( 220, 220, 220);
    public static readonly Color LightGray = From( 211, 211, 211);
    public static readonly Color Silver = From( 192, 192, 192);
    public static readonly Color DarkGray = From( 169, 169, 169);
    public static readonly Color Gray = From( 128, 128, 128);
    public static readonly Color DimGray = From( 105, 105, 105);
    public static readonly Color LightSlateGray = From( 119, 136, 153);
    public static readonly Color SlateGray = From( 112, 128, 144);
    public static readonly Color DarkSlateGray = From( 47, 79, 79);
    public static readonly Color Black = From( 0, 0, 0);


    //Old Dark Tones
    public static readonly Color OliveDrabCamouflage = From( 84, 79, 61 );
    public static readonly Color BlackOlive = From( 59, 60, 54 );
}
