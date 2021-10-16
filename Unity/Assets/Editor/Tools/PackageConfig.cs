using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using Sirenix.OdinInspector;
using AssetBundles;
using ET;

[System.Serializable]
public class PackageItem
{
    public enum PackType { File, DirFiles, Dir }
    public PackType type;
    [FolderPath(AbsolutePath=false,ParentFolder = "Assets", UseBackslashes=true)]
    public string path;
    public string searchPattern = "*.*";
    public SearchOption searchOption;
}

[CreateAssetMenu(fileName = "packageConfig", menuName = "packageConfig", order = 1)]
public class PackageConfig : ScriptableObject
{
    #region 单例类
    private static PackageConfig _instance;
    public static PackageConfig Instance
    {
        get
        {
            _instance = AssetDatabase.LoadAssetAtPath<PackageConfig>("Assets/Editor/Tools/PackConf.asset");
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<PackageConfig>();
                AssetDatabase.CreateAsset(_instance, "Assets/Tools/Editor/PackConf.asset");
            }

            return _instance;
        }
    }
    #endregion

    #region 可配置属性定义
    [SerializeField]
    List<PackageItem> packageItems;
    [SerializeField]
    [FolderPath(AbsolutePath=false)]
    string targetDir;
    [SerializeField]
    BuildTarget targetPlatform;
    #endregion


    string abExtens = AssetBundles.AssetBundleConfig.AssetBundleSuffix;
    string abDirExtens = AssetBundles.AssetBundleConfig.AssetBundleSuffix;


    //文件打包
    public bool PackFile(string res)
    {
        AssetImporter importer = AssetImporter.GetAtPath(res);
        if (importer == null)
        {
            Debug.LogError("Path not Exist!" + res);
            return false;
        }
        importer.assetBundleName = res + abExtens;
        return true;
    }

    //打包目录中所有资源，查找文件夹下的所有文件，每个文件单独一个AB包
    public bool PackDirFiles(string res, string pattern, SearchOption searchOption)
    {
        string[] files = Directory.GetFiles(res, pattern, searchOption);
        if (files.Length == 0) return false;
        foreach (var file in files)
        {
            PackFile(file);
        }
        return true;
    }

    //目录打包
    public bool PackDir(string res, string pattern, SearchOption searchOption)
    {
        string[] files = Directory.GetFiles(res, pattern, searchOption);
        if (files.Length == 0) return false;
        string fn = res.Trim('/') + abDirExtens;

        foreach (var file in files)
        {
            AssetImporter impoter = AssetImporter.GetAtPath(file);
            impoter.assetBundleName = fn;
        }
        return true;
    }

    //打包目录中所有资源
    public bool PackDirDir(string res, string pattern)
    {
        string[] Dirs = Directory.GetDirectories(res);
        if (Dirs.Length == 0) return false;
        foreach (var d in Dirs)
        {
            PackDir(d.Replace('\\', '/'), pattern, SearchOption.AllDirectories);
        }
        return true;
    }
    public static void CreateAssetbundleForCurrent(string assetPath)
    {
        AssetImporter importer = AssetImporter.GetAtPath(assetPath);
        if (importer == null)
        {
            Debug.LogError(string.Format("importer null! make sure object at path({0}) is a valid assetbundle!", assetPath));
            return;
        }

        importer.assetBundleName = importer.assetPath;
    }
    public AssetBundleManifest BuildAllAssetRes()
    {
        if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
        BuildAssetBundleOptions buildOption = BuildAssetBundleOptions.IgnoreTypeTreeChanges | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression;
        return BuildPipeline.BuildAssetBundles(targetDir, buildOption, targetPlatform);
    }
    void BuildPathMapping(AssetBundleManifest manifest)
    {
        List<string> mappingList = new List<string>();
        string[] allAssetbundles = manifest.GetAllAssetBundles();
        foreach(var assetbundle in allAssetbundles)
        {
            var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetbundle);
            foreach(var assetPath in assetPaths)
            {
                string mappingItem = string.Format("{0}{1}{2}", assetbundle, AssetBundleConfig.PATTREN, assetPath);
                mappingList.Add(mappingItem);
            }
        }
        mappingList.Sort();//让每次生成文件内容相同
        string outputFilePath = AssetBundleConfig.PackagePathToAssetsPath(AssetBundleConfig.AssetsPathMapFileName);

        if (!GameUtility.SafeWriteAllLines(outputFilePath, mappingList.ToArray()))
        {
            Debug.LogError("BuildPathMapping failed!!! try rebuild it again!");
        }
        else
        {
            AssetDatabase.Refresh();
            CreateAssetbundleForCurrent(outputFilePath);
            Debug.Log("BuildPathMapping success...");
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/BuildAll")]
    public static void BuildAllResources()
    {
        Log.Info("Start BuildAll");
        Log.Info($"Clear {Instance.targetDir}");
        GameUtility.SafeClearDir(Instance.targetDir);
        Log.Info("start set ab name");
        foreach (var item in Instance.packageItems)
        {
            string path = "Assets/" + item.path;
            if (item.type == PackageItem.PackType.File)
            {
                Instance.PackFile(item.path);
            }
            else if (item.type == PackageItem.PackType.DirFiles)
            {
                Instance.PackDirFiles(item.path, item.searchPattern, item.searchOption);
            }
            else if (item.type == PackageItem.PackType.Dir)
            {
                Instance.PackDir(path, item.searchPattern, item.searchOption);
            }
        }
        Log.Info("set ab name success,start first build");
        AssetBundleManifest manifest = Instance.BuildAllAssetRes();
        Log.Info("start save maifest mapping");
        Instance.BuildPathMapping(manifest);
        Log.Info("start build all");
        Instance.BuildAllAssetRes();
        Log.Info("build success!!");
        GameUtility.CopyFolder(Instance.targetDir, Application.streamingAssetsPath,"*" + AssetBundleConfig.AssetBundleSuffix);
    }


}
