using System;
using UnityEngine;
using UnityEngine.Events;

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
                OperationManager.GetJsonRequest<AddressableAssetsConfigs>(ConfigUrl, (c) =>
                {
                    _onConfigLoaded?.Invoke(c);
                });
        }


        [Serializable]
        public class ConfigLoadedEvent : UnityEvent<AddressableAssetsConfigs>
        { }

    }
}

