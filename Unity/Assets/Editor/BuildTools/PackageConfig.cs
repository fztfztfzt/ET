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

public enum BuildType
{
    Development,
    Release,
}

[CreateAssetMenu(fileName = "packageConfig", menuName = "packageConfig", order = 1)]
public class PackageConfig : ScriptableObject
{
    #region ������
    private static PackageConfig _instance;
    public static PackageConfig Instance
    {
        get
        {
            _instance = AssetDatabase.LoadAssetAtPath<PackageConfig>("Assets/Editor/BuildTools/PackConf.asset");
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<PackageConfig>();
                AssetDatabase.CreateAsset(_instance, "Assets/Editor/BuildTools/PackConf.asset");
            }

            return _instance;
        }
    }
    #endregion

    #region ���������Զ���
    [SerializeField, LabelText("AB����")]
    List<PackageItem> packageItems;
    [SerializeField, LabelText("���ƽ̨")]
    BuildTarget targetPlatform = BuildTarget.StandaloneWindows64;
    [SerializeField]
    BuildType buildType = BuildType.Development;

    BuildAssetBundleOptions buildOption = BuildAssetBundleOptions.IgnoreTypeTreeChanges | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression;
    #endregion


    string abExtens = AssetBundleConfig.AssetBundleSuffix;
    string abDirExtens = AssetBundleConfig.AssetBundleSuffix;
    string _targetDir;
    public string TargetDir
    {
        get
        {
            if (string.IsNullOrEmpty(_targetDir))
            {
                _targetDir = $"../Release/{targetPlatform}/StreamingAssets";
            }
            return _targetDir;
        }
    }
    //�ļ����
    public bool PackFile(string res)
    {
        string path = "Assets/" + res;
        AssetImporter importer = AssetImporter.GetAtPath(path);
        if (importer == null)
        {
            Debug.LogError("Path not Exist!" + res);
            return false;
        }
        importer.assetBundleName = res + abExtens;
        return true;
    }

    //���Ŀ¼��������Դ�������ļ����µ������ļ���ÿ���ļ�����һ��AB��
    public bool PackDirFiles(string res, string pattern, SearchOption searchOption)
    {
        string path = "Assets/" + res;
        string[] files = Directory.GetFiles(path, pattern, searchOption);
        if (files.Length == 0) return false;
        foreach (var file in files)
        {
            PackFile(file);
        }
        return true;
    }

    //Ŀ¼���
    public bool PackDir(string res, string pattern, SearchOption searchOption)
    {
        string path = "Assets/" + res;
        string[] files = Directory.GetFiles(path, pattern, searchOption);
        if (files.Length == 0) return false;
        string fn = res.Trim('/') + abDirExtens;

        foreach (var file in files)
        {
            AssetImporter impoter = AssetImporter.GetAtPath(file);
            impoter.assetBundleName = fn;
        }
        return true;
    }

    //���Ŀ¼��������Դ
    public bool PackDirDir(string res, string pattern)
    {
        string path = "Assets/" + res;
        string[] Dirs = Directory.GetDirectories(path);
        if (Dirs.Length == 0) return false;
        foreach (var d in Dirs)
        {
            PackDir(d.Replace('\\', '/'), pattern, SearchOption.AllDirectories);
        }
        return true;
    }
    public static void CreateAssetbundleForCurrent(string assetPath)
    {
        string outputFilePath = AssetBundleConfig.GetAssetPath(assetPath);
        AssetImporter importer = AssetImporter.GetAtPath(outputFilePath);
        if (importer == null)
        {
            Debug.LogError(string.Format("importer null! make sure object at path({0}) is a valid assetbundle!", assetPath));
            return;
        }

        importer.assetBundleName = assetPath + AssetBundleConfig.AssetBundleSuffix;
    }
    public AssetBundleManifest BuildAllAssetRes()
    {
        if (!Directory.Exists(TargetDir)) Directory.CreateDirectory(TargetDir);
        return BuildPipeline.BuildAssetBundles(TargetDir, buildOption, targetPlatform);
    }
    void BuildPathMapping(AssetBundleManifest manifest)
    {
        List<string> mappingList = new List<string>();
        string[] allAssetbundles = manifest.GetAllAssetBundles();
        foreach (var assetbundle in allAssetbundles)
        {
            var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetbundle);
            foreach (var assetPath in assetPaths)
            {
                string mappingItem = string.Format("{0}{1}{2}", assetbundle, AssetBundleConfig.PATTREN, assetPath);
                mappingList.Add(mappingItem);
            }
        }
        mappingList.Sort();//��ÿ�������ļ�������ͬ

        string outputFilePath = AssetBundleConfig.GetAssetPath(AssetBundleConfig.AssetsPathMapFileName);

        if (!GameUtility.SafeWriteAllLines(outputFilePath, mappingList.ToArray()))
        {
            Debug.LogError("BuildPathMapping failed!!! try rebuild it again!");
        }
        else
        {
            AssetDatabase.Refresh();
            CreateAssetbundleForCurrent(AssetBundleConfig.AssetsPathMapFileName);
            Debug.Log("BuildPathMapping success...");
        }
        AssetDatabase.Refresh();
    }

    [Button("ȫ������AB", ButtonSizes.Large)]
    public static void BuildAllResources()
    {
        Log.Info("Start BuildAll");
        Log.Info($"Clear {Instance.TargetDir}");
        GameUtility.SafeClearDir(Instance.TargetDir);
        //Log.Info("start set ab name");
        foreach (var item in Instance.packageItems)
        {
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
                Instance.PackDir(item.path, item.searchPattern, item.searchOption);
            }
        }
        Log.Info("set ab name success,start first build");
        AssetBundleManifest manifest = Instance.BuildAllAssetRes();
        Log.Info("start save maifest mapping");
        Instance.BuildPathMapping(manifest);
        Log.Info("start build all");
        Instance.BuildAllAssetRes();
        Log.Info("build success!!");
        GameUtility.CopyFolder(Instance.TargetDir, AssetBundleConfig.AssetsRootPath, AssetBundleConfig.AssetBundlePattern);
        File.Copy(Path.Combine(Instance.TargetDir, "StreamingAssets"), Path.Combine(AssetBundleConfig.AssetsRootPath, "streamingassets" + AssetBundleConfig.AssetBundleSuffix), true);//����ģʽ
    }

    [Button("��������AB", ButtonSizes.Large)]
    public static void BuildIncrement()
    {

    }

    
    [MenuItem("Tools/SetEditorMode")]
    public static void SwitchEditorMode()
    {
        AssetBundleConfig.IsEditorMode = true;
    }
    [MenuItem("Tools/SetNoEditorMode")]
    public static void SwitchNoEditorMode()
    {
        AssetBundleConfig.IsEditorMode = false;
    }
}
