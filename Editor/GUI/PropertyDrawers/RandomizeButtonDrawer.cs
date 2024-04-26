using K10.EditorGUIExtention;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RandomizeButtonAttribute))]
public sealed class RandomizeButtonDrawer : PropertyDrawer
{
	const float BUTTON_WIDTH = 40;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUIUtility.singleLineHeight;
	}

	public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	{
		if (GUI.Button(rect.RequestLeft(BUTTON_WIDTH), "Rand")) Randomize(property);
		GuiLabelWidthManager.New( EditorGUIUtility.labelWidth - (BUTTON_WIDTH + EditorGUIUtility.standardVerticalSpacing));
		EditorGUI.PropertyField(rect.CutLeft(BUTTON_WIDTH + EditorGUIUtility.standardVerticalSpacing), property);
		GuiLabelWidthManager.Revert();
		// GUI.Label(rect.CutLeft(BUTTON_WIDTH), $"{property.name}: {property.propertyType} {property.type}");
	}

	private static void Randomize(SerializedProperty property)
	{
		var rnd = new System.Random( Random.Range(0, int.MaxValue) );
		var type = property.type;
		switch (property.propertyType)
		{
			// case SerializedPropertyType.Generic: return property..ToStringOrNull();
			case SerializedPropertyType.Integer:
				{
					if (type == "int")
					{
						var min = int.MinValue;
						var max = int.MaxValue;
						property.intValue = rnd.Next(min,max);
						return;
					}
#if UNITY_2022_1_OR_NEWER
					else if (type == "uint")
					{
						property.uintValue = (uint)Random.Range(uint.MinValue, uint.MaxValue);
						return;
					}
					else if (type == "ulong")
					{
						property.ulongValue = rnd.NextULong();
						return;
					}
#endif //UNITY_2022_1_OR_NEWER
					else if (type == "long")
					{
						property.longValue = rnd.NextLong();
						return;
					}
					break;
				}
			case SerializedPropertyType.Boolean: property.boolValue = rnd.Next(1) == 1 ? true : false; return;
			case SerializedPropertyType.Float: //property.floatValue = (float)rnd.NextDouble();
				{
					if (type == "float")
					{
						property.floatValue = (float)rnd.NextDouble();
						return;
					}
					else if (type == "double")
					{
						property.doubleValue = rnd.NextDouble();
						return;
					}
					break;
				}
			case SerializedPropertyType.String: property.stringValue = GUID.Generate().ToString(); return;
			case SerializedPropertyType.Color: property.colorValue = new Color((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()); return;
			// case SerializedPropertyType.ObjectReference: property.objectReferenceValue.ToStringOrNull(); return;
			// case SerializedPropertyType.LayerMask: property..ToStringOrNull(); return;
			case SerializedPropertyType.Enum: property.enumValueIndex = rnd.Next( property.enumNames.Length ); return;
			case SerializedPropertyType.Vector2: property.vector2Value = new Vector2((float)rnd.NextDouble(), (float)rnd.NextDouble()); return;
			case SerializedPropertyType.Vector3: property.vector3Value = new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()); return;
			case SerializedPropertyType.Vector4: property.vector4Value = new Vector4((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()); return;
			case SerializedPropertyType.Rect: property.rectValue = new Rect((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()); return;
			// case SerializedPropertyType.ArraySize: property.arraySize.ToStringOrNull(); return;
			// case SerializedPropertyType.Character: property..ToStringOrNull(); return;
			// case SerializedPropertyType.AnimationCurve: property.animationCurveValue.ToStringOrNull(); return;
			case SerializedPropertyType.Bounds: property.boundsValue = new Bounds(new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()), new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble())); return;
			// case SerializedPropertyType.Gradient: property.gradientValue.ToStringOrNull(); return;
			case SerializedPropertyType.Quaternion: property.quaternionValue = new Quaternion((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()); return;
			// case SerializedPropertyType.ExposedReference: property.exposedReferenceValue.ToStringOrNull(); return;
			// case SerializedPropertyType.FixedBufferSize: property.fixedBufferSize.ToStringOrNull(); return;
			case SerializedPropertyType.Vector2Int: property.vector2IntValue = new Vector2Int(rnd.Next(), rnd.Next()); return;
			case SerializedPropertyType.Vector3Int: property.vector3IntValue = new Vector3Int(rnd.Next(), rnd.Next(), rnd.Next()); return;
			case SerializedPropertyType.RectInt: property.rectIntValue = new RectInt(rnd.Next(), rnd.Next(), rnd.Next(), rnd.Next()); return;
			case SerializedPropertyType.BoundsInt: property.boundsIntValue = new BoundsInt(new Vector3Int(rnd.Next(), rnd.Next(), rnd.Next()), new Vector3Int(rnd.Next(), rnd.Next(), rnd.Next())); ; return;
			// case SerializedPropertyType.ManagedReference: property.managedReferenceValue.ToStringOrNull(); return;
			case SerializedPropertyType.Hash128: property.hash128Value = new Hash128((uint)rnd.Next(), (uint)rnd.Next(), (uint)rnd.Next(), (uint)rnd.Next()); return;
		}
		Debug.LogError($"Randomize not implemented for {property.propertyType.ToStringOrNull()}.{type}");
	}
}
