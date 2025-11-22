using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_PokerPopup : UI_Popup
{
    enum Buttons
    {
        UI_Beginner,
        UI_Amateur,
        UI_Pro,
        UI_EnterRoomButton,
        UI_CreateRoomButton,
    }

    enum GameObjects
    {
        UI_RoomEnterPanel,
    }

    GameObject _enterPanel = null;
    GameObject EnterPanel
    {
        get
        {
            if (_enterPanel == null)
                _enterPanel = GetGameObject((int)GameObjects.UI_RoomEnterPanel);
            return _enterPanel;
        }
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));

        BindEvent(GetButton((int)Buttons.UI_Beginner).gameObject, BeginnerButton);
        BindEvent(GetButton((int)Buttons.UI_Amateur).gameObject, AmateurButton);
        BindEvent(GetButton((int)Buttons.UI_Pro).gameObject, ProButton);
        BindEvent(GetButton((int)Buttons.UI_EnterRoomButton).gameObject, EnterRoomClicked);
        BindEvent(GetButton((int)Buttons.UI_CreateRoomButton).gameObject, CreateRoomClicked);

        EnterPanel.SetActive(false);
    }

    void BeginnerButton(PointerEventData data)
    {
        SetEnterPanel(Buttons.UI_Beginner);
        Managers.CurrentDifficulty = Define.Difficulty.Beginner;
    }

    void AmateurButton(PointerEventData data)
    {
        SetEnterPanel(Buttons.UI_Amateur);
        Managers.CurrentDifficulty = Define.Difficulty.Amateur;
    }

    void ProButton(PointerEventData data)
    {
        SetEnterPanel(Buttons.UI_Pro);
        Managers.CurrentDifficulty = Define.Difficulty.Pro;
    }

    void EnterRoomClicked(PointerEventData data)
    {
        if (User.NowUser.IsEnoughMoney() == false)
        {
            Managers.UI.ShowPopupUI<UI_NotEnoughMoneyPopup>();
            return;
        }

        Managers.Photon.JoinGame(Managers.CurrentDifficulty, Managers.CurrentGameType);
    }

    void CreateRoomClicked(PointerEventData data)
    {
        if (User.NowUser.IsEnoughMoney() == false)
        {
            Managers.UI.ShowPopupUI<UI_NotEnoughMoneyPopup>();
            return;
        }

        Managers.Photon.CreateGame(Managers.CurrentDifficulty, Managers.CurrentGameType);
    }

    void SetEnterPanel(Buttons button)
    {
        EnterPanel.SetActive(true);
        RectTransform target = EnterPanel.GetComponent<RectTransform>();
        RectTransform src = GetButton((int)button).GetComponent<RectTransform>();
        CopyRectTransform(src, target);
    }

    void CopyRectTransform(RectTransform from, RectTransform to)
    {
        to.anchoredPosition = from.anchoredPosition;
        to.sizeDelta = from.sizeDelta;
        to.anchorMin = from.anchorMin;
        to.anchorMax = from.anchorMax;
        to.pivot = from.pivot;
        to.localScale = from.localScale;
        to.localRotation = from.localRotation;
    }
}
