using UnityEngine;
using UnityEditor;

public static class K10GuiStyles
{
	const int NORMAL_FONT_SIZE = 15;
	const int BIG_FONT_SIZE = 20;
	const int SMALL_FONT_SIZE = 12;

	public static Color DefaultGUIColorLerp( Color color, float whiteFactor = 75f ) => Color.LerpUnclamped( color, Color.white, .75f );

	public static readonly Color RED_TINT_COLOR = DefaultGUIColorLerp( Color.red );
	public static readonly Color MAGENTA_TINT_COLOR = DefaultGUIColorLerp( Color.magenta );
	public static readonly Color YELLOW_TINT_COLOR = DefaultGUIColorLerp( Color.yellow );
	public static readonly Color GREEN_TINT_COLOR = DefaultGUIColorLerp( Color.green );
	public static readonly Color BLUE_TINT_COLOR = DefaultGUIColorLerp( Color.blue );
	public static readonly Color CYAN_TINT_COLOR = DefaultGUIColorLerp( Color.cyan );
	public static readonly Color GREY_TINT_COLOR = DefaultGUIColorLerp( Color.grey );
	public static readonly Color DARKER_TINT_COLOR = DefaultGUIColorLerp( Color.black, .5f );
	public static readonly Color LIME_TINT_COLOR = DefaultGUIColorLerp( Color.LerpUnclamped( Color.yellow, Color.green, .25f ), .75f );

	public static readonly GUIStyle richLabel = new GUIStyle( EditorStyles.label ) { richText = true };
	public static readonly GUIStyle titleStyle = new GUIStyle( EditorStyles.label ) { fontSize = NORMAL_FONT_SIZE, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
	public static readonly GUIStyle bigBoldCenterStyle = new GUIStyle( EditorStyles.label ) { fontSize = BIG_FONT_SIZE, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
	public static readonly GUIStyle bigBold = new GUIStyle( EditorStyles.label ) { fontSize = BIG_FONT_SIZE, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
	public static readonly GUIStyle boldToggle = new GUIStyle( EditorStyles.toggle ) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
	public static readonly GUIStyle basicCenterStyle = new GUIStyle( EditorStyles.label ) { fontSize = NORMAL_FONT_SIZE, alignment = TextAnchor.MiddleCenter };
	public static readonly GUIStyle boldCenterStyle = new GUIStyle( EditorStyles.label ) { fontSize = NORMAL_FONT_SIZE, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
	public static readonly GUIStyle basicStyle = new GUIStyle( EditorStyles.label ) { fontSize = NORMAL_FONT_SIZE };
	public static readonly GUIStyle fieldStyle = new GUIStyle( EditorStyles.textField ) { fontSize = NORMAL_FONT_SIZE };
	public static readonly GUIStyle boldStyle = new GUIStyle( EditorStyles.label ) { fontSize = NORMAL_FONT_SIZE, fontStyle = FontStyle.Bold };
	public static readonly GUIStyle buttonStyle = new GUIStyle( EditorStyles.miniButton ) { fontSize = SMALL_FONT_SIZE, /*fontStyle = FontStyle.Bold*/};
	public static readonly GUIStyle midButtonStyle = new GUIStyle( EditorStyles.miniButton ) { fontSize = SMALL_FONT_SIZE, fontStyle = FontStyle.Bold, fixedHeight = 28 };
	public static readonly GUIStyle midLeftButtonStyle = new GUIStyle( EditorStyles.miniButton ) { fontSize = SMALL_FONT_SIZE, alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fixedHeight = 28 };
	public static readonly GUIStyle bigbuttonStyle = new GUIStyle( EditorStyles.miniButton ) { fontSize = NORMAL_FONT_SIZE, fontStyle = FontStyle.Bold, fixedHeight = 40 };
	public static readonly GUIStyle bigbuttonFlatStyle = new GUIStyle( EditorStyles.miniButton ) { fontSize = NORMAL_FONT_SIZE, fontStyle = FontStyle.Bold, fixedHeight = 32, normal = new GUIStyleState() { background = EditorGUIUtility.whiteTexture, textColor = EditorStyles.miniButton.normal.textColor } };
	public static readonly GUIStyle bigbuttonLeanFlatStyle = new GUIStyle( EditorStyles.miniButton ) { fontSize = NORMAL_FONT_SIZE, fontStyle = FontStyle.Bold, fixedHeight = 18, normal = new GUIStyleState() { background = EditorGUIUtility.whiteTexture, textColor = EditorStyles.miniButton.normal.textColor } };
	public static readonly GUIStyle bigFoldStyle = new GUIStyle( EditorStyles.foldout ) { fontSize = NORMAL_FONT_SIZE, fontStyle = FontStyle.Bold };
	public static readonly GUIStyle textFieldStyle = new GUIStyle( EditorStyles.textField ) { fontSize = NORMAL_FONT_SIZE, fontStyle = FontStyle.Bold };
	public static readonly GUIStyle foldStyle = new GUIStyle( EditorStyles.foldout ) { fontSize = NORMAL_FONT_SIZE, fontStyle = FontStyle.Bold };
	public static readonly GUIStyle centerEnumStyle = new GUIStyle( EditorStyles.popup ) { alignment = TextAnchor.MiddleCenter };
	public static readonly GUIStyle unitStyle = new GUIStyle( EditorStyles.centeredGreyMiniLabel ) { alignment = TextAnchor.LowerRight };

	public static readonly GUIStyle warningText = new GUIStyle( boldStyle ) { fontSize = NORMAL_FONT_SIZE, normal = new GUIStyleState() { textColor = Color.yellow } };
	public static readonly GUIStyle errorText = new GUIStyle( boldStyle ) { fontSize = NORMAL_FONT_SIZE, normal = new GUIStyleState() { textColor = Color.red } };

	public static readonly GUIStyle smallboldStyle = new GUIStyle( boldStyle ) { fontSize = SMALL_FONT_SIZE, alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
	public static readonly GUIStyle smallStyle = new GUIStyle( EditorStyles.label ) { fontSize = SMALL_FONT_SIZE };
	public static readonly GUIStyle smallboldRightStyle = new GUIStyle( boldStyle ) { fontSize = SMALL_FONT_SIZE, alignment = TextAnchor.MiddleRight };
	public static readonly GUIStyle smallRightStyle = new GUIStyle( smallStyle ) { alignment = TextAnchor.MiddleRight };
	public static readonly GUIStyle smallCenterStyle = new GUIStyle( smallStyle ) { alignment = TextAnchor.MiddleCenter };
	public static readonly GUIStyle smalltextFieldStyle = new GUIStyle( textFieldStyle ) { fontSize = SMALL_FONT_SIZE };
	public static readonly GUIStyle smallBoldCenterStyle = new GUIStyle( EditorStyles.boldLabel ) { alignment = TextAnchor.MiddleCenter };

	public static readonly GUIStyle leftFieldStyle = new GUIStyle( EditorStyles.textField ) { alignment = TextAnchor.MiddleLeft };

	public static readonly GUIStyle boxStyle = new GUIStyle( "CN Box" ) { padding = new RectOffset( 5, 5, 5, 5 ) };
	public static readonly GUIStyle tabTitleStyle = new GUIStyle( "HelpBox" ) { alignment = TextAnchor.MiddleCenter };
}
