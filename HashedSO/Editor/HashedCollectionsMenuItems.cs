using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

public static class HashedCollectionsMenuItems
{

    [MenuItem("K10/HashedCollections/Check All Consistencies", false, 0)]
    public static async Task CheckAllHashedListsConsistency()
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(BaseHashedSOCollection).Name}");
        await Task.Yield();
        var count = guids.Length;
        if( count == 0 )
        {
            Debug.Log( $"No Hashed Collections Found".Colorfy( Colors.Console.Warning ) );
            return;
        }
        Debug.Log( $"{"Found".Colorfy( Colors.Console.Verbs )} {guids.Length.ToStringColored( Colors.Console.Numbers )} Hashed Collections" );
        for (int i = 0; i < guids.Length; i++)
        {
            string guid = guids[i];
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath) as IHashedSOCollection;
            if (asset == null) continue;
            asset.EditorCheckConsistency();
            var coloredNum = i.ToStringColored( Colors.Console.Numbers );
            var coloredVerb = "CheckConsistency".Colorfy( Colors.Console.Verbs );
            var coloredPath = assetPath.Colorfy( Colors.Console.Info );
            Debug.Log( $"{coloredNum}]{coloredVerb} for {coloredPath}" );
            await Task.Yield();
        }
        AssetDatabase.SaveAssets();
    }
}
