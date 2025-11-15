using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;

public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }

    public T[] LoadAll<T>(string path) where T : Object
    {
        return Resources.LoadAll<T>(path);
    }

    public GameObject PhotonInstantiate(string path, Transform pos = null)
    {
        path = $"Prefabs/{path}";
        // Debug.Log("pi");
        GameObject go = PhotonNetwork.Instantiate(path, pos.position, Quaternion.identity);
        // Debug.Log("pi null");

        int index = go.name.IndexOf("(Clone)");         // 프리펩 생성시 붙는 Clone~을 없앰
        if (index > 0)
            go.name = go.name.Substring(0, index);

        return go;
    }

    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject prefab = Load<GameObject>($"Prefabs/{path}");
        if (prefab == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        GameObject go = Object.Instantiate(prefab, parent);
        int index = go.name.IndexOf("(Clone)");         // 프리펩 생성시 붙는 Clone~을 없앰
        if (index > 0)
            go.name = go.name.Substring(0, index);

        return go;
    }

    public void PhotonDestroy(GameObject go)
    {
        if (go == null)
            return;

        PhotonNetwork.Destroy(go);
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        Object.Destroy(go);
    }
}
