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
    Renderer _imageRenderer, _image360Renderer;

    [SerializeField]
    JobStater _jobStater;

    public Action<GameObject> OnPrefabDownloaded { get; set; }
    public Action<AddressableAssetsConfigs> OnConfigDownloaded { get; set; }
    public Action OnLoadFail { get; set; }
    public Action<string> OnStartConfigDownload { get; set; }

    private void Awake()
    {
        _imageRenderer.enabled = false;
        _image360Renderer.enabled = false;
        OnConfigDownloaded += OnConfigLoaded;
        OnStartConfigDownload += OnStartConfigLoad;
    }

    private void OnStartConfigLoad(string obj)
    {
        _jobStater.AddJob();
    }

    void OnConfigLoaded(AddressableAssetsConfigs config)
    {
        var path = config.CatalogPath;

        if (config.Type == AddressableAssetsConfigs.AssetType.Video)
        {
            _player.RenderingObjects = new GameObject[] { _imageRenderer.gameObject };
            _imageRenderer.enabled = true;
            OnVideoUrlGet(path);
        }

        if (config.Type == AddressableAssetsConfigs.AssetType.Video360)
        {
            _player.RenderingObjects = new GameObject[] { _image360Renderer.gameObject };
            _image360Renderer.enabled = true;
            OnVideoUrlGet(path);
        }

    }

    void OnVideoUrlGet(string url)
    {
        Debug.Log("OnVideoUrlGet: " + url);
        _player.Path = url;
        _player.Prepare();
        _jobStater.FinishJob();
    }

    public void Unload()
    {
        _imageRenderer.enabled = false;
        _image360Renderer.enabled = false;
        _player.Stop();
        _player.Path = "";
        _jobStater.Reset();
    }

    public void Preview()
    {
        if (_player.Path.Length > 0)
            _player.Play();
    }

    public void StopPreview()
    {
        _player.Stop();
    }
}
