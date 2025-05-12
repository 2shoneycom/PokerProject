using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SearchFriendList : UI_FriendBase
{
    enum Buttons
    {
        UI_AddFriendButton,
    }

    enum Texts
    {
        UI_SearchFriendList_FriendNameText,
    }

    enum Images
    {
        UI_SearchFriendList_Icon,
    }

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Setting();

        BindEvent(GetButton((int)Buttons.UI_AddFriendButton).gameObject, AddFriend);
    }

    void AddFriend(PointerEventData data)
    {
        // 친구 추가 로직...
        Debug.Log("Friend Add!");
        Managers.Resource.Destroy(gameObject);
    }

    public override void Setting()
    {
        //GetText((int)Texts.UI_SearchFriendList_FriendNameText).text = Name;
        //GetImage((int)Images.UI_SearchFriendList_Icon).sprite = Icon.sprite;
    }
}
