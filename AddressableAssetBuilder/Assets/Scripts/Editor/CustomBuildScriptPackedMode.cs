using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.Build.Pipeline;

//namespace WTC

/// <summary>
/// Build scripts used for player builds and running with bundles in the editor.
/// </summary>
[CreateAssetMenu(fileName = "CustomBuildScriptPacked.asset", menuName = "Addressables/Content Builders/Custom Default Build Script")]
public class CustomBuildScriptPackedMode : BuildScriptPackedMode
{
    /// <inheritdoc />
    public override string Name
    {
        get
        {
            return "Custom Build Script";
        }
    }

    protected override string ConstructAssetBundleName(AddressableAssetGroup assetGroup,
        BundledAssetGroupSchema schema, BundleDetails info, string assetBundleName)
    {
        return assetBundleName + "AAA.xxx";
        //return base.ConstructAssetBundleName(assetGroup, schema, info, assetBundleName);
    }

    protected override TResult DoBuild<TResult>(AddressablesDataBuilderInput builderInput, AddressableAssetsBuildContext aaContext)
    {
        return base.DoBuild<TResult>(builderInput, aaContext);
    }

}

