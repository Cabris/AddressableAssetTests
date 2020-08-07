using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace WTC.Resource
{
    public class OperationManager : Singleton<OperationManager>
    {
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

