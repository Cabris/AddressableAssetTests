using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace WTC.Resource
{
    public class OperationManager : Singleton<OperationManager>
    {
        [SerializeField]
        string _url;
        ResourceManager _resourceManager;

        private void Start()
        {
            _resourceManager = new ResourceManager();
        }

        public static AsyncOperationHandle<AddressableAssetsConfigs> LoadCatalogAsync(string url)
        {
            LoadCatalogOperation op = new LoadCatalogOperation(url);
            AsyncOperationHandle<AddressableAssetsConfigs> handle = Instance._resourceManager.StartOperation(op, default);
            return handle;
        }
    }


}

