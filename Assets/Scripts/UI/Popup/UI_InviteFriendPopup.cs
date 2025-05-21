using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_InviteFriendPopup : UI_Popup
{
    enum GameObjects
    {
        UI_Block,
        UI_FriendList,
        UI_FriendList_Contents,
    }

    public override void Init()
    {
        base.Init();

        Bind<GameObject>(typeof(GameObjects));

        BindEvent(GetGameObject((int)GameObjects.UI_Block), (PointerEventData) => { ClosePopupUI(); });

        // 임시...
        GameObject go = GetGameObject((int)GameObjects.UI_FriendList_Contents);
        // 실제 친구 정보를 참고하여
        for (int i = 0; i < 3; i++)
        {
            GameObject friendGO = Managers.UI.MakeSubItem<UI_InviteFriendList>(go.transform).gameObject;
            UI_InviteFriendList friend = friendGO.GetOrAddComponent<UI_InviteFriendList>();
        }
    }
}
