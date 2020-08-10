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

        private void Operation()
        {
            _progress = 0;

            void OnConfigDownloaded(AddressableAssetsConfigs config)
            {
                _config = config;
                if (_config == null)
                {
                    _progress = 1;
                    Complete(_config, false, "download config fail at url: " + _url);
                    return;
                }
                _progress = 0.5f;

                if (_config.Type == AddressableAssetsConfigs.AssetType.Prefab)
                {
                    var handle = LoadCatalog(_config);
                }
                else
                {
                    Debug.Log("not a prefab, skip load catalog");
                    Complete(_config, true, "download configs success");
                }
            }

            OperationManager.GetJsonRequest<AddressableAssetsConfigs>(_url, OnConfigDownloaded);

        }

        private AsyncOperationHandle<IResourceLocator> LoadCatalog(AddressableAssetsConfigs configs)
        {
            Debug.Log("LoadCatalog: " + configs.AddressableName);
            string assetName = configs.AddressableName;
            string catalogPath = configs.CatalogPath;

            AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync(catalogPath);
            handle.Completed += (task) =>
            {
                var status = task.Status;
                Debug.Log("LoadContentCatalogAsync Complete ==> " + status);
                if (status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("LoadContentCatalogAsync is Succeeded");
                    _progress = 1;
                    Complete(_config, true, "download catalog success");
                }
                else
                {
                    Debug.LogError("LoadContentCatalogAsync is failed");
                }
            };

            return handle;
        }
    }
}
