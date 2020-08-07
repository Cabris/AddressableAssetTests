
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace WTC.Resource
{

    class LoadCatalogOperation : AsyncOperationBase<AddressableAssetsConfigs>
    {
        private string _url;
        private AsyncOperationStatus _status = AsyncOperationStatus.None;
        private AddressableAssetsConfigs _config;
        float _progress;
        protected override float Progress => _progress;

        public LoadCatalogOperation(string url) : base()
        {
            _url = url;
        }

        protected override void Execute()
        {
            Operation();
        }

        private async void Operation()
        {
            _progress = 0;
            _config = await DownloadConfig(_url);
            if (_config == null)
            {
                _progress = 1;
                Complete(_config, false, "download config fail at url: " + _url);
                return;
            }
            _progress = 0.5f;

            if (_config.Type == AddressableAssetsConfigs.AssetType.Prefab)
            {
                var status = await LoadCatalog(_config);
                if (status != AsyncOperationStatus.Succeeded)
                {
                    _progress = 1;
                    Complete(_config, false, "download catalog fail at url: " + _url);
                    return;
                }
            }
            else
                Debug.Log("not a prefab, skip load catalog");
            _progress = 1;
            Complete(_config, true, "download catalog success");
        }

        private async Task<AddressableAssetsConfigs> DownloadConfig(string url)
        {
            AddressableAssetsConfigs config = null;
            await OperationManager.GetJsonRequest<AddressableAssetsConfigs>(url, (c) =>
            {
                config = c;
            });
            return config;
        }

        private async Task<AsyncOperationStatus> LoadCatalog(AddressableAssetsConfigs configs)
        {
            Debug.Log("LoadCatalog: " + configs.AddressableName);
            string assetName = configs.AddressableName;
            string catalogPath = configs.CatalogPath;

            bool isComplete = false;
            AsyncOperationStatus status = AsyncOperationStatus.None;
            AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync(catalogPath);
            handle.Completed += (task) =>
            {
                isComplete = true;
                status = task.Status;
                Debug.Log("LoadContentCatalogAsync Complete ==> " + status);
                if (status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("LoadContentCatalogAsync is Succeeded");
                }
                else
                {
                    Debug.LogError("LoadContentCatalogAsync is failed");
                }
            };

            await new WaitUntil(() => isComplete);
            return status;
        }
    }
}
