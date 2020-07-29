using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor.Build;
using System.Collections.Generic;
using System.Linq;
using System;

class BuildAddressablesProcessor
{
    [MenuItem("Assets/AddressableAsset/BuildBundles with Prefab")]
    static void BuildBundlesWithPrefab()
    {
        var target = Selection.activeObject;
        PrefabAssetType type = PrefabUtility.GetPrefabAssetType(target);
        //Debug.Log("BuildBundles with prefab, target type: " + type);
        if (type == PrefabAssetType.NotAPrefab)
        {
            Debug.LogError("select object must be a prefab!!");
            return;
        }

        string path = AssetDatabase.GetAssetPath(target);
        //Debug.Log("path: " + path);
        string guid = AssetDatabase.AssetPathToGUID(path);

        string settingPath = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        var settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(settingPath);

        var groups = settings.groups.ToArray();
        foreach (var g in groups)
        {
            if (!g.IsDefaultGroup())
                settings.RemoveGroup(g);
        }

        string groupName = "Group_" + guid;
        var group = settings.FindGroup(groupName);
        if (group == null)
            group = settings.CreateGroup(groupName, false, false, true, settings.DefaultGroup.Schemas, settings.DefaultGroup.SchemaTypes.ToArray());

        var entry = settings.CreateOrMoveEntry(guid, group);
        var address = entry.address;

        SwitchWebGL();
        var webGlJson = CleanAndBuildAssets();
        var remoteLoadPath = settings.RemoteCatalogLoadPath.GetValue(settings);
        Debug.Log("SwitchWebGL remoteLoadPath: " + remoteLoadPath);
        webGlJson = remoteLoadPath + "/" + webGlJson;

        SwitchAndroid();
        var androidJson = CleanAndBuildAssets();
        remoteLoadPath = settings.RemoteCatalogLoadPath.GetValue(settings);
        Debug.Log("SwitchAndroid remoteLoadPath: " + remoteLoadPath);
        androidJson = remoteLoadPath + "/" + androidJson;


        AddressableAssetsConfigs configs = new AddressableAssetsConfigs();
        configs.WebGL = webGlJson;
        configs.Android = androidJson;
        configs.AddressableName = address;

        string jsonStr = JsonUtility.ToJson(configs);

        string assetPath = Application.dataPath;
        var indexPath = assetPath.Replace("/Assets", "") + "/ServerData/Indexes/" + address.Replace("/", "_") + ".json";
        Debug.Log("write json data path: " + indexPath);

        using (StreamWriter file =
            new StreamWriter(indexPath, false))
        {
            file.WriteLine(jsonStr);
        }


    }

    [MenuItem("Switch Platform/WebGL")]
    static void SwitchWebGL()
    {
        // Switch to WebGLbuild.
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
    }

    [MenuItem("Switch Platform/Android")]
    static void SwitchAndroid()
    {
        // Switch to WebGLbuild.
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
    }

    /*[MenuItem("AddressableAsset/BuildBundles")]
    static private void BuildBundles()
    {
        string settingPath = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        AddressableAssetSettings settings = (AddressableAssetSettings)
            AssetDatabase.LoadAssetAtPath(settingPath, typeof(AddressableAssetSettings));

        //string t = AssetDatabase.AssetPathToGUID("Assets/models/elephant.fbx");
        //settings.CreateOrMoveEntry(t, settings.DefaultGroup);

        //List<AddressableAssetEntry> assetEntries = new List<AddressableAssetEntry>();
        //settings.DefaultGroup.GatherAllAssets(assetEntries, true, false, false);


        SwitchWebGL();
        var webGlJson = settings.RemoteCatalogLoadPath + CleanAndBuildAssets();

        SwitchAndroid();
        var androidJson = settings.RemoteCatalogLoadPath + CleanAndBuildAssets();

        AddressableAssetsConfigs configs = new AddressableAssetsConfigs();
        configs.WebGL = webGlJson;
        configs.Android = androidJson;


        //List<AddressableAssetEntry> assetEntries = new List<AddressableAssetEntry>();
        //AddressableAssetSettings.DefaultGroup
        //AddressableAssetGroup.GatherAllAssets(assetEntries, true, false, false);
    }*/

    /// <summary>
    /// Run a clean build before export.
    /// </summary>
    //[MenuItem("AddressableAsset/Clean&Build Assets")]
    static private string CleanAndBuildAssets()
    {
        string assetPath = Application.dataPath;
        var bundlePath = assetPath.Replace("/Assets", "") + "/ServerData";
        string target = EditorUserBuildSettings.activeBuildTarget.ToString();
        bundlePath = bundlePath + "/" + target;
        //Debug.Log("bundlePath: " + bundlePath);

        try
        {
            if (Directory.Exists(bundlePath))
            {
                Directory.Delete(bundlePath, true);
            }

            Debug.Log("BuildAddressablesProcessor.BuildAssets[" + target + "] start");
            AddressableAssetSettings.CleanPlayerContent(
                AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
            AddressableAssetSettings.BuildPlayerContent();
            Debug.Log("BuildAddressablesProcessor.BuildAssets[" + target + "] done");

            string[] files = System.IO.Directory.GetFiles(bundlePath, "*.json");

            return Path.GetFileName(files[0]);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            throw;
        }
    }

    //[Serializable]
    public class AddressableAssetsConfigs
    {
        public string AddressableName = "";
        public string WebGL = "";
        public string Android = "";
    }
}