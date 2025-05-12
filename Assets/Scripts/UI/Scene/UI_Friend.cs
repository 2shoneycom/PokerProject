using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Friend : UI_Scene
{
    enum Texts
    {
        UI_FriendTitleText,
    }

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

    enum Buttons
    {
        UI_AcceptFriendButton,
    }

    public override void Init()
    {
        base.Init();

        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));
        Bind<Button>(typeof(Buttons));

        SwitchToFriendList();
    }

    void LoadAndMakeFriendList()
    {
        GameObject go = GetGameObject((int)GameObjects.UI_FriendList_Contents);
        // 실제 친구 정보를 참고하여
        for (int i = 0; i < 10; i++)
        {
            GameObject friendGO = Managers.UI.MakeSubItem<UI_FriendList>(go.transform).gameObject;
            UI_FriendList friend = friendGO.GetOrAddComponent<UI_FriendList>();
        }
    }

    void ClearFriendList()
    {
        GameObject friendList = Get<GameObject>((int)GameObjects.UI_FriendList_Contents);
        foreach (Transform child in friendList.transform)
            Managers.Resource.Destroy(child.gameObject);
    }

    void LoadAndMakeAcceptFriendList()
    {
        GameObject go = GetGameObject((int)GameObjects.UI_FriendList_Contents);
        // 실제 친구 추가 정보를 참고하여
        for (int i = 0; i < 5; i++)
        {
            GameObject friendGO = Managers.UI.MakeSubItem<UI_AcceptFriendList>(go.transform).gameObject;
            UI_AcceptFriendList friend = friendGO.GetOrAddComponent<UI_AcceptFriendList>();
        }
    }

    void SwitchToAcceptFriend(PointerEventData data)
    {
        GetImage((int)Images.UI_IconAddFriend).gameObject.SetActive(false);
        GetButton((int)Buttons.UI_AcceptFriendButton).gameObject.SetActive(false);
        GetText((int)Texts.UI_FriendTitleText).text = "친구 신청 목록";

        DisBindEvent(GetImage((int)Images.UI_Backspace).gameObject, Managers.Scene.MoveToLobbyScene);
        BindEvent(GetImage((int)Images.UI_Backspace).gameObject, SwitchToFriendList);

        ClearFriendList();
        LoadAndMakeAcceptFriendList();
    }

    void SwitchToFriendList()
    {
        GetImage((int)Images.UI_IconAddFriend).gameObject.SetActive(true);
        GetButton((int)Buttons.UI_AcceptFriendButton).gameObject.SetActive(true);
        GetText((int)Texts.UI_FriendTitleText).text = "친구 목록";

        DisBindEvent(GetImage((int)Images.UI_Backspace).gameObject, SwitchToFriendList);
        BindEvent(GetImage((int)Images.UI_Backspace).gameObject, Managers.Scene.MoveToLobbyScene);

        BindEvent(GetButton((int)Buttons.UI_AcceptFriendButton).gameObject, SwitchToAcceptFriend);
        BindEvent(GetImage((int)Images.UI_IconAddFriend).gameObject, AddFriendClicked);

        ClearFriendList();
        LoadAndMakeFriendList();
    }

    void SwitchToFriendList(PointerEventData data)
    {
        SwitchToFriendList();
    }

    void AddFriendClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_AddFriendPopup>();
    }
}
