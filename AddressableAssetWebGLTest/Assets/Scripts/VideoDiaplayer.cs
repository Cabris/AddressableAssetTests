using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WTC.Resource;

public class VideoDiaplayer : MonoBehaviour, ILoaderListener
{
    [SerializeField]
    UMP.UniversalMediaPlayer _player;
    [SerializeField]
    JobStater _jobStater;

    public Action<GameObject> OnPrefabDownloaded { get; set; }
    public Action<AddressableAssetsConfigs> OnConfigDownloaded { get; set; }
    public Action OnLoadFail { get; set; }
    public Action<string> OnStartConfigDownload { get; set; }

    private void Awake()
    {
        OnConfigDownloaded += OnConfigLoaded;
        OnStartConfigDownload += OnStartConfigLoad;
        _player.AddPlayingEvent(OnPlaying);
    }

    private void OnStartConfigLoad(string obj)
    {
        _jobStater.AddJob();
    }

    void OnConfigLoaded(AddressableAssetsConfigs config)
    {
        if (config.Type == AddressableAssetsConfigs.AssetType.Video ||
            config.Type == AddressableAssetsConfigs.AssetType.Video360)
        {
            OnVideoUrlGet(AddressablesConsts.ParseDynamicPath(config.WebGL));
        }
    }

    void OnVideoUrlGet(string url)
    {
        Debug.Log("OnVideoUrlGet: " + url);
        _player.Path = url;
        _player.Play();
    }

    void OnPlaying()
    {
        _jobStater.FinishJob();
    }

    public void Unload()
    {
        _player.Stop();
        _jobStater.Reset();
    }

}
