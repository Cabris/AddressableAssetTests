using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WTC.Resource;
using static WTC.Resource.AddressableAssetLoader;

public class AsyncTester : MonoBehaviour
{
    [SerializeField]
    InputField _input;

    [SerializeField]
    Transform _root;

    [SerializeField]
    Text _text;



    public void OnClick()
    {
        LoadRemoteAssetEvent onLoaded = new LoadRemoteAssetEvent();
        AddressableAssetLoader.Instance.LoadRemoteAsset(_input.text, onLoaded);
        onLoaded.AddListener(OnPrefabLoaded);
    }

    void OnPrefabLoaded(GameObject prefab)
    {
        Debug.Log("OnPrefabLoaded: " + prefab);
        if (prefab != null)
        {
            Instantiate(prefab, _root, false);
        }
        else
            Debug.LogError("prefab is null");
    }


    private async void Work()
    {
        await RunAsyncFromCoroutineTest();
        _text.text = "RunAsyncFromCoroutineTest done";
        //_text.text = "OnClick...";
        //await new WaitForSeconds(1.0f);
        //_text.text = "OnClick...1";
        //await new WaitForSeconds(1.0f);
        //_text.text = "OnClick...2";
        //await new WaitForSeconds(1.0f);
        //_text.text = "OnClick...3";
    }

    IEnumerator RunAsyncFromCoroutineTest()
    {
        _text.text = "OnClick...";
        yield return new WaitForSeconds(1.0f);
        _text.text = "OnClick...1";
        yield return new WaitForSeconds(1.0f);
        _text.text = "OnClick...2";
        yield return new WaitForSeconds(1.0f);
        _text.text = "OnClick...3";
    }

}
