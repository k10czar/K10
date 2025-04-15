 using UnityEngine;
 
 
 /// <summary>
 /// Display a field as read-only in the inspector.
 /// CustomPropertyDrawers will not work when this attribute is used.
 /// </summary>
 /// <seealso cref="BeginBlockEditGroupAttribute"/>
 /// <seealso cref="EndBlockEditGroupAttribute"/>
 public class BlockEditAttribute : PropertyAttribute { }
 
 /// <summary>
 /// Display one or more fields as read-only in the inspector.
 /// Use <see cref="EndBlockEditGroupAttribute"/> to close the group.
 /// Works with CustomPropertyDrawers.
 /// </summary>
 /// <seealso cref="EndBlockEditGroupAttribute"/>
 /// <seealso cref="BlockEditAttribute"/>
 public class BeginBlockEditGroupAttribute : PropertyAttribute { }
 
 /// <summary>
 /// Use with <see cref="BeginBlockEditGroupAttribute"/>.
 /// Close the read-only group and resume editable fields.
 /// </summary>
 /// <seealso cref="BeginBlockEditGroupAttribute"/>
 /// <seealso cref="BlockEditAttribute"/>
 public class EndBlockEditGroupAttribute : PropertyAttribute { }
 