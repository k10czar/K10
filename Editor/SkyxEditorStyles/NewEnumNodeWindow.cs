using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Skyx.CustomEditor
{
    public class NewEnumNodeWindow : EditorWindow
    {
        public static void OpenWindow(Enum value, string path, string filePath)
        {
            var window = GetWindow<NewEnumNodeWindow>("New Enum", true);
            window.Setup(value, path, filePath);
        }

        private string previousEnumName;
        private string newNodeName;
        private string newNodePath;
        private int targetIntValue;

        private string filePath;

        private bool destroyed;

        private void OnLostFocus() => CloseInternal();

        private void CloseInternal()
        {
            if (destroyed || this == null) return;

            Close();
            destroyed = true;
        }

        private void OnGUI()
        {
            newNodeName = EditorGUILayout.TextField($"{newNodePath}/", newNodeName);
            EditorGUILayout.LabelField($"Target Enum Value: {targetIntValue}");

            EnsurePascalCase();

            EditorGUILayout.Space();

            if (GUILayout.Button("Create!")) CreateEnumEntry();
        }

        private void EnsurePascalCase()
        {
            var rgx = new Regex("[^a-zA-Z]");
            newNodeName = rgx.Replace(newNodeName, "");

            if (newNodeName.Length <= 0) return;

            var firstChar = newNodeName[0];
            if (!char.IsUpper(firstChar))
                newNodeName = newNodeName[0].ToString().ToUpper() + newNodeName[1..];
        }

        private string GetInsertedEntryLine()
        {
            return newNodePath == null
                ? $"        {newNodeName} = {targetIntValue},"
                : $"        [EnumTree({newNodePath.Replace("/", ",")})] {newNodeName} = {targetIntValue},";
        }

        private void CreateEnumEntry()
        {
            if (destroyed) return;

            EnsurePascalCase();
            if (string.IsNullOrEmpty(newNodeName)) return;

            var allLines = File.ReadLines(filePath).ToList();

            for (var index = 0; index < allLines.Count; index++)
            {
                var line = allLines[index];
                if (line.Contains(previousEnumName))
                {
                    allLines.Insert(index + 1, GetInsertedEntryLine());
                }
            }

            File.WriteAllLines(filePath, allLines);

            CloseInternal();
            CompilationPipeline.RequestScriptCompilation();
        }

        private void GetTargetEnumValue(Enum enumValue)
        {
            targetIntValue = ((IConvertible)enumValue).ToInt32(null);
            var delta = targetIntValue >= 0 ? 1 : -1;

            for (int i = 0; i < 1000; i++)
            {
                targetIntValue += delta;

                var boxedEnumValue = Enum.ToObject(enumValue.GetType(), targetIntValue);
                var isNumeric = int.TryParse(boxedEnumValue.ToString(), out _);

                if (isNumeric) return;
                else previousEnumName = boxedEnumValue.ToString();
            }
        }

        private void Setup(Enum value, string path, string targetPath)
        {
            SetupPosition();

            newNodePath = path;
            newNodeName = "";
            filePath = targetPath;

            GetTargetEnumValue(value);
        }

        private void SetupPosition()
        {
            const int width = 300;
            const int height = 200;

            var x = (Screen.currentResolution.width - width) / 2;
            var y = (Screen.currentResolution.height - height) / 2;

            position = new Rect(x, y, width, height);
        }
    }
}