using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WTC.Resource;
using static WTC.Resource.AddressableAssetLoader;

public class AsyncTester : MonoBehaviour
{

    [SerializeField]
    string _slideUrl;

    [SerializeField]
    Transform _root;

    [SerializeField]
    Text _text;

    [SerializeField]
    float _rotateSpeed = 10;

    float _startLoadTime = 0;

    [SerializeField]
    Renderer _imageRenderer;

    [SerializeField]
    UMP.UniversalMediaPlayer _player;

    string[] _slideUrls = {
        "/slides/s0.json",
        "/slides/s1.json",
        "/slides/s2.json",
        "/slides/s3.json"
    };

    public void OnSlideSelected(int index)
    {
        _slideUrl = WTC.Resource.AddressablesConsts.RuntimePath + _slideUrls[index];
    }

    public void OnLoadSlideClick()
    {
        Debug.Log("OnLoadSlideClick: " + _slideUrl);
        StartCoroutine(GetRequest(_slideUrl, OnSlideJsonLoaded));
    }
       
    private void UnloadAll() {

    }

    private void LoadModel(string modelConfigUrl)
    {
        Debug.Log("LoadModel: " + modelConfigUrl);
        var onLoaded = new LoadAssetListener();
        onLoaded.OnPrefabDownloaded.AddListener(OnPrefabLoaded);
        AddressableAssetLoader.Instance.LoadRemoteAsset(modelConfigUrl, onLoaded);
    }

    private void LoadVideo(string videoConfigUrl)
    {
        Debug.Log("LoadVideo: " + videoConfigUrl);
        var onLoaded = new LoadAssetListener();
        onLoaded.OnConfigDownloaded.AddListener(OnConfigLoaded);
        AddressableAssetLoader.Instance.LoadRemoteAsset(videoConfigUrl, onLoaded);
    }

    private void LoadImage(string configUrl)
    {
        Debug.Log("LoadImage: " + configUrl);
        var onLoaded = new LoadAssetListener();
        onLoaded.OnConfigDownloaded.AddListener(OnConfigLoaded);
        AddressableAssetLoader.Instance.LoadRemoteAsset(configUrl, onLoaded);
    }

    private void OnSlideJsonLoaded(SlideConfigs slide)
    {
        Debug.Log("OnSlideJsonLoaded: " + slide);
        if (slide != null)
        {
            Debug.Log("Parse Slide: " + slide.Name);

            Debug.Log("Parse Slide Models: " + slide.Models.Length);
            foreach (var url in slide.Models)
            {
                LoadModel(AddressablesConsts.ParseDynamicPath(url));
            }

            Debug.Log("Parse Slide Videos: " + slide.Video);
            if (slide.Video.Length > 0)
            {
                LoadVideo(AddressablesConsts.ParseDynamicPath(slide.Video));
            }

            Debug.Log("Parse Slide Videos360: " + slide.Video360);
            if (slide.Video360.Length > 0)
            {
                LoadVideo(AddressablesConsts.ParseDynamicPath(slide.Video360));
            }

            Debug.Log("Parse Slide Image: " + slide.Image);
            if (slide.Image.Length > 0)
            {
                LoadImage(AddressablesConsts.ParseDynamicPath(slide.Image));
            }

            Debug.Log("Parse Slide Image360: " + slide.Image360);
            if (slide.Image360.Length > 0)
            {
                LoadImage(AddressablesConsts.ParseDynamicPath(slide.Image360));
            }
        }
    }

    private static IEnumerator GetRequest(string uri, Action<SlideConfigs> onComplete)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                onComplete?.Invoke(null);
            }
            else
            {
                string data = webRequest.downloadHandler.text;
                Debug.Log(pages[page] + ":\nReceived: " + data);

                var jsonObj = JsonUtility.FromJson<SlideConfigs>(data);
                onComplete?.Invoke(jsonObj);
            }
        }
    }

    void OnConfigLoaded(AddressableAssetsConfigs config)
    {
        if (config.Type == AddressableAssetsConfigs.AssetType.Video ||
            config.Type == AddressableAssetsConfigs.AssetType.Video360)
        {
            OnVideoUrlGet(AddressablesConsts.ParseDynamicPath(config.WebGL));
        }

        if (config.Type == AddressableAssetsConfigs.AssetType.Image ||
              config.Type == AddressableAssetsConfigs.AssetType.Image360)
        {
            OnImageUrlGet(AddressablesConsts.ParseDynamicPath(config.WebGL));
        }
    }

    void OnVideoUrlGet(string url)
    {
        Debug.Log("OnVideoUrlGet: " + url);
        _player.Path = url;
        _player.Play();

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

    void OnPrefabLoaded(GameObject prefab)
    {
        Debug.Log("OnPrefabLoaded: " + prefab);
        if (prefab != null)
        {
            Vector3 pos = new Vector3(UnityEngine.Random.Range(1, -1), 0, UnityEngine.Random.Range(1, -1)) * 2f;
            var go = Instantiate(prefab, _root, false);
            go.transform.localPosition = pos;
        }
        else
            Debug.LogError("prefab is null");
    }



    private void Update()
    {
        _root.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);
    }

}
