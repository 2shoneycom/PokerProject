using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_FriendList : UI_Base
{
    enum Buttons
    {
        UI_FriendList_DeleteButton,
    }

    enum Texts
    {
        UI_FriendList_FriendNameText,
        UI_FriendList_FriendStatusText,
    }

    enum Images
    {
        UI_FriendList_Icon,
    }

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));

        BindEvent(GetButton((int)Buttons.UI_FriendList_DeleteButton).gameObject, DeleteFriend);
    }

    public void SetStatusInfo(bool isOnline)
    {
        TextMeshProUGUI txt = GetText((int)Texts.UI_FriendList_FriendStatusText);
        if (isOnline)
        {
            txt.text = "온라인";
            txt.color = Color.green;
        }
        else
        {
            txt.text = "오프라인";
            txt.color = Color.red;
        }
    }

    public void SetFriendName(string name)
    {
        GetText((int)Texts.UI_FriendList_FriendNameText).text = name;
    }

    public void SetFriendIcon()
    {

    }

    void DeleteFriend(PointerEventData data)
    {
        Debug.Log("Friend Delete!");
    }
}
