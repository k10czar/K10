using UnityEditor;

namespace Rogue.REditor
{
    public class ChangeDetector : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets.Length > 0) PropertyCollection.AssetsChanged();
        }
    }
}