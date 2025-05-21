using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldemGameControl : MonoBehaviour
{
    private static HoldemGameControl instance;
    public static HoldemGameControl Instacne
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }
    }

    HoldemScene _scene;
    UI_Holdem _holdemUI;

    void Start()
    {
        _scene = (HoldemScene)Managers.Scene.CurrentScene;
        _holdemUI = (UI_Holdem)Managers.UI.SceneUI;
    }

    public void StartGame()
    {

    }
}
