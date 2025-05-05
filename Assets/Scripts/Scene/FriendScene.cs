using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Friend;
        Managers.UI.ShowSceneUI<UI_Friend>();
    }

    public override void Clear()
    {

    }
}
