using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UI_GameInvitePopup : UI_Popup
{
    private string gameRoomID;
    private bool isInited = false; 

    enum Buttons
    {
        UI_YesButton,
        UI_NoButton,
    }

    enum Texts
    {
        UI_NickName,
        UI_GameType,
    }

    public override void Init()
    {
        if (isInited)
        {
            return;
        }

        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));

        BindEvent(GetButton((int)Buttons.UI_YesButton).gameObject, YesClick);
        BindEvent(GetButton((int)Buttons.UI_NoButton).gameObject, NoClick);

        isInited = true;
    }

    public void SetRoomID(string value)
    {
        gameRoomID = value;
    }

    public void SetSenderNickName(string name)
    {
        GetText((int)Texts.UI_NickName).text = name;
    }

    public void SetGameType(string type)
    {
        GetText((int)Texts.UI_GameType).text = type;
    }

    void YesClick(PointerEventData data)
    {
        ClosePopupUI();
        Managers.DB.RemoveInvitation(); // 받았던 초대 삭제하고 
        Managers.Photon.JoinRoomByName(gameRoomID);    // 해당 방으로 이동
    }

    void NoClick(PointerEventData data)
    {
        ClosePopupUI();
        Managers.DB.RemoveInvitation(); // 받았던 초대 삭제
    }
}
