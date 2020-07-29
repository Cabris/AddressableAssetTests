using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //[SerializeField]
    //Button LoadObjBtn;

    [SerializeField]
    string AddressNameStr;

    [SerializeField]
    string AddressableScene;

    GameObject assetObj;
    bool isLoadSucc;

    void Start()
    {
        //LoadObjBtn.interactable = false;
        isLoadSucc = false;

        //LoadObjBtn.onClick.AddListener(CreateObjBtn);
        StartCoroutine(DoTask());
    }

    IEnumerator DoTask()
    {
        //  AsyncOperationHandle<GameObject> task = Addressables.LoadAssetAsync<GameObject>(AddressNameStr);
        //task.Completed += OnAssetObjLoaded;

        var task = Addressables.LoadSceneAsync(AddressableScene, LoadSceneMode.Additive, true);

        yield return new WaitUntil(() =>
    {
        Debug.Log("DoTask: " + task.PercentComplete * 100);

        return task.IsDone;
    });
        Debug.Log("TaskDown");
    }

    void OnAssetObjLoaded(AsyncOperationHandle<GameObject> asyncOperationHandle)
    {
        //LoadObjBtn.interactable = true;
        Debug.Log("asyncOperationHandle.Result: " + asyncOperationHandle.Result);
        isLoadSucc = true;
        assetObj = asyncOperationHandle.Result;
        CreateObjBtn();
    }

    void CreateObjBtn()
    {
        Vector3 pos = new Vector3(Random.Range(5, -5), Random.Range(4, -4), 0);
        Instantiate(assetObj, pos, Quaternion.identity);
    }
}
