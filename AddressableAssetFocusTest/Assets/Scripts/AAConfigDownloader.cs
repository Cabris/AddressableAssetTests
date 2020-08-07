using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace WTC.Resource
{
    public class AAConfigDownloader : MonoBehaviour
    {
        public bool _downloadAtStart = false;

        [SerializeField]
        string _configUrl = "http://10.222.130.205:8080/Configs/AddressableAssetsBundles.json";

        [SerializeField]
        ConfigLoadedEvent _onConfigLoaded = new ConfigLoadedEvent();

        public string ConfigUrl { get => _configUrl; set => _configUrl = value; }

        // Start is called before the first frame update
        void Start()
        {
            if (_downloadAtStart)
                StartCoroutine(GetRequest(ConfigUrl, _onConfigLoaded));
        }

        public static IEnumerator GetRequest(string uri, ConfigLoadedEvent onComplete)
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
                    data = AddressablesConsts.ParseDynamicPath(data);
                    var jsonObj = JsonUtility.FromJson<AddressableAssetsConfigs>(data);
                    onComplete?.Invoke(jsonObj);
                }
            }


        }

        [Serializable]
        public class ConfigLoadedEvent : UnityEvent<AddressableAssetsConfigs>
        { }

    }
}

