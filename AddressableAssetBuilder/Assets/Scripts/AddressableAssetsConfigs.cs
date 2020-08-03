using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddressableAssetsConfigs
{
    public string AddressableName = "";
    public string WebGL = "";
    public string Android = "";
    public string Type = "";

    public class AssetType
    {
        public const string Scene = "Scene";
        public const string Prefab = "Prefab";

    }
}
