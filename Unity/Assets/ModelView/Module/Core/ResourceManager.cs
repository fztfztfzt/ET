using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 提供统一的资源加载接口
/// 内部封装了资源加载模块，区分开发和发布模式
/// </summary>
namespace AssetBundles
{
    public class ResourceManager
    {
        static ResourceManager _instance;
        public static ResourceManager Instance { get
        {
            if(_instance==null)
            {
                _instance = new ResourceManager();
                    _instance.Initialize();
            }
            return _instance;
        } }
        AssetBundleManifest assetBundleManifest;
        AssetsPathMapping assetsPathMapping = new AssetsPathMapping();
        string GetAssetBundlePath(string abName)
        {
            return Path.Combine(Application.streamingAssetsPath, abName);
        }

        void InitManifest()
        {
            // 初始化Manifest，用来获取ab包的依赖关系
            var ManifestBundle = AssetBundle.LoadFromFile(AssetBundleConfig.GetAssetBundleMainfestPath());
            assetBundleManifest = ManifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            ManifestBundle.Unload(false);
        }

        public void Initialize()
        {
            InitManifest();
            // 初始化资源和ab映射表
            var assetBundle = AssetBundle.LoadFromFile(AssetBundleConfig.GetAssetBundlePath(assetsPathMapping.AssetbundleName));
            var mapContent = assetBundle.LoadAsset<TextAsset>(assetsPathMapping.AssetName);
            assetsPathMapping.Initialize(mapContent.text);
        }
        public void LoadBundle(string path)
        {
            var assetBundle = AssetBundle.LoadFromFile(path);
            assetBundle.LoadAllAssets();
        }
        public T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                return AssetDatabase.LoadAssetAtPath<T>("Assets/" + path);
            }
#endif
            var assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/res/images/1024portraits.ab");
            var ans = assetBundle.LoadAsset<T>("Assets/" + path);
            return ans;
        }
    }
}
