using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Login : UI_Scene        // Lobby씬의 SceneUI
{
    enum Buttons
    {
        UI_GoogleLoginButton,
        UI_KakaoLoginButton,
        UI_ReconnectButton,
    }

    enum Texts
    {
        UI_TitleText,
        UI_LoginText,
        UI_GoogleLoginButton_Text,
        UI_KakaoLoginButton_Text,
    }

    Button _lobbyButton = null;
    public Button LobbyButton { get { return _lobbyButton; } }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));

        GetText((int)Texts.UI_GoogleLoginButton_Text).text = "구글 로그인";
        GetText((int)Texts.UI_KakaoLoginButton_Text).text = "카카오 로그인";

        SetConnectionInfoText("로그인 해주세요!");

        // 구글 버튼
        _lobbyButton = GetButton((int)Buttons.UI_GoogleLoginButton);
        BindEvent(_lobbyButton.gameObject, OnGoogleButtonClicked);

        // 카카오 버튼
        BindEvent(GetButton((int)Buttons.UI_KakaoLoginButton).gameObject, OnKakaoButtonClicked);

        // 재접속 버튼
        GetButton((int)Buttons.UI_ReconnectButton).gameObject.SetActive(false);
        BindEvent(GetButton((int)Buttons.UI_ReconnectButton).gameObject, ReconnectButtonClicked);

        Managers.Login.LoginSceneLoaded(this);
        Managers.Auth.LoginSceneLoaded(this);
    }

    public void SetConnectionInfoText(string info)
    {
        GetText((int)Texts.UI_LoginText).text = info;
    }

    public void ButtonInteractive(bool on)
    {
        _lobbyButton.interactable = on;
        GetButton((int)Buttons.UI_KakaoLoginButton).interactable = on;
    }

    private void OnGoogleButtonClicked(PointerEventData data)
    {
        if (!_lobbyButton.interactable)
            return;

        DisableAllButton();
        LoginScene.Instance.RequestLogin();       // 기존 구글 플로우
    }

    private void OnKakaoButtonClicked(PointerEventData data)
    {
        DisableAllButton();
        LoginScene.Instance.RequestKakaoLogin();  // 새 카카오 플로우
    }

    public void ShowLoginButtons()
    {
        GetButton((int)Buttons.UI_GoogleLoginButton).gameObject.SetActive(true);
        GetButton((int)Buttons.UI_KakaoLoginButton).gameObject.SetActive(true);
    }

    void DisableAllButton()
    {
        for (int i = 0; i < Enum.GetValues(typeof(Buttons)).Length; i++)
            GetButton(i).gameObject.SetActive(false);
    }

    public void ShowReconnectButton()
    {
        GetButton((int)Buttons.UI_ReconnectButton).gameObject.SetActive(true);
    }

    void ReconnectButtonClicked(PointerEventData data)
    {
        DisableAllButton();
        Managers.Photon.Reconnect();
    }
}
