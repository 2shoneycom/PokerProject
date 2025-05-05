using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Friend : UI_Scene
{
    enum Images
    {
        UI_IconAddFriend,
        UI_Backspace,
    }

    enum GameObjects
    {
        UI_FriendList,
        UI_FriendList_Contents,
    }

    public override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));
        BindEvent(GetImage((int)Images.UI_Backspace).gameObject, Managers.Scene.MoveToLobbyScene);

        GameObject go = GetGameObject((int)GameObjects.UI_FriendList_Contents);
        // 실제 친구 정보를 참고하여
        for (int i = 0; i < 10; i++)
        {
            GameObject friendGO = Managers.UI.MakeSubItem<UI_FriendList>(go.transform).gameObject;
            UI_FriendList friend = friendGO.GetOrAddComponent<UI_FriendList>();
        }
    }
}
