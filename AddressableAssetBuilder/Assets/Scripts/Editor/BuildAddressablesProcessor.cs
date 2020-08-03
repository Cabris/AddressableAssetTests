using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.IO;


namespace WTC
{
    public static class AddressablesConstants
    {
        public static string BundleRootID = "DataRoot";
        public static string SettingPath = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        public static string AssetNameID = "AssetName";

    }

}

static class BuildAddressablesProcessor
{

    [MenuItem("Assets/AddressableAsset/BuildBundles with Prefab")]
    static void BuildBundlesWithPrefab()
    {
        var target = Selection.activeObject;
        PrefabAssetType type = PrefabUtility.GetPrefabAssetType(target);

        if (type == PrefabAssetType.NotAPrefab)
        {
            Debug.LogError("select object must be a prefab!!");
            return;
        }

        if (type == PrefabAssetType.MissingAsset)
        {
            Debug.LogError("select object MissingAsset!");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(target);
        string guid = AssetDatabase.AssetPathToGUID(assetPath);

        AddressableAssetSettings settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(WTC.AddressablesConstants.SettingPath);
        var profile = settings.profileSettings;
        var profileId = settings.activeProfileId;
        var buildedDataRoot = profile.GetValueByName(profileId, WTC.AddressablesConstants.BundleRootID);

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
        var entryName = address.Replace("/", "_");

        profile.SetValue(profileId, WTC.AddressablesConstants.AssetNameID, entryName);

        SwitchWebGL();
        var webGlJson = CleanAndBuildAssets(settings, buildedDataRoot);
        var remoteLoadPath = settings.RemoteCatalogLoadPath.GetValue(settings);
        Debug.Log("SwitchWebGL remoteLoadPath: " + remoteLoadPath);
        webGlJson = remoteLoadPath + "/" + webGlJson;

        SwitchAndroid();
        var androidJson = CleanAndBuildAssets(settings, buildedDataRoot);
        remoteLoadPath = settings.RemoteCatalogLoadPath.GetValue(settings);
        Debug.Log("SwitchAndroid remoteLoadPath: " + remoteLoadPath);
        androidJson = remoteLoadPath + "/" + androidJson;


        string dataPath = Application.dataPath;
        var bundleRootPath = dataPath.Replace("/Assets", "") + "/" + buildedDataRoot;

        var entryPath = bundleRootPath + "/" + entryName;

        if (Directory.Exists(entryPath))
        {
            Directory.Delete(entryPath, true);
        }
        Directory.CreateDirectory(entryPath);

        var sourcePath = bundleRootPath + "/" + BuildTarget.WebGL.ToString();
        if (Directory.Exists(sourcePath))
        {
            var targetPath = entryPath + "/" + BuildTarget.WebGL.ToString();
            Directory.Move(sourcePath, targetPath);
            Debug.Log("Directory.Move: " + sourcePath + " to " + targetPath);
        }

        sourcePath = bundleRootPath + "/" + BuildTarget.Android.ToString();
        if (Directory.Exists(sourcePath))
        {
            var targetPath = entryPath + "/" + BuildTarget.Android.ToString();
            Directory.Move(sourcePath, targetPath);
            Debug.Log("Directory.Move: " + sourcePath + " to " + targetPath);
        }

        AddressableAssetsConfigs configs = new AddressableAssetsConfigs();
        configs.WebGL = webGlJson;
        configs.Android = androidJson;
        configs.AddressableName = address;
        configs.Type = AddressableAssetsConfigs.AssetType.Prefab;
        string jsonStr = JsonUtility.ToJson(configs);

        string indexesPath = bundleRootPath + "/Indexes/"; // your code goes here
        bool exists = Directory.Exists(indexesPath);
        if (!exists)
        {
            Debug.Log("CreateDirectory: " + indexesPath);
            Directory.CreateDirectory(indexesPath);
        }

        var indexPath = indexesPath + entryName + ".json";
        Debug.Log("write json data path: " + indexPath);

        using (StreamWriter file =
            new StreamWriter(indexPath, false))
        {
            file.WriteLine(jsonStr);
        }

        profile.SetValue(profileId, WTC.AddressablesConstants.AssetNameID, "default");

        //need upload to server
        //entryPath, indexPath
        //
    }


    static private string CleanAndBuildAssets(AddressableAssetSettings settings, string DataRoot)
    {
        string assetPath = Application.dataPath;
        string target = EditorUserBuildSettings.activeBuildTarget.ToString();
        var bundlePath = assetPath.Replace("/Assets", "") + "/" + DataRoot;
        bundlePath = bundlePath + "/" + target;
        Debug.Log("bundlePath: " + bundlePath);

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

            string[] files = Directory.GetFiles(bundlePath, "*.json");

            return Path.GetFileName(files[0]);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            throw;
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
}