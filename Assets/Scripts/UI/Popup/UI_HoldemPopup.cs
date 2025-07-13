using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_HoldemPopup : UI_Popup
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

    private Define.GameType _gameType = Define.GameType.Holdem;

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));

        BindEvent(GetButton((int)Buttons.UI_Beginner).gameObject, BeginnerButton);
        BindEvent(GetButton((int)Buttons.UI_Amateur).gameObject, AmateurButton);
        BindEvent(GetButton((int)Buttons.UI_Pro).gameObject, ProButton);

        EnterPanel.SetActive(false);
    }

    void BeginnerButton(PointerEventData data)
    {
        SetEnterPanel(Buttons.UI_Beginner);
        Managers.CurrentDifficulty = Define.Difficulty.Beginner;

        BindEvent(GetButton((int)Buttons.UI_EnterRoomButton).gameObject, EnterBeginnerRoom);
        BindEvent(GetButton((int)Buttons.UI_CreateRoomButton).gameObject, CreateBeginnerRoom);
    }

    void EnterBeginnerRoom(PointerEventData data)
    {
        Managers.Photon.JoinGame(500, _gameType);
    }

    void CreateBeginnerRoom(PointerEventData data)
    {
        Managers.Photon.CreateGame(500, _gameType);
    }

    void AmateurButton(PointerEventData data)
    {
        SetEnterPanel(Buttons.UI_Amateur);
        Managers.CurrentDifficulty = Define.Difficulty.Amateur;

        BindEvent(GetButton((int)Buttons.UI_EnterRoomButton).gameObject, EnterAmateurRoom);
        BindEvent(GetButton((int)Buttons.UI_CreateRoomButton).gameObject, CreateAmateurRoom);
    }

    void EnterAmateurRoom(PointerEventData data)
    {
        Managers.Photon.JoinGame(5000, _gameType);
    }

    void CreateAmateurRoom(PointerEventData data)
    {
        Managers.Photon.CreateGame(5000, _gameType);
    }

    void ProButton(PointerEventData data)
    {
        SetEnterPanel(Buttons.UI_Pro);
        Managers.CurrentDifficulty = Define.Difficulty.Pro;

        BindEvent(GetButton((int)Buttons.UI_EnterRoomButton).gameObject, EnterProRoom);
        BindEvent(GetButton((int)Buttons.UI_CreateRoomButton).gameObject, CreateProRoom);
    }

    void EnterProRoom(PointerEventData data)
    {
        Managers.Photon.JoinGame(50000, _gameType);
    }

    void CreateProRoom(PointerEventData data)
    {
        Managers.Photon.CreateGame(50000, _gameType);
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
