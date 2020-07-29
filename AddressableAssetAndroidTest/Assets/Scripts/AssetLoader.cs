using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;

public class AssetLoader : MonoBehaviour
{
    public string catalogPath;

    [SerializeField]
    string AddressNameStr = "Mei_Run";

    [SerializeField]
    string AddressableScene = "test";


    public List<IResourceLocation> locations;

    void Start()
    {
        initAddressables();
    }


    void initAddressables()
    {
        Debug.Log("initAddressables");
        AsyncOperationHandle<IResourceLocator> handle = Addressables.InitializeAsync();
        handle.Completed += initDone;
    }
    private void initDone(AsyncOperationHandle<IResourceLocator> obj)
    {
        Debug.Log("Initialization Complete ==> " + obj.Status);
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            loadCatalog();
        }
    }

    void loadCatalog()
    {
        Debug.Log("loadCatalog");
        AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync(catalogPath);
        handle.Completed += loadCatalogsCompleted;
    }

    void loadCatalogsCompleted(AsyncOperationHandle<IResourceLocator> obj)
    {
        Debug.Log("loadCatalogsCompleted ==> " + obj.Status);
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            loadResourceLocation();
        }
        else
        {
            Debug.LogError("LoadCatalogsCompleted is failed");
        }
    }

    void loadResourceLocation()
    {
        Debug.Log("loadResourceLocation");

        AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync("test");
        handle.Completed += locationsLoaded;
    }
    void locationsLoaded(AsyncOperationHandle<IList<IResourceLocation>> obj)
    {
        Debug.Log("locationsLoaded ==> " + obj.Status);
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            locations = new List<IResourceLocation>(obj.Result);
            loadDependency();
        }
        else
        {
            Debug.LogError("locationsLoaded is failed");
        }
    }

    void loadDependency()
    {
        Debug.Log("loadDependency");
        AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync("test");
        handle.Completed += dependencyLoaded;
    }

    void dependencyLoaded(AsyncOperationHandle obj)
    {
        Debug.Log("dependencyLoaded ==> " + obj.Status);
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            loadAssets();
        }
        else
        {
            Debug.LogError("dependencyLoaded is Failed");
        }
    }

    private void loadAssets()
    {
        //AsyncOperationHandle<IList<GameObject>> handle = Addressables.LoadAssetsAsync<GameObject>(locations, onAssetsCategoryLoaded);
        //handle.Completed += onAssetsLoaded;
        Addressables.LoadSceneAsync(AddressableScene, LoadSceneMode.Additive, true);

        AsyncOperationHandle<GameObject> task = Addressables.LoadAssetAsync<GameObject>(AddressNameStr);
        task.Completed += (temp) =>
        {
            var result = temp.Result;
            if (result != null)
            {
                Vector3 pos = new Vector3(Random.Range(5, -5), Random.Range(4, -4), 0);
                Instantiate(result, pos, Quaternion.identity);
            }
           else
                Debug.LogError("result of "+AddressNameStr+" is null");

        };


    }


    /*
    private void onAssetsCategoryLoaded(GameObject obj)
    {
        SpawnItem(obj.name);
    }
    private void onAssetsLoaded(AsyncOperationHandle<IList<GameObject>> obj)
    {
    }

    void SpawnItem(string addressableKey)
    {
        Debug.Log("SpawnItem ==> " + addressableKey);
        AsyncOperationHandle<GameObject> asyncLoad = Addressables.InstantiateAsync(addressableKey, Vector3.zero, Quaternion.identity);
        StartCoroutine(progressAsync(asyncLoad));
        asyncLoad.Completed += assetSpawned;
    }

    void SpawnItem(GameObject addressableObj)
    {
        Debug.Log("SpawnItem ==> " + addressableObj);
        AsyncOperationHandle<GameObject> asyncLoad = Addressables.InstantiateAsync(addressableObj);
        StartCoroutine(progressAsync(asyncLoad));
        asyncLoad.Completed += assetSpawned;
    }
    private System.Collections.IEnumerator progressAsync(AsyncOperationHandle<GameObject> asyncOperation)
    {
        float percentLoaded = asyncOperation.PercentComplete;
        while (!asyncOperation.IsDone)
        {
            Debug.Log("Progress = " + percentLoaded + "%");
            yield return 0;
        }
        Debug.Log("Progress Done= " + percentLoaded + "%");
    }
    void assetSpawned(AsyncOperationHandle<GameObject> obj)
    {
        Debug.Log("Instantiate completed ==> " + obj.Status);
    }*/
}