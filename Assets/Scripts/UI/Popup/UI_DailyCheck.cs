using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    bool isClaimed;
    int streak;
    UI_Lobby _lobby;

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

        isClaimed = User.NowUser.GetisDailyClaimed();
        streak = User.NowUser.Getstreak();

        RefreshDailyGrid(streak);
        RefreshButton(isClaimed);
    }

    void GetRewardButton(PointerEventData _)
    {
        if (!isClaimed)
        {
            var btn = GetButton((int)Buttons.UI_GetRewardButton);
            btn.interactable = false;
            Managers.DB.GetDailyReward(streak, () =>
            {
                isClaimed = User.NowUser.GetisDailyClaimed();
                streak = User.NowUser.Getstreak();

                _lobby = (UI_Lobby)Managers.UI.SceneUI;
                _lobby.SetMoneyText(User.NowUser.GetSeedMoney());

                RefreshDailyGrid(streak);
                RefreshButton(isClaimed);
            });
        }
    }

    private void RefreshDailyGrid(int streak)
    {
        GameObject grid = Get<GameObject>((int)GameObjects.UI_DailyList);
        int idx = 0;
        foreach (Transform child in grid.transform)
        {
            var item = child.GetComponent<UI_DailyItem>();

            if (idx < streak)
                item.SetInfo($"X");
            else
                item.SetInfo($"{idx + 1}");

            idx++;
        }
    }

    private void RefreshButton(bool isClaimed)
    {
        var btn = GetButton((int)Buttons.UI_GetRewardButton);
        var label = btn.GetComponentInChildren<TextMeshProUGUI>();

        btn.interactable = !isClaimed;
        label.text = !isClaimed ? "보상 받기" : "오늘 수령 완료";
    }
}
