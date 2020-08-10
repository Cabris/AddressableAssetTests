using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using ResLocatsHandle = UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<System.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>>;

namespace WTC.Resource
{

    public class AddressableAssetLoader : Utility.Singleton<AddressableAssetLoader>
    {
        //<url, config>
        Dictionary<string, AddressableAssetsConfigs> _loadedConfigs;
        //<key, prefab>
        Dictionary<string, GameObject> _loadedPrefabs;
        Dictionary<string, ResLocatsHandle> _loadedResLocats;
        Dictionary<string, AsyncOperationHandle> _loadedDeps;

        [SerializeField]
        bool _inited = false;

        [SerializeField]
        AddressableAssetLoaderEvent OnAALoaderInited = new AddressableAssetLoaderEvent();

        [SerializeField]
        AddressableAssetLoaderEvent OnAALoaderInitFailed = new AddressableAssetLoaderEvent();


        public bool Inited { get => _inited; private set => _inited = value; }

        void Start()
        {
            Inited = false;
            InitAddressables();
            _loadedConfigs = new Dictionary<string, AddressableAssetsConfigs>();
            _loadedPrefabs = new Dictionary<string, GameObject>();
            _loadedResLocats = new Dictionary<string, ResLocatsHandle>();
            _loadedDeps = new Dictionary<string, AsyncOperationHandle>();
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

        public void LoadRemoteAsset(string configUrl, ILoaderListener loadTask)
        {
            loadTask.OnStartConfigDownload?.Invoke(configUrl);
            if (_loadedConfigs.ContainsKey(configUrl))
            {
                var config = _loadedConfigs[configUrl];
                Debug.Log("reuse _loadedConfigs: " + configUrl);
                loadTask.OnConfigDownloaded?.Invoke(config);

                if (config.Type == AddressableAssetsConfigs.AssetType.Prefab)
                    LoadAssetResouces(config, loadTask);
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
                        loadTask.OnConfigDownloaded?.Invoke(config);
                        if (task.Result.Type == AddressableAssetsConfigs.AssetType.Prefab)
                            LoadAssetResouces(task.Result, loadTask);
                    }
                    else
                    {
                        Debug.LogError("LoadCatalogAsync [" + configUrl + "] is failed");
                        loadTask.OnLoadFail?.Invoke();
                    }
                };
            }

        }

        private void LoadAssetResouces(AddressableAssetsConfigs config, ILoaderListener loadTask)
        {
            Debug.Log("LoadAssetResouces: " + config.AddressableName);

            if (config.Type == AddressableAssetsConfigs.AssetType.Prefab)
            {
                if (_loadedPrefabs.ContainsKey(config.AddressableName))
                {
                    loadTask.OnPrefabDownloaded?.Invoke(_loadedPrefabs[config.AddressableName]);
                    Debug.Log("reuse _loadedPrefabs: " + config.AddressableName);
                }
                else
                {
                    var resHandle = LoadResourceLocation(config);
                    resHandle.Completed += (task) =>
                    {
                        if (resHandle.Status != AsyncOperationStatus.Succeeded)
                        {
                            loadTask.OnLoadFail?.Invoke();
                            return;
                        }

                        var depHandle = LoadDependency(config);
                        depHandle.Completed += (task1) =>
                        {
                            if (depHandle.Status == AsyncOperationStatus.Succeeded)
                                LoadAssetsPrefab(config, loadTask);
                            loadTask.OnLoadFail?.Invoke();
                        };
                    };
                }
            }


        }

        private ResLocatsHandle LoadResourceLocation(AddressableAssetsConfigs configs)
        {
            Debug.Log("LoadResourceLocation");
            var handle = Addressables.LoadResourceLocationsAsync(configs.AddressableName);

            handle.Completed += (task) =>
            {
                Debug.Log("LoadResourceLocationsAsync ==> " + task.Status);
                if (task.Status == AsyncOperationStatus.Succeeded)
                {
                    OnResourceLocationsLoaded(configs, task);
                }
                else
                {
                    Debug.LogError("LoadResourceLocationsAsync is failed");
                }
            };
            return handle;
        }

