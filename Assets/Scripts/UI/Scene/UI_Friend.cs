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

    /*
        Scene 1(친구 목록 화면) 진입 시 친구 목록 불러오기
    */
    void LoadAndMakeFriendList()
    {
        GameObject go = GetGameObject((int)GameObjects.UI_FriendList_Contents);

        // 친구 리스트 정보 받아오기
        Managers.DB.GetFriendsData((friends) =>
        {
            if (friends != null)
            {
                foreach (string uid in friends)
                {
                    GameObject friendGO = Managers.UI.MakeSubItem<UI_FriendList>(go.transform).gameObject;
                    UI_FriendList friend = friendGO.GetOrAddComponent<UI_FriendList>();

                    Managers.DB.GetNicknameByUID(uid, (nickname) =>
                    {
                        if (nickname != null)
                        {
                            friend.SetFriendName(nickname);
                            friend.SetUID(uid);
                        }
                        else
                        {
                            Debug.Log(uid + "의 닉네임을 불러오지 못했습니다.");
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

    void ClearFriendList()
    {
        GameObject friendList = Get<GameObject>((int)GameObjects.UI_FriendList_Contents);
        foreach (Transform child in friendList.transform)
            Managers.Resource.Destroy(child.gameObject);
    }

    /*
        Scene 2(친구 요청 화면) 진입 시, 친구 요청 리스트 불러오기
    */
    void LoadAndMakeAcceptFriendList()
    {
        GameObject go = GetGameObject((int)GameObjects.UI_FriendList_Contents);

        Managers.DB.GetRequests((requests) =>
        {
            if (requests != null)
            {
                foreach (string uid in requests)
                {
                    GameObject requestGO = Managers.UI.MakeSubItem<UI_AcceptFriendList>(go.transform).gameObject;
                    UI_AcceptFriendList request = requestGO.GetOrAddComponent<UI_AcceptFriendList>();

                    Managers.DB.GetNicknameByUID(uid, (nickname) =>
                    {
                        if (nickname != null)
                        {
                            request.SetNickname(nickname);
                            request.SetUID(uid);
                        }
                        else
                        {
                            Debug.Log(uid + "의 닉네임을 불러오지 못했습니다.");
                        }
                    });
                }
            }
            else
            {
                Debug.Log("친구 요청 목록을 불러오지 못했습니다.");
            }
        });
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
