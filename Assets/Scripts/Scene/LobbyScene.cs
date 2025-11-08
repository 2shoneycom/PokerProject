using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;
        Managers.UI.ShowSceneUI<UI_Lobby>();
        Managers.Audio.PlayBGM(Define.BGM.Lobby);

        if (User.NowUser.GetisDailyClaimed() == false)
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
