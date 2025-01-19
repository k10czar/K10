using UnityEditor;
using UnityEditor.Build;

namespace K10.EditorUtils
{
    public static class DefineSymbols
    {
        public static void Toggle(string symbolName)
        {
            var isEnabled = Has(symbolName);

            if (isEnabled) Remove(symbolName);
            else Add(symbolName);

            // EditorPrefs.SetBool(symbolName, !isEnabled);

            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            AssetDatabase.SaveAssets();
        }

        public static bool Has(string symbol)
        {
            var all = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            return all.Contains(symbol);
        }

        private static void Remove(string symbol)
        {
            var all = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            var newSymbols = all.Replace($"{symbol}", "");

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, newSymbols);
        }

        private static void Add(string symbol)
        {
            var all = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, $"{all};{symbol}");
        }
    }
}