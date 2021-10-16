using UnityEngine;
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
        public static T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                return AssetDatabase.LoadAssetAtPath<T>("Assets/" + path);
            }
#endif
            var assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/assets/res/images/1024.assetbundle");
            var ans = assetBundle.LoadAsset<T>("Assets/" + path);
            return ans;
        }
    }
}
