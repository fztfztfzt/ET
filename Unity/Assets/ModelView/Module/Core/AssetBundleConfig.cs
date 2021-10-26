using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace AssetBundles
{
    public class AssetBundleConfig
    {
        public const string AssetBundleSuffix = ".ab";
        public const string AssetBundlePattern = "*" + AssetBundleSuffix;
        public static string AssetsPathMapFileName = "AssetsMap.bytes";
        public static string AssetsRootPath = Application.streamingAssetsPath;
        public const string PATTREN = ",";
        
        public static string GetAssetPath(string assetPath)
        {
            return Path.Combine("Assets", assetPath);
        }

        public static string GetAssetBundlePath(string assetPath)
        {
            return Path.Combine(Application.streamingAssetsPath, assetPath + AssetBundleSuffix);
        }

        public static string PackagePathToStreamAssetsPath(string assetPath)
        {
            return Path.Combine(Application.streamingAssetsPath, assetPath);
        }

        public static string GetAssetBundleMainfestPath()
        {
            return Path.Combine(Application.streamingAssetsPath, "streamingassets" + AssetBundleSuffix);
        }

#if UNITY_EDITOR
        const string kIsEditorMode = "IsEditorMode";
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


    }
}
