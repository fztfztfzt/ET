using UnityEngine;
using System.IO;
using System.Collections.Generic;
using ET;
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
            }
            return _instance;
        } }
        AssetBundleManifest assetBundleManifest;
        AssetsPathMapping assetsPathMapping = new AssetsPathMapping();




        



    }
}
