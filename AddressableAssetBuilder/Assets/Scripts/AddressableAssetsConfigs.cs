﻿public class AddressableAssetsConfigs
{
    public string AddressableName = "";
    public string WebGL = "";
    public string Android = "";
    public string Type = "";

    public class AssetType
    {
        public const string Scene = "Scene";
        public const string Prefab = "Prefab";
        public const string Video360 = "Video360";
        public const string Video = "Video";
        public const string Image360 = "Image360";
        public const string Image = "Image";
    }
}


public class SlideConfigs
{
    public string Name = "";
    public string[] Models = new string[0];
    public string Video360s = "";
    public string Videos = "";
    public string Images = "";
    public string Image360s = "";
}
