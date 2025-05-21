using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_DailyCheck : UI_Popup
{
    enum Buttons
    {
        UI_GetRewardButton,
    }

    enum GameObjects
    {
        UI_PopupClose,
        UI_DailyList,
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));

        GameObject gridPanel = Get<GameObject>((int)GameObjects.UI_DailyList);
        foreach (Transform child in gridPanel.transform)
            Managers.Resource.Destroy(child.gameObject);

        for (int i = 0; i < 7; i++)
        {
            GameObject item = Managers.UI.MakeSubItem<UI_DailyItem>(parent: gridPanel.transform).gameObject;
            UI_DailyItem dailyItem = item.GetOrAddComponent<UI_DailyItem>();
            dailyItem.SetInfo($"{i + 1}");
        }

        BindEvent(GetGameObject((int)GameObjects.UI_PopupClose), (PointerEventData) => { ClosePopupUI(); });
        BindEvent(GetButton((int)Buttons.UI_GetRewardButton).gameObject, GetRewardButton);
    }

    void GetRewardButton(PointerEventData data)
    {
        // 보상 받기
        ClosePopupUI();
    }
}
