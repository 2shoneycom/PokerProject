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
        GameObject go = GetGameObject((int)GameObjects.UI_SearchFriendList_Contents);
        // 실제 친구 추가 정보를 참고하여
        for (int i = 0; i < 3; i++)
        {
            GameObject friendGO = Managers.UI.MakeSubItem<UI_SearchFriendList>(go.transform).gameObject;
            UI_SearchFriendList friend = friendGO.GetOrAddComponent<UI_SearchFriendList>();
        }
    }
}
