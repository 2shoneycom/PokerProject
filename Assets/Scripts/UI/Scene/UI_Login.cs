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
    public Button LobbyButton {  get { return _lobbyButton; } }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));

        TextMeshProUGUI tmp = GetText((int)Texts.UI_GoogleLoginButton_Text);
        tmp.text = "방 참가";

        tmp = GetText((int)Texts.UI_LoginText);
        tmp.text = "연결 중...";

        _lobbyButton = GetButton((int)Buttons.UI_GoogleLoginButton);
        BindEvent(_lobbyButton.gameObject, OnButtonClicked);

        GetButton((int)Buttons.UI_ReconnectButton).gameObject.SetActive(false);
        BindEvent(GetButton((int)Buttons.UI_ReconnectButton).gameObject, ReconnectButtonClicked);
//        LoginManager.Instance.LoginSceneLoaded(this);
    }

    public void SetConnectionInfoText(string info)
    {
        GetText((int)Texts.UI_LoginText).text = info;
    }

    public void ButtonInteractive(bool on)
    {
        _lobbyButton.interactable = on;
    }

    public void OnButtonClicked(PointerEventData data)
    {
        if (!_lobbyButton.interactable)
            return;

        DisableAllButton();
        Managers.Photon.ConnectToPhoton(this);
//        LoginManager.Instance.LogIn();
    }

    void DisableAllButton()
    {
        for (int i = 0; i < Enum.GetValues(typeof(Buttons)).Length; i++)
        {
            GetButton(i).gameObject.SetActive(false);
        }
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
