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

    // 구글 로그인 요청 (기존)
    public void RequestLogin()
    {
        if (Managers.Login != null)
            Managers.Login.LogIn();
        else
            Debug.LogError("LoginManager가 초기화되지 않았습니다.");
    }

    // 카카오 로그인 요청 (신규)
    public void RequestKakaoLogin()
    {
        if (Managers.Login != null)
            Managers.Login.LogInWithKakao();
        else
            Debug.LogError("LoginManager가 초기화되지 않았습니다.");
    }
}
