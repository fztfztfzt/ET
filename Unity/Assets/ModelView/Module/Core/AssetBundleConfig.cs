using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace AssetBundles
{
    public class AssetBundleConfig
    {
        public const string AssetBundleSuffix = ".assetbundle";
        public const string AssetsFolderName = "AssetsPackage";
        public const string AssetsPathMapFileName = "AssetsMap.bytes";

        const string kIsEditorMode = "IsEditorMode";
        public const string PATTREN = ",";
#if UNITY_EDITOR
        private static int mIsEditorMode = -1;

        public static bool IsEditorMode 
        {
            get
            {
                if(mIsEditorMode == -1)
                {
                    if (!EditorPrefs.HasKey(kIsEditorMode))
                    {
                        EditorPrefs.SetInt(kIsEditorMode, 1);
                    }
                    mIsEditorMode = EditorPrefs.GetInt(kIsEditorMode);
                }
                return mIsEditorMode == 1;
            }
            set
            {
                mIsEditorMode = value ? 1 : 0;
                EditorPrefs.SetInt(kIsEditorMode, mIsEditorMode);
            }
        }
#endif

        public static string PackagePathToAssetsPath(string assetPath)
        {
            return "Assets/" + AssetBundleConfig.AssetsFolderName + "/" + assetPath;
        }
    }
}
