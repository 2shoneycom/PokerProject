using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_FriendDelPopup : UI_Popup
{
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

        BindEvent(GetButton((int)Buttons.UI_YesButton).gameObject, YesClick);
        BindEvent(GetButton((int)Buttons.UI_NoButton).gameObject, NoClick);
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
