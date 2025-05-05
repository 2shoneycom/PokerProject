using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class LoginScene : BaseScene
{

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;
        Managers.UI.ShowSceneUI<UI_Login>();
    }

    public override void Clear()
    {
        Debug.Log("Login Scene Clear");
    }
}
