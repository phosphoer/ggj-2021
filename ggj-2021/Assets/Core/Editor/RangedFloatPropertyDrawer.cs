using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(RangedFloat))]
[CustomPropertyDrawer(typeof(RangedInt))]
public class RangedFloatPropertyDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    EditorGUI.BeginProperty(position, label, property);
    SerializedProperty minProp = property.FindPropertyRelative("MinValue");
    SerializedProperty maxProp = property.FindPropertyRelative("MaxValue");

    position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

    Rect drawRect = position;
    bool drawLabels = position.width > 200;

    int oldIndentLevel = EditorGUI.indentLevel;
    EditorGUI.indentLevel = 0;

    GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
    if (drawLabels)
    {
      float minWidth, maxWidth = 0;
      labelStyle.CalcMinMaxWidth(new GUIContent("Min"), out minWidth, out maxWidth);
      drawRect.width = minWidth;
      EditorGUI.LabelField(drawRect, "Min", labelStyle);
      drawRect.x += drawRect.width + 5;
    }

    drawRect.width = 50;
    EditorGUI.PropertyField(drawRect, minProp, GUIContent.none);
    drawRect.x += drawRect.width + 10;

    if (drawLabels)
    {
      float minWidth, maxWidth = 0;
      labelStyle.CalcMinMaxWidth(new GUIContent("Max"), out minWidth, out maxWidth);
      drawRect.width = minWidth;
      EditorGUI.LabelField(drawRect, "Max", labelStyle);
      drawRect.x += drawRect.width + 5;
    }

    drawRect.width = 50;
    EditorGUI.PropertyField(drawRect, maxProp, GUIContent.none);

    EditorGUI.indentLevel = oldIndentLevel;
    EditorGUI.EndProperty();
  }
}