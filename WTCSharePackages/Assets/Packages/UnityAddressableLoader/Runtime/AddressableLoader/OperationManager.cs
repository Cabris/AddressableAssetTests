using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace WTC.Resource
{
    public class OperationManager : Utility.Singleton<OperationManager>
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

        private static IEnumerator GetRequest(string uri, Action<UnityWebRequestAsyncOperation> onComplete)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                UnityWebRequestAsyncOperation handle = webRequest.SendWebRequest();
                yield return handle;
                onComplete?.Invoke(handle);
            }
        }

        public static void GetJsonRequest<T>(string uri, Action<T> onJsonLoaded) where T : class
        {
            void OnWebRequest(UnityWebRequestAsyncOperation handle)
            {
                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                var webRequest = handle.webRequest;
                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Debug.Log(pages[page] + ": Error: " + webRequest.error);
                    onJsonLoaded?.Invoke(null);
                }
                else
                {
                    string data = webRequest.downloadHandler.text;
                    data = AddressablesConsts.ParseDynamicPath2(data);
                    Debug.Log(pages[page] + ":\nReceived: " + data);
                    var jsonObj = JsonUtility.FromJson<T>(data);
                    onJsonLoaded?.Invoke(jsonObj);
                }
            }

            Instance.StartCoroutine(GetRequest(uri, OnWebRequest));
        }
    }
}




