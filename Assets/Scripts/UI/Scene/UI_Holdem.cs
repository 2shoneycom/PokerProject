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

    public override void Init()
    {
//        base.Init();

        Managers.UI.SetWorldSpaceUI(gameObject);

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        // 0,0 은 왼쪽 아래, 1,1 은 오른쪽 위
        GetGameObject((int)GameObjects.UI_Backspace).transform.position =
            Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.nearClipPlane));
        GetGameObject((int)GameObjects.UI_IconFriend).transform.position =
            Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));

        SeatBind();
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

    void Update()
    {
        
    }
}
