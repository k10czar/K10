using UnityEngine;

public static class Colors
{
	public static readonly Color Orange = Color.LerpUnclamped( Color.red, Color.yellow, .5f );

    public static class Console
    {
        public static readonly Color Numbers = Color.cyan;
        public static readonly Color Types = Color.yellow;
        public static readonly Color Interfaces = Orange;
        public static readonly Color Keyword = Color.LerpUnclamped( Color.green, Color.yellow, .5f );
        public static readonly Color Abstraction = Color.LerpUnclamped( Color.cyan, Color.blue, .5f );
        public static readonly Color Negation = Color.LerpUnclamped( Color.red, Color.yellow, .2f );
        public static readonly Color Verbs = Color.LerpUnclamped( Color.cyan, Color.green, .5f );
    }
}
