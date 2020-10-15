using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WTC.Resource;

public class ModelDiaplayer : MonoBehaviour, ILoaderListener
{
    [SerializeField]
    Transform _root;

    [SerializeField]
    float _rotateSpeed = 10;

    [SerializeField]
    private List<AddressableAssetsConfigs> _configs = new List<AddressableAssetsConfigs>();

    [SerializeField]
    JobStater _jobStater;

    List<GameObject> _instances = new List<GameObject>();

    public Action<GameObject> OnPrefabDownloaded { get; set; }
    public Action<AddressableAssetsConfigs> OnConfigDownloaded { get; set; }
    public Action OnLoadFail { get; set; }
    public Action<string> OnStartConfigDownload { get; set; }

    private void Awake()
    {
        OnPrefabDownloaded += OnPrefabLoaded;
        OnConfigDownloaded += OnConfigLoaded;
        OnStartConfigDownload += OnStartConfigLoad;
    }

    private void OnStartConfigLoad(string obj)
    {
        _jobStater.AddJob();
    }

    private void OnConfigLoaded(AddressableAssetsConfigs config)
    {
        _configs.Add(config);
    }

    void OnPrefabLoaded(GameObject prefab)
    {
        Debug.Log("OnPrefabLoaded: " + prefab);
        if (prefab != null)
        {
            Vector3 pos = new Vector3(UnityEngine.Random.Range(1, -1), 0, UnityEngine.Random.Range(1, -1)) * 0f;
            var go = Instantiate(prefab, _root, false);
            go.transform.localPosition = pos;
            _instances.Add(go);

            var at = go.GetComponent<Animator>();
            if (at != null)
                at.speed = 0;

            var ani = go.GetComponent<Animation>();
            if (ani != null)
            {
                ani.Stop();
            }
        }
        else
            Debug.LogError("prefab is null");

        _jobStater.FinishJob();
    }

    void Update()
    {
        _root.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);
    }

    public void Unload()
    {
        foreach (var i in _instances)
            Destroy(i);
        _instances.Clear();

        foreach (var c in _configs)
            AddressableAssetLoader.Instance.UnloadAsset(c.AddressableName);
        _configs.Clear();

        _jobStater.Reset();
    }

    public void Preview()
    {
        foreach (var i in _instances)
        {
            var at = i.GetComponent<Animator>();
            if (at != null)
                at.speed=1;
            var ani = i.GetComponent<Animation>();
            if (ani != null)
            {
                ani.Play();
            }
        }
    }

    public void StopPreview()
    {
        foreach (var i in _instances)
        {
            var at = i.GetComponent<Animator>();
            if (at != null)
                at.speed = 0;
            var ani = i.GetComponent<Animation>();
            if (ani != null)
            {
                ani.Stop();
            }
        }
    }

}


[Serializable]
public class JobStater
{
    [SerializeField]
    int _jobCount;
    [SerializeField]
    Text _text;
    float _startLoadTime = 0;

    public int JobCount { get => _jobCount; }

    public void AddJob()
    {
        if (_jobCount == 0)
        {
            _startLoadTime = Time.time;
            _text.text = "Loading...";
        }
        _jobCount += 1;
    }

    public void FinishJob()
    {
        _jobCount -= 1;

        if (_jobCount == 0)
        {
            var t = Time.time - _startLoadTime;
            _text.text = "Loading...Done in " + t + "s.";
        }
    }

    public void Reset()
    {
        _jobCount = 0;
        _text.text = "";
    }
}