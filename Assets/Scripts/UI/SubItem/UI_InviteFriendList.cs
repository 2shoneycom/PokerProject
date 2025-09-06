using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_InviteFriendList : UI_FriendBase
{
    private string boundUID;
    private bool isInited = false;

    enum Buttons
    {
        UI_InviteFriendList_InviteButton,
    }

    enum Texts
    {
        UI_InviteFriendList_FriendNameText,
        UI_InviteFriendList_FriendStatusText,
    }

    enum Images
    {
        UI_InviteFriendList_Icon,
    }

    public override void Init()
    {
        if (isInited)
        {
            return;
        }

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));

        BindEvent(GetButton((int)Buttons.UI_InviteFriendList_InviteButton).gameObject, InviteFriend);

        Setting();

        isInited = true;
    }

    public void SetUID(string uid)
    {
        boundUID = uid;
    }

    public void SetFriendName(string name)
    {
        GetText((int)Texts.UI_InviteFriendList_FriendNameText).text = name;
    }

    public void SetStatusInfo(Define.Status value)
    {
        TextMeshProUGUI txt = GetText((int)Texts.UI_InviteFriendList_FriendStatusText);
        if (value == Define.Status.Online)
        {
            txt.text = "온라인";
            txt.color = Color.green;
        }
        else if (value == Define.Status.Playing)
        {
            txt.text = "게임 중";
            txt.color = Color.green;
        }
        else if (value == Define.Status.Offline)
        {
            txt.text = "오프라인";
            txt.color = Color.red;
        }
    }

    private void InviteFriend(PointerEventData data)
    {
        Managers.DB.SendInvitation(boundUID);
    }

    public override void Setting()
    {
        // 이름
        // 아이콘
        // 상태
    }
}
