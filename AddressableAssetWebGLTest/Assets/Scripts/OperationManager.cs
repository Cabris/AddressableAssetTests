using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
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

        public static IEnumerator GetJsonRequest<T>(string uri, Action<T> onComplete) where T:class
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                if (webRequest.isNetworkError)
                {
                    Debug.Log(pages[page] + ": Error: " + webRequest.error);
                    onComplete?.Invoke(null);
                }
                else
                {
                    string data = webRequest.downloadHandler.text;
                    Debug.Log(pages[page] + ":\nReceived: " + data);
                    data = AddressablesConsts.ParseDynamicPath2(data);
                    var jsonObj = UnityEngine.JsonUtility.FromJson<T>(data);
                    onComplete?.Invoke(jsonObj);
                }
            }
        }
    }


}

