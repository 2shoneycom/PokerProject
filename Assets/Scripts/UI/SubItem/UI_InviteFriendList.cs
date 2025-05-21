using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InviteFriendList : UI_FriendBase
{
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
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Setting();
    }

    public override void Setting()
    {
        // 이름
        // 아이콘
        // 상태
    }
}
