using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HoldemGameControl : MonoBehaviour
{
    private static HoldemGameControl instance;
    public static HoldemGameControl Instance
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

    HoldemPlayerManager _holdemaPlayers;
    public static HoldemPlayerManager Players { get { return Instance._holdemaPlayers; } }

    HoldemBetManager _betManager;
    HoldemCardManager _cardManager;

    HoldemScene _scene;
    UI_Holdem _holdemUI;

    bool isPlaying = false;
    public bool IsPlaying { get { return isPlaying; } }

    void Start()
    {
        _scene = (HoldemScene)Managers.Scene.CurrentScene;
        _holdemUI = (UI_Holdem)Managers.UI.SceneUI;

        _cardManager = this.GetOrAddComponent<HoldemCardManager>();
        _betManager = new HoldemBetManager(_holdemUI);
        _holdemaPlayers = new HoldemPlayerManager();
    }

    public void StartGame()
    {
        _betManager.CalBetAndButtonSwitch();
    }
}
