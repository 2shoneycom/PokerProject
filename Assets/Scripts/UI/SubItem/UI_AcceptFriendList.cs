using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_AcceptFriendList : UI_FriendBase
{
    private string boundUID;

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

        BindEvent(GetButton((int)Buttons.UI_AcceptFriendList_RejectButton).gameObject, RejectFriend);
        BindEvent(GetButton((int)Buttons.UI_AcceptFriendList_AcceptButton).gameObject, AcceptFriend);
    }

    public override void Setting()
    {
        //GetText((int)Texts.UI_AcceptFriendList_FriendNameText).text = Name;
        //GetImage((int)Images.UI_AcceptFriendList_Icon).sprite = Icon.sprite;
    }

    public void SetUID(string uid)
    {
        boundUID = uid;
    }

    public void SetNickname(string nickname)
    {
        GetText((int)Texts.UI_AcceptFriendList_FriendNameText).text = nickname;
    }

    void AcceptFriend(PointerEventData data)
    {
        Debug.Log("Friend Accept!");
        Managers.DB.AcceptAndAddFriend(boundUID);   // 친구 수락 처리
        Managers.Resource.Destroy(gameObject);      // 해당 요소 제거
    }

    void RejectFriend(PointerEventData data)
    {
        Debug.Log("Friend Reject!");
        Managers.DB.RejectRequest(boundUID);
        Managers.Resource.Destroy(gameObject);
    }
}
