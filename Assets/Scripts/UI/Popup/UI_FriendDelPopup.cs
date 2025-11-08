using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_FriendDelPopup : UI_Popup
{
    enum Texts
    {
        UI_DelPopupTitle,
    }

    enum Buttons
    {
        UI_YesButton,
        UI_NoButton,
    }

    UI_FriendList caller = null;

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        SetText();

        BindEvent(GetButton((int)Buttons.UI_YesButton).gameObject, YesClick);
        BindEvent(GetButton((int)Buttons.UI_NoButton).gameObject, NoClick);
    }

    void SetText()
    {
        string text = caller.GetFriendName();
        text += " 님을 친구 목록에서 삭제할까요?";
        GetText((int)Texts.UI_DelPopupTitle).text = text;
    }

    public void InitCaller(UI_FriendList caller) { this.caller = caller; }

    void YesClick(PointerEventData data)
    {
        caller?.DeleteFriend(true);
        ClosePopupUI();
    }

    void NoClick(PointerEventData data)
    {
        caller?.DeleteFriend(false);
        ClosePopupUI();
    }
}
