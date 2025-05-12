using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_JackPopup : UI_Popup
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

        EnterPanel.SetActive(false);
    }

    void BeginnerButton(PointerEventData data)
    {
        SetEnterPanel(Buttons.UI_Beginner);

        BindEvent(GetButton((int)Buttons.UI_EnterRoomButton).gameObject, EnterBeginnerRoom);
        BindEvent(GetButton((int)Buttons.UI_CreateRoomButton).gameObject, CreateBeginnerRoom);
    }

    void EnterBeginnerRoom(PointerEventData data)
    {

    }

    void CreateBeginnerRoom(PointerEventData data)
    {

    }

    void AmateurButton(PointerEventData data)
    {
        SetEnterPanel(Buttons.UI_Amateur);
    }

    void ProButton(PointerEventData data)
    {
        SetEnterPanel(Buttons.UI_Pro);
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
