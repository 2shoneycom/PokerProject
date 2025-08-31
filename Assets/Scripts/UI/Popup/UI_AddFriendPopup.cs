using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_AddFriendPopup : UI_Popup
{
    enum Buttons
    {
        UI_SearchButton,
    }

    enum GameObjects
    {
        UI_Input,
        UI_PopupClose,
        UI_SearchFriendList,
        UI_SearchFriendList_Contents,
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));

        BindEvent(GetGameObject((int)GameObjects.UI_PopupClose), (PointerEventData) => { ClosePopupUI(); });

        // 임시...
        BindEvent(GetButton((int)Buttons.UI_SearchButton).gameObject, SearchFriend);
    }

    void SearchFriend(PointerEventData data)
    {
        // 이전에 검색된 기록 초기화
        ClearSearchRecords();

        // 현재 입력된 값 (찾고자 하는 닉네임)
        string inputNickname = GetGameObject((int)GameObjects.UI_Input).GetComponent<TMP_InputField>().text;

        GameObject go = GetGameObject((int)GameObjects.UI_SearchFriendList_Contents);
        // 찾아진 유저들 목록 띄우기
        Managers.DB.GetUIDsByNickname(inputNickname, (searchedUsers) =>
        {
            if (searchedUsers != null)
            {
                foreach (string uid in searchedUsers)
                {
                    // 검색된 유저가 나라면 패스
                    if (uid == User.NowUser.GetUid())
                    {
                        continue;
                    }

                    /* (차후에 할 것) 
                    CheckIfAlreadyFriend 검사를 여기서 미리 해놓는 것이 보기 좋아보임
                    저기 뒤에서 뒤늦게 하니까 친구 추가 버튼이 한 번 보였다가 사라짐
                    */

                    GameObject searchedUserGO = Managers.UI.MakeSubItem<UI_SearchFriendList>(go.transform).gameObject;
                    UI_SearchFriendList searchedUser = searchedUserGO.GetOrAddComponent<UI_SearchFriendList>();

                    Managers.DB.GetNicknameByUID(uid, async (nickname) =>
                    {
                        if (nickname != null)
                        {
                            searchedUser.SetNickname(nickname);
                            searchedUser.SetUID(uid);
                            await searchedUser.CheckIfAlreadyFriend();
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
                Debug.Log("해당 유저를 찾지 못했습니다.");
            }
        });
    }

    private void ClearSearchRecords()
    {
        GameObject go = GetGameObject((int)GameObjects.UI_SearchFriendList_Contents);
        foreach (Transform child in go.transform)
        {
            Managers.Resource.Destroy(child.gameObject);
        }
    }
}
