using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{

    bool isReward = true;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;
        Managers.UI.ShowSceneUI<UI_Lobby>();

        if(isReward == false)
            Managers.UI.ShowPopupUI<UI_DailyCheck>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public override void Clear()
    {
        Debug.Log("Lobby Scene Clear");
    }
}
