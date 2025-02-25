using UnityEditor;

namespace Skyx.SkyxEditor
{
    public class ChangeDetector : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets.Length > 0) PropertyCollection.AssetsChanged();
        }
    }
}