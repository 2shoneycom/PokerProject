using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.PlayerInfo;
        Managers.UI.ShowSceneUI<UI_PlayerInfo>();
        Managers.Audio.PlayBGM(Define.BGM.PlayerInfo);
    }

    public override void Clear()
    {

    }
}
