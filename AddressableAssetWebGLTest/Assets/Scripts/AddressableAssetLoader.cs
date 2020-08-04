using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Events;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;


namespace WTC.Resource
{

    public class AddressableAssetLoader : Singleton<AddressableAssetLoader>
    {
        Dictionary<string, AddressableAssetsConfigs> _loadedConfigs;

        Dictionary<string, GameObject> _loadedPrefabs;

        [SerializeField]
        bool _inited = false;

        [SerializeField]
        AddressableAssetLoaderEvent OnAALoaderInited = new AddressableAssetLoaderEvent();

        [SerializeField]
        AddressableAssetLoaderEvent OnAALoaderInitFailed = new AddressableAssetLoaderEvent();

        [SerializeField]
        LoadRemoteResourceLocationsEvent OnRemoteResourceLocationsLoaded = new LoadRemoteResourceLocationsEvent();
        [SerializeField]
        DownloadDependenciesEvent OnDependenciesDownload = new DownloadDependenciesEvent();

        public bool Inited { get => _inited; private set => _inited = value; }

        void Start()
        {
            Inited = false;
            InitAddressables();
            _loadedConfigs = new Dictionary<string, AddressableAssetsConfigs>();
            _loadedPrefabs = new Dictionary<string, GameObject>();
        }

        void InitAddressables()
        {
            Debug.Log("initAddressables");
            AddressablesConsts.RuntimePath = "http://localhost:8887";
            AsyncOperationHandle<IResourceLocator> handle = Addressables.InitializeAsync();
            handle.Completed += (obj) =>
            {
                Debug.Log("Initialization Complete ==> " + obj.Status);
                if (obj.Status == AsyncOperationStatus.Succeeded)
                {
                    Inited = true;
                    OnAALoaderInited?.Invoke(this);
                }
                else
                {
                    OnAALoaderInitFailed?.Invoke(this);
                    Debug.LogError("InitializeAsync is failed");
                }
            };
        }

        public void LoadRemoteAsset(string configUrl, LoadAssetListener onLoaded)
        {
            if (_loadedConfigs.ContainsKey(configUrl))
            {
                var config = _loadedConfigs[configUrl];
                Debug.Log("reuse _loadedConfigs: " + configUrl);
                onLoaded.OnConfigDownloaded?.Invoke(config);

                if (config.Type == AddressableAssetsConfigs.AssetType.Prefab)
                    LoadAssetResouces(config, onLoaded);
            }
            else
            {
                var handle = OperationManager.LoadCatalogAsync(configUrl);
                handle.Completed += (task) =>
                {
                    Debug.Log("LoadCatalogAsync ==> " + task.Status);
                    if (task.Status == AsyncOperationStatus.Succeeded)
                    {
                        var config = task.Result;
                        Debug.Log("LoadCatalogAsync [" + configUrl + "] is success, AddressableName: "
                            + config.AddressableName);
                        _loadedConfigs.Add(configUrl, config);
                        onLoaded.OnConfigDownloaded?.Invoke(config);
                        if (task.Result.Type == AddressableAssetsConfigs.AssetType.Prefab)
                            LoadAssetResouces(task.Result, onLoaded);
                    }
                    else
                    {
                        Debug.LogError("LoadCatalogAsync [" + configUrl + "] is failed");
                        onLoaded.OnLoadFail?.Invoke();
                    }
                };
            }

        }

        async void LoadAssetResouces(AddressableAssetsConfigs config, LoadAssetListener onLoaded)
        {
            Debug.Log("LoadAssetResouces: " + config.AddressableName);

            if (config.Type == AddressableAssetsConfigs.AssetType.Prefab)
            {
                if (_loadedPrefabs.ContainsKey(config.AddressableName))
                {
                    onLoaded.OnPrefabDownloaded?.Invoke(_loadedPrefabs[config.AddressableName]);
                    Debug.Log("reuse _loadedPrefabs: " + config.AddressableName);
                }
                else
                {
                    var status = await LoadResourceLocation(config);
                    if (status != AsyncOperationStatus.Succeeded)
                    {
                        onLoaded.OnLoadFail?.Invoke();
                        return;
                    }

                    status = await LoadDependency(config);
                    if (status != AsyncOperationStatus.Succeeded)
                    {
                        onLoaded.OnLoadFail?.Invoke();
                        return;
                    }

                    LoadAssetsPrefab(config, onLoaded);

                }
            }


        }

        async Task<AsyncOperationStatus> LoadResourceLocation(AddressableAssetsConfigs configs)
        {
            Debug.Log("LoadResourceLocation");
            AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(configs.AddressableName);
            bool isCompleted = false;

            handle.Completed += (task) =>
            {
                Debug.Log("LoadResourceLocationsAsync ==> " + task.Status);
                if (task.Status == AsyncOperationStatus.Succeeded)
                {
                    OnRemoteResourceLocationsLoaded?.Invoke(task.Result);
                }
                else
                {
                    Debug.LogError("LoadResourceLocationsAsync is failed");
                }
                isCompleted = true;
            };
            await new WaitUntil(() => isCompleted);
            return handle.Status;
        }

        async Task<AsyncOperationStatus> LoadDependency(AddressableAssetsConfigs configs)
        {
            Debug.Log("LoadDependency");
            AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(configs.AddressableName);
            bool isCompleted = false;
            handle.Completed += (task) =>
            {
                Debug.Log("DownloadDependenciesAsync ==> " + task.Status);
                if (task.Status == AsyncOperationStatus.Succeeded)
                {
                    OnDependenciesDownload?.Invoke(configs.AddressableName);
                }
                else
                {
                    Debug.LogError("DownloadDependenciesAsync is Failed");
                }
                isCompleted = true;
            };
            await new WaitUntil(() => isCompleted);
            return handle.Status;
        }

        void LoadAssetsPrefab(AddressableAssetsConfigs configs, LoadAssetListener onLoaded)
        {
            string AddressNameStr = configs.AddressableName;
                        
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(AddressNameStr);
            handle.Completed += (task) =>
            {
                Debug.Log("LoadAssetAsync ==> " + task.Status);
                if (task.Status == AsyncOperationStatus.Succeeded)
                {
                    var result = task.Result;
                    onLoaded.OnPrefabDownloaded?.Invoke(result);
                    _loadedPrefabs.Add(AddressNameStr, result);
                    Debug.Log("LoadAssetAsync is success, Result: " + result);
                }
                else
                {
                    Debug.LogError("LoadAssetAsync is Failed");
                    onLoaded.OnLoadFail?.Invoke();
                }
            };

        }

    }

    [Serializable]
    public class LoadAssetListener
    {
        public LoadRemotePrefabEvent OnPrefabDownloaded = new LoadRemotePrefabEvent();

        public LoadRemoteConfigEvent OnConfigDownloaded = new LoadRemoteConfigEvent();

        public UnityEvent OnLoadFail = new UnityEvent();
    }

    [Serializable]
    public class AddressableAssetLoaderEvent : UnityEvent<AddressableAssetLoader>
    { }

    [Serializable]
    public class LoadRemoteResourceLocationsEvent : UnityEvent<IList<IResourceLocation>>
    { }

    [Serializable]
    public class DownloadDependenciesEvent : UnityEvent<string>
    { }

    [Serializable]
    public class LoadRemotePrefabEvent : UnityEvent<GameObject>
    { }

    [Serializable]
    public class LoadRemoteConfigEvent : UnityEvent<AddressableAssetsConfigs>
    { }

}


