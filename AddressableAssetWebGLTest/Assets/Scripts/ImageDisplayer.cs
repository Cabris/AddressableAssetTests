using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WTC.Resource;

public class ImageDisplayer : MonoBehaviour, ILoaderListener
{
    [SerializeField]
    Renderer _imageRenderer;

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
    }

    private void OnStartConfigLoad(string obj)
    {
        _jobStater.AddJob();
    }

    void OnConfigLoaded(AddressableAssetsConfigs config)
    {
        if (config.Type == AddressableAssetsConfigs.AssetType.Image ||
              config.Type == AddressableAssetsConfigs.AssetType.Image360)
        {
            OnImageUrlGet(AddressablesConsts.ParseDynamicPath(config.WebGL));
        }
    }

    void OnImageUrlGet(string url)
    {
        Debug.Log("OnImageUrlGet: " + url);
        StartCoroutine(DownloadImage(url, OnTextureLoad));
    }

    void OnTextureLoad(Texture2D texture)
    {
        Debug.Log("OnTextureLoad: " + texture.width + ", " + texture.height);
        _imageRenderer.material.mainTexture = texture;
        _jobStater.FinishJob();
    }

    IEnumerator DownloadImage(string MediaUrl, Action<Texture2D> onComplete)
    {
        Debug.Log("DownloadImage: " + MediaUrl);

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.LogError(request.error);
        else
        {
            Debug.Log("DownloadImage Done: " + request.downloadedBytes);
            var tex = DownloadHandlerTexture.GetContent(request);
            onComplete?.Invoke(tex);
        }
    }

    public void Unload()
    {
        _imageRenderer.material.mainTexture = null;
        _jobStater.Reset();
    }
}
