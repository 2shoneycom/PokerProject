using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Holdem : UI_Scene
{
    enum Buttons
    {
        UI_Buttons_Die,
        UI_Buttons_Call,
        UI_Buttons_Double,
        UI_Buttons_Quater,
        UI_Buttons_Half,
        UI_Buttons_AllIn,
        UI_RoomButton,
    }

    enum Texts
    {
        UI_Player1_NameText,
        UI_Player2_NameText,
        UI_Player3_NameText,
        UI_Player4_NameText,
        UI_Player5_NameText,
        UI_Player6_NameText,
        UI_Player7_NameText,
        UI_PotMoney_Text,
        UI_RoomButton_Text,
    }

    enum Images
    {
        UI_Player1_Icon,
        UI_Player2_Icon,
        UI_Player3_Icon,
        UI_Player4_Icon,
        UI_Player5_Icon,
        UI_Player6_Icon,
        UI_Player7_Icon,
    }

    enum GameObjects
    {
        UI_Backspace,
        UI_IconFriend,
    }

    bool isRoomOpened = false;

    public override void Init()
    {
//        base.Init();

        Managers.UI.SetWorldSpaceUI(gameObject);

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        SettingUIIconPos();

        SeatBind();
        ButtonOff();

        BindEvent(GetGameObject((int)GameObjects.UI_Backspace), Managers.Scene.MoveToLobbyScene);
        BindEvent(GetGameObject((int)GameObjects.UI_IconFriend), IconFriendClicked);

        SetRoomButton(isRoomOpened);
    }

    void SettingUIIconPos()
    {
        // 0,0 은 왼쪽 아래, 1,1 은 오른쪽 위
        GetGameObject((int)GameObjects.UI_Backspace).transform.position =
            Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.nearClipPlane));
        GetGameObject((int)GameObjects.UI_IconFriend).transform.position =
            Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));

        RectTransform toRect = GetGameObject((int)GameObjects.UI_IconFriend).GetComponent<RectTransform>();
        RectTransform targetRect = GetButton((int)Buttons.UI_RoomButton).GetComponent<RectTransform>();

        // 기준 오브젝트의 왼쪽 중앙 위치를 구함
        Vector3 leftCenterLocal = new Vector3(-toRect.rect.width, -toRect.rect.height * 0.5f, 0);
        Vector3 leftCenterWorld = toRect.TransformPoint(leftCenterLocal);

        // A 오브젝트를 해당 위치로 이동
        targetRect.position = leftCenterWorld;
    }

    void ButtonOff()
    {
        foreach (int idx in Enum.GetValues(typeof(Buttons)))
        {
            if (idx == (int)Buttons.UI_RoomButton) 
                continue;
            GetButton(idx).gameObject.SetActive(false);
        }
    }

    public void UpdatePlayerName(int index, string str)
    {
        GetText((int)Enum.Parse(typeof(Texts), $"UI_Player{index}_NameText")).text = str;
    }

    void SeatBind()
    {
        GetImage((int)Images.UI_Player1_Icon).gameObject.BindEvent(Button1);
        GetImage((int)Images.UI_Player2_Icon).gameObject.BindEvent(Button2);
        GetImage((int)Images.UI_Player3_Icon).gameObject.BindEvent(Button3);
        GetImage((int)Images.UI_Player4_Icon).gameObject.BindEvent(Button4);
        GetImage((int)Images.UI_Player5_Icon).gameObject.BindEvent(Button5);
        GetImage((int)Images.UI_Player6_Icon).gameObject.BindEvent(Button6);
        GetImage((int)Images.UI_Player7_Icon).gameObject.BindEvent(Button7);
    }

    public void Button1(PointerEventData data) { Managers.Seat.HaveSeat("1", 0); }
    public void Button2(PointerEventData data) { Managers.Seat.HaveSeat("2", 1); }
    public void Button3(PointerEventData data) { Managers.Seat.HaveSeat("3", 2); }
    public void Button4(PointerEventData data) { Managers.Seat.HaveSeat("4", 3); }
    public void Button5(PointerEventData data) { Managers.Seat.HaveSeat("5", 4); }
    public void Button6(PointerEventData data) { Managers.Seat.HaveSeat("6", 5); }
    public void Button7(PointerEventData data) { Managers.Seat.HaveSeat("7", 6); }

    void SetRoomButton(bool isRoomOpened)
    {
        if (!isRoomOpened)
        {
            ColorUtility.TryParseHtmlString("#FF0000", out Color targetColor);
            GetButton((int)Buttons.UI_RoomButton).GetComponent<Image>().color = targetColor;

            GetText((int)Texts.UI_RoomButton_Text).text = "방 공개";
            BindEvent(GetButton((int)Buttons.UI_RoomButton).gameObject, OpenRoomClicked);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#CFBFBF", out Color targetColor);
            GetButton((int)Buttons.UI_RoomButton).GetComponent<Image>().color = targetColor;

            GetText((int)Texts.UI_RoomButton_Text).text = "방 이동";
            BindEvent(GetButton((int)Buttons.UI_RoomButton).gameObject, MoveRoomClicked);
        }
    }

    void IconFriendClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_InviteFriendPopup>();
    }

    void OpenRoomClicked(PointerEventData data)
    {

    }

    void MoveRoomClicked(PointerEventData data)
    {

    }
}
