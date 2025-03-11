using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer( typeof( BlockEditAttribute ) )]
 public class BlockEditDrawer : PropertyDrawer {
 
     public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
         return EditorGUI.GetPropertyHeight( property, label, true );
     }
 
     public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
         using (var scope = new EditorGUI.DisabledGroupScope(true)) {
             EditorGUI.PropertyField( position, property, label, true );
         }
     }
 
 }
 
 [CustomPropertyDrawer( typeof( BeginBlockEditGroupAttribute ) )]
 public class BeginBlockEditGroupDrawer : DecoratorDrawer {
 
     public override float GetHeight() { return 0; }
 
     public override void OnGUI( Rect position ) {
         EditorGUI.BeginDisabledGroup( true );
     }
 
 }
 
 [CustomPropertyDrawer( typeof( EndBlockEditGroupAttribute ) )]
 public class EndBlockEditGroupDrawer : DecoratorDrawer {
 
     public override float GetHeight() { return 0; }
 
     public override void OnGUI( Rect position ) {
         EditorGUI.EndDisabledGroup();
     }
 
 }