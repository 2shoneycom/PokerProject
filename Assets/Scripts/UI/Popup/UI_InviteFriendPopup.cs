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

        LoadOnlineFriends();
    }

    private void LoadOnlineFriends()
    {
        GameObject go = GetGameObject((int)GameObjects.UI_FriendList_Contents);

        // 친구 목록 중에서...
        Managers.DB.GetFriendsData((friends) =>
        {
            if (friends != null)
            {
                foreach (string uid in friends)
                {
                    Managers.DB.GetStatusByUID(uid, (status) =>
                    {
                        // 온라인 상태인 친구만...
                        if (status == Define.Status.Online)
                        {
                            GameObject friendGO = Managers.UI.MakeSubItem<UI_InviteFriendList>(go.transform).gameObject;
                            UI_InviteFriendList friend = friendGO.GetOrAddComponent<UI_InviteFriendList>();
                            friend.Init();
                            friend.SetUID(uid);
                            friend.SetStatusInfo(status);

                            Managers.DB.GetNicknameByUID(uid, (nickname) =>
                            {
                                if (nickname != null)
                                {
                                    friend.SetFriendName(nickname);
                                }
                            });
                        }
                    });
                }
            }
            else
            {
                Debug.Log("친구 목록을 불러오지 못했습니다.");
            }
        });
    }
}
