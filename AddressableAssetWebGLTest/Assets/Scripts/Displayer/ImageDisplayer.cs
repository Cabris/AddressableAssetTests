using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using WTC.Resource;

public class ImageDisplayer : MonoBehaviour, ILoaderListener
{
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
        if (config.Type == AddressableAssetsConfigs.AssetType.Image)
            OnImageUrlGet(path, _imageRenderer);

        if (config.Type == AddressableAssetsConfigs.AssetType.Image360)
            OnImageUrlGet(path, _image360Renderer);
    }

    void OnImageUrlGet(string url, Renderer renderer)
    {
        Debug.Log("OnImageUrlGet: " + url);
        StartCoroutine(DownloadImage(url, (texture) =>
        {
            Debug.Log("OnTextureLoad: " + texture.width + ", " + texture.height);
            renderer.material.mainTexture = texture;
            renderer.enabled = true;
            _jobStater.FinishJob();
        }));
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
        _imageRenderer.enabled = false;
        _image360Renderer.enabled = false;
        _jobStater.Reset();
    }
}
