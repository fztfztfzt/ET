using ET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AssetBundles
{
    public class ResourcesMapItem
    {
        public string assetbundleName;
        public string assetName;
    }
    class AssetsPathMapping
    {
        protected Dictionary<string, ResourcesMapItem> pathLookup = new Dictionary<string, ResourcesMapItem>();
        public AssetsPathMapping()
        {
            AssetName = AssetBundleConfig.AssetsPathMapFileName;
            AssetbundleName = AssetName.ToLower();
        }

        public string AssetbundleName
        {
            get;
            protected set;
        }

        public string AssetName
        {
            get;
            protected set;
        }

        public void Initialize(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                Log.Error("ResourceNameMap empty!!");
                return;
            }

            content = content.Replace("\r\n", "\n");
            string[] mapList = content.Split('\n');
            foreach (string map in mapList)
            {
                if (string.IsNullOrEmpty(map))
                {
                    continue;
                }

                string[] splitArr = map.Split(new[] { AssetBundleConfig.PATTREN }, System.StringSplitOptions.None);
                if (splitArr.Length < 2)
                {
                    Log.Error("splitArr length < 2 : " + map);
                    continue;
                }

                ResourcesMapItem item = new ResourcesMapItem();
                // 如：ui/prefab/assetbundleupdaterpanel_prefab.assetbundle
                item.assetbundleName = splitArr[0];
                // 如：UI/Prefab/AssetbundleUpdaterPanel.prefab
                item.assetName = splitArr[1];

                var assetPath = item.assetName;
                pathLookup.Add(assetPath, item);
                Debug.Log($"{assetPath}-{item.assetbundleName}");
                //List<string> assetsList = null;
                //assetsLookup.TryGetValue(item.assetbundleName, out assetsList);
                //if (assetsList == null)
                //{
                //    assetsList = new List<string>();
                //}
                //if (!assetsList.Contains(item.assetName))
                //{
                //    assetsList.Add(item.assetName);
                //}
                //assetsLookup[item.assetbundleName] = assetsList;
                //assetbundleLookup.Add(item.assetName, item.assetbundleName);
            }
        }
        public string GetAssetBundleName(string assetName)
        {
            if(pathLookup.TryGetValue(assetName, out var assetbundleInfo))
            {
                return assetbundleInfo.assetbundleName;
            }
            return null;
        }
    }
}
