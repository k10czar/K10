using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using K10.EditorGUIExtention;
using System.Collections.Generic;


[CustomPropertyDrawer(typeof(Weighted))]
public class WeightedDrawer : PropertyDrawer
{
    private static Dictionary<(UnityEngine.Object,string),float> _sumCache = new Dictionary<(UnityEngine.Object, string), float>();

    public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
    {
        var weight = property.FindPropertyRelative( "_weight" );
        var data = property.FindPropertyRelative( "_t" );

        var startArea = area.VerticalSlice( 0, 2 );
        var restArea = area.VerticalSlice( 1, 2 );

        // string lastArray = null;
        // var wPath = weight.propertyPath;
        // var childCount = property.CountInProperty() - 1;
        // if( childCount <= 0 ) restArea = area;
        // else 
        // {
        //     int id = 0;
        //     foreach( var innProp in property )
        //     {
        //         if( innProp is SerializedProperty sp )
        //         {
        //             if( string.CompareOrdinal( sp.propertyPath, wPath ) == 0 ) continue;
        //             if (sp.isArray)
        //             {
        //                 lastArray = sp.propertyPath + ".Array.";
        //             }
        //             else if (lastArray != null)
        //             {
        //                 var isInnerArrayProp = sp.propertyPath.StartsWith(lastArray);
        //                 if (!isInnerArrayProp) lastArray = null;
        //                 else continue;
        //             }

        //             EditorGUI.PropertyField( startArea.VerticalSlice( id, childCount ), sp, GUIContent.none );
        //             id++;
        //         }
        //     }
        // }

        EditorGUI.PropertyField( startArea, data, GUIContent.none );
        GuiLabelWidthManager.New( 12 );
        var initialValue = weight.floatValue;
        EditorGUI.PropertyField( restArea.VerticalSlice( 0, 2 ), weight, new GUIContent("W") );
        weight.floatValue = Mathf.Max( 0, weight.floatValue );
        if( !Mathf.Approximately( initialValue, weight.floatValue ) ) _sumCache.Remove( ( property.serializedObject.targetObject, GetParentArrayPropPath( property ) ) );
        GuiLabelWidthManager.Revert();
        // var sw = new System.Diagnostics.Stopwatch();
        // sw.Start();
        var fill = CalculatePercentage( weight.floatValue, property );
        // sw.Stop();
        EditorGUI.ProgressBar( restArea.VerticalSlice( 1, 2 ), fill, fill.ToPercentageString() );
        // EditorGUI.ProgressBar( restArea.VerticalSlice( 1, 2 ), fill, fill.ToPercentageString() + $" {sw.Elapsed.TotalMilliseconds}ms%" );
    }

    private float CalculatePercentage( float weight, SerializedProperty property )
    {
        var sum = CalculateListWeigthSum( property );
        if( Mathf.Approximately( sum, 0 ) ) return 0;
        return weight / sum;
    }

    private string GetParentArrayPropPath( SerializedProperty property )
    {
        var path = property.propertyPath;
        var pLen = path.Length;
        var it = 1;
        while( path[pLen-it] != '[' && it < path.Length ) it++;
        return path.Substring( 0, Mathf.Max( pLen - ( ".Array.data".Length + it ), 0 ) );
    }

    private SerializedProperty GetParentArrayProp( SerializedProperty property )
    {
        var parentPath = GetParentArrayPropPath( property );
        return property.serializedObject.FindProperty( parentPath );
    }

    private float CalculateListWeigthSum( SerializedProperty property )
    {
        var parentPath = GetParentArrayPropPath( property );
        if( _sumCache.TryGetValue( ( property.serializedObject.targetObject, parentPath ), out var sum ) ) return sum;
        var p = property.serializedObject.FindProperty( parentPath );
        if( !p.isArray ) return 0;
        var arrayLen = p.arraySize;
        float newSum = 0;
        for( int i = 0; i < arrayLen; i++ ) newSum += p.GetArrayElementAtIndex(i).FindPropertyRelative( "_weight" ).floatValue;
        _sumCache[( property.serializedObject.targetObject, parentPath )] = newSum;
        return newSum;
    }

    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
