using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_FriendList : UI_FriendBase
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
        Setting();

        BindEvent(GetButton((int)Buttons.UI_FriendList_DeleteButton).gameObject, DeleteFriend);
    }

    public override void Setting()
    {
        //GetText((int)Texts.UI_FriendList_FriendNameText).text = Name;
        //GetText((int)Texts.UI_FriendList_FriendStatusText).text = Status;
        //GetImage((int)Images.UI_FriendList_Icon).sprite = Icon.sprite;
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
        Managers.UI.ShowPopupUI<UI_FriendDelPopup>().InitCaller(this);
    }

    public void DeleteFriend(bool isDel)
    {
        if (isDel == false) return;

        // 친구 삭제 처리...
        Debug.Log("Friend Deleted!");
        Managers.Resource.Destroy(gameObject);
    }
}
