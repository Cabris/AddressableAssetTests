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

        //[SerializeField]
        public readonly LoadRemoteResourceLocationsEvent OnRemoteResourceLocationsLoaded = new LoadRemoteResourceLocationsEvent();

        public readonly DownloadDependenciesEvent OnDependenciesDownload = new DownloadDependenciesEvent();

        [SerializeField]
        LoadRemoteAssetEvent OnRemotePrefabDownloaded = new LoadRemoteAssetEvent();

        [SerializeField]
        LoadRemoteSceneEvent OnRemoteSceneDownloaded = new LoadRemoteSceneEvent();

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

        public void LoadRemoteAsset(string configUrl, LoadRemoteAssetEvent onLoaded)
        {
            if (_loadedConfigs.ContainsKey(configUrl))
            {
                var config = _loadedConfigs[configUrl];
                if (_loadedPrefabs.ContainsKey(config.AddressableName))
                {
                    var prefab = _loadedPrefabs[config.AddressableName];
                    onLoaded?.Invoke(prefab);
                }
                else//load res, dep, asset
                {
                    LoadAssetResouces(config, onLoaded);
                }
            }
            else
            {
                var handle = OperationManager.LoadCatalogAsync(configUrl);
                handle.Completed += (task) =>
                {
                    Debug.Log("LoadCatalogAsync ==> " + task.Status);
                    if (task.Status == AsyncOperationStatus.Succeeded)
                    {
                        Debug.Log("LoadCatalogAsync [" + configUrl + "] is success, AddressableName: "
                            + task.Result.AddressableName);
                        _loadedConfigs.Add(configUrl, task.Result);
                        //load res, dep, asset
                        LoadAssetResouces(task.Result, onLoaded);
                    }
                    else
                    {
                        Debug.LogError("LoadCatalogAsync [" + configUrl + "] is failed");
                        onLoaded?.Invoke(null);
                    }
                };
            }

        }

        async void LoadAssetResouces(AddressableAssetsConfigs config, LoadRemoteAssetEvent onLoaded)
        {
            Debug.Log("LoadAssetResouces: " + config.AddressableName);

            if (_loadedPrefabs.ContainsKey(config.AddressableName))
            {
                onLoaded?.Invoke(_loadedPrefabs[config.AddressableName]);
            }
            else
            {
                var status = await LoadResourceLocation(config);
                if (status != AsyncOperationStatus.Succeeded)
                    onLoaded?.Invoke(null);

                status = await LoadDependency(config);
                if (status != AsyncOperationStatus.Succeeded)
                    onLoaded?.Invoke(null);

                LoadAssetsPrefab(config, onLoaded);
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

        void LoadAssetsPrefab(AddressableAssetsConfigs configs, LoadRemoteAssetEvent onLoaded)
        {
            string AddressNameStr = configs.AddressableName;

            if (configs.Type == AddressableAssetsConfigs.AssetType.Prefab)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(AddressNameStr);
                handle.Completed += (task) =>
                {
                    Debug.Log("LoadAssetAsync ==> " + task.Status);
                    if (task.Status == AsyncOperationStatus.Succeeded)
                    {
                        var result = task.Result;
                        OnRemotePrefabDownloaded?.Invoke(result);
                        onLoaded?.Invoke(result);
                        _loadedPrefabs.Add(AddressNameStr, result);
                        Debug.Log("LoadAssetAsync is success, Result: " + result);
                    }
                    else
                    {
                        Debug.LogError("LoadAssetAsync is Failed");
                        onLoaded?.Invoke(null);
                    }
                };
            }
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
        public class LoadRemoteAssetEvent : UnityEvent<GameObject>
        { }

        [Serializable]
        public class LoadRemoteSceneEvent : UnityEvent<string>
        { }

    }
}