        private AsyncOperationHandle LoadDependency(AddressableAssetsConfigs configs)
        {
            Debug.Log("LoadDependency");
            AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(configs.AddressableName);
            handle.Completed += (task) =>
            {
                Debug.Log("DownloadDependenciesAsync ==> " + task.Status);
                if (task.Status == AsyncOperationStatus.Succeeded)
                {
                    OnDependenciesDownloaded(configs, task);
                }
                else
                {
                    Debug.LogError("DownloadDependenciesAsync is Failed");
                }
            };
            return handle;
        }

        private void LoadAssetsPrefab(AddressableAssetsConfigs configs, ILoaderListener loadTask)
        {
            string AddressNameStr = configs.AddressableName;

            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(AddressNameStr);
            handle.Completed += (task) =>
            {
                Debug.Log("LoadAssetAsync " + AddressNameStr + " ==> " + task.Status);
                if (task.Status == AsyncOperationStatus.Succeeded)
                {
                    var result = task.Result;
                    _loadedPrefabs.Add(AddressNameStr, result);
                    OnAssetsPrefabLoaded(configs, result, loadTask);
                    Debug.Log("LoadAssetAsync is success, Result: " + result);
                }
                else
                {
                    Debug.LogError("LoadAssetAsync is Failed");
                    loadTask.OnLoadFail?.Invoke();
                }
            };

        }

        private void OnAssetsPrefabLoaded(AddressableAssetsConfigs configs,
            GameObject prefab, ILoaderListener loadTask)
        {
            string AddressNameStr = configs.AddressableName;

            var h = Addressables.LoadAssetAsync<IList<AnimationClip>>(AddressNameStr);
            h.Completed += (task) =>
            {
                Debug.Log("LoadAssetAsync AnimationClip " + AddressNameStr + " ==> " + task.Status);
                if (task.Status == AsyncOperationStatus.Succeeded)
                {
                    var result = task.Result;
                    Debug.Log("LoadAssetAsync AnimationClip is success, Result: " + result);
                    var ani = prefab.GetComponent<Animation>();
                    if (ani == null)
                        ani = prefab.AddComponent<Animation>();
                    ani.playAutomatically = false;
                    //foreach (var clip in result)
                    //{
                    //    Debug.Log("Add AniClip" + clip.name);
                    //    ani.AddClip(clip, clip.name);
                    //    ani.clip = clip;
                    //}

                    loadTask.OnPrefabDownloaded?.Invoke(prefab);
                }
                else
                {
                    Debug.LogWarning("LoadAssetAsync AnimationClip is Failed");
                    loadTask.OnPrefabDownloaded?.Invoke(prefab);
                }
            };

        }

        private void OnResourceLocationsLoaded(AddressableAssetsConfigs configs, ResLocatsHandle handle)
        {
            _loadedResLocats.Add(configs.AddressableName, handle);
        }

        private void OnDependenciesDownloaded(AddressableAssetsConfigs configs, AsyncOperationHandle handle)
        {
            _loadedDeps.Add(configs.AddressableName, handle);
        }

        public void UnloadAsset(string key)
        {
            string rev = null;
            foreach (var p in _loadedConfigs)
            {
                if (p.Value.AddressableName == key)
                {
                    UnloadAsset(p.Value);
                    rev = p.Key;
                    break;
                }
            }
            _loadedConfigs.Remove(rev);
        }

        private void UnloadAsset(AddressableAssetsConfigs configs)
        {
            var prefab = _loadedPrefabs[configs.AddressableName];
            Addressables.Release(prefab);
            _loadedPrefabs.Remove(configs.AddressableName);

            var resLocts = _loadedResLocats[configs.AddressableName];
            Addressables.Release(resLocts);
            _loadedResLocats.Remove(configs.AddressableName);

            var deps = _loadedDeps[configs.AddressableName];
            Addressables.Release(deps);
            _loadedDeps.Remove(configs.AddressableName);
        }
    }

    public interface ILoaderListener
    {
        Action<string> OnStartConfigDownload { get; set; }
        Action<GameObject> OnPrefabDownloaded { get; set; }
        Action<AddressableAssetsConfigs> OnConfigDownloaded { get; set; }
        Action OnLoadFail { get; set; }

        //Action<AsyncOperationHandle<IList<IResourceLocation>>> OnResourceLocationsLoaded { get; set; }
        //Action<AsyncOperationHandle> OnDependenciesDownloaded { get; set; }
    }

    [Serializable]
    public class AddressableAssetLoaderEvent : UnityEvent<AddressableAssetLoader>
    { }

}


