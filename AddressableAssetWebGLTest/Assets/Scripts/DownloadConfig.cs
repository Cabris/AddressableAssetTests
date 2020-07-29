using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class DownloadConfig : MonoBehaviour
{
    [SerializeField]
    string _configUrl = "http://10.222.130.205:8080/Configs/AddressableAssetsBundles.json";

    public ConfigLoadedEvent _onConfigLoaded = new ConfigLoadedEvent();

    //public UnityEvent myEvent = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetRequest(_configUrl));
    }

    IEnumerator GetRequest(string uri)
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
            }
            else
            {
                string data = webRequest.downloadHandler.text;
                Debug.Log(pages[page] + ":\nReceived: " + data);

                var jsonObj = JsonUtility.FromJson<AddressableAssetsConfigs>(data);
                _onConfigLoaded?.Invoke(jsonObj);
                //#if UNITY_WEBGL
                //                _onConfigLoaded?.Invoke(jsonObj.WebGL);
                //#endif
                //#if UNITY_ANDROID
                //                _onConfigLoaded?.Invoke(jsonObj.Android);
                //#endif
            }
        }
    }

    [Serializable]
    public class AddressableAssetsConfigs
    {
        public string AddressableName = "";
        public string WebGL = "";
        public string Android = "";
    }

    [Serializable]
    public class ConfigLoadedEvent : UnityEvent<AddressableAssetsConfigs>
    { }

}
