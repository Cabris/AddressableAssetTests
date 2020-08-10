using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WTC.Resource;

public class AsyncTester : MonoBehaviour
{
    [SerializeField]
    ModelDiaplayer _modelDiaplayer;

    [SerializeField]
    ImageDisplayer _imageDisplayer;

    [SerializeField]
    VideoDiaplayer _videoDiaplayer;

    [SerializeField]
    string _slideUrl;

    string[] _slideUrls = {
        "/slides/s0.json",
        "/slides/s1.json",
        "/slides/s2.json",
        "/slides/s3.json"
    };

    int _index = 0;

    private void Awake()
    {
        AddressablesConsts.RuntimePath = "http://10.222.132.159:7777";
    }

    public void OnRuntimePathChanged(string path)
    {
        AddressablesConsts.RuntimePath = path;
    }

    public void OnSlideSelected(int index)
    {
        _index = index;
    }

    public void OnLoadSlideClick()
    {
        _slideUrl = AddressablesConsts.RuntimePath + _slideUrls[_index];
        Debug.Log("OnLoadSlideClick: " + _slideUrl);
        UnloadAll();
        OperationManager.GetJsonRequest<SlideConfigs>(_slideUrl, OnSlideJsonLoaded);
    }

    public void UnloadAll()
    {
        Debug.Log("UnloadAll");

        _modelDiaplayer.Unload();
        _imageDisplayer.Unload();
        _videoDiaplayer.Unload();
    }

    public void Preview()
    {
        _modelDiaplayer.Preview();
        _videoDiaplayer.Preview();
    }

    public void StopPreview()
    {
        _modelDiaplayer.StopPreview();
        _videoDiaplayer.StopPreview();
    }

    private void LoadModel(string modelConfigUrl)
    {
        Debug.Log("LoadModel: " + modelConfigUrl);
        AddressableAssetLoader.Instance.LoadRemoteAsset(modelConfigUrl, _modelDiaplayer);
    }

    private void LoadVideo(string videoConfigUrl)
    {
        Debug.Log("LoadVideo: " + videoConfigUrl);
        AddressableAssetLoader.Instance.LoadRemoteAsset(videoConfigUrl, _videoDiaplayer);
    }

    private void LoadImage(string configUrl)
    {
        Debug.Log("LoadImage: " + configUrl);
        AddressableAssetLoader.Instance.LoadRemoteAsset(configUrl, _imageDisplayer);
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
                LoadModel(url);
            }

            Debug.Log("Parse Slide Videos: " + slide.Video);
            if (slide.Video.Length > 0)
            {
                LoadVideo(slide.Video);
            }

            Debug.Log("Parse Slide Videos360: " + slide.Video360);
            if (slide.Video360.Length > 0)
            {
                LoadVideo(slide.Video360);
            }

            Debug.Log("Parse Slide Image: " + slide.Image);
            if (slide.Image.Length > 0)
            {
                LoadImage(slide.Image);
            }

            Debug.Log("Parse Slide Image360: " + slide.Image360);
            if (slide.Image360.Length > 0)
            {
                LoadImage(slide.Image360);
            }
        }
    }


}
