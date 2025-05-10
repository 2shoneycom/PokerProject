using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_AcceptFriendList : UI_FriendBase
{
    enum Buttons
    {
        UI_AcceptFriendList_RejectButton,
        UI_AcceptFriendList_AcceptButton,
    }

    enum Texts
    {
        UI_AcceptFriendList_FriendNameText,
    }

    enum Images
    {
        UI_AcceptFriendList_Icon,
    }

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Setting();

        BindEvent(GetButton((int)Buttons.UI_AcceptFriendList_RejectButton).gameObject, RejecteFriend);
        BindEvent(GetButton((int)Buttons.UI_AcceptFriendList_AcceptButton).gameObject, AcceptFriend);
    }

    public override void Setting()
    {
        //GetText((int)Texts.UI_AcceptFriendList_FriendNameText).text = Name;
        //GetImage((int)Images.UI_AcceptFriendList_Icon).sprite = Icon.sprite;
    }

    void AcceptFriend(PointerEventData data)
    {
        // 荐遏 贸府...

        Debug.Log("Friend Accept!");
        Managers.Resource.Destroy(gameObject);
    }

    void RejecteFriend(PointerEventData data)
    {
        // 芭例 贸府...

        Debug.Log("Friend Reject!");
        Managers.Resource.Destroy(gameObject);
    }
}
