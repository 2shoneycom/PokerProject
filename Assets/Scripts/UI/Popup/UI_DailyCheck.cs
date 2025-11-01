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

        // ✅ 팝업이 뜰 때 초기 상태 반영
        var vm = Managers.Reward.PrepareDailyState();
        ApplyInitialState(vm);
    }

    public void ApplyInitialState(DBManager.ClaimView vm)
    {
        var btn = GetButton((int)Buttons.UI_GetRewardButton);
        var label = btn.GetComponentInChildren<TextMeshProUGUI>();

        btn.interactable = vm.canClaim;
        label.text = vm.canClaim ? "보상 받기" : "오늘 수령 완료";

        RefreshDailyGridByStreak(vm.streakToShow);
    }

    private void RefreshDailyGridByStreak(int streak)
    {
        GameObject grid = Get<GameObject>((int)GameObjects.UI_DailyList);
        int idx = 0;
        foreach (Transform child in grid.transform)
        {
            var item = child.GetComponent<UI_DailyItem>();
            // 1~streak까지는 체크(✓), 그 외는 숫자 그대로
            if (idx < streak)
                item.SetInfo($"X");
            else
                item.SetInfo($"{idx + 1}");

            idx++;
        }

        // 텍스트가 바뀌었으니 재바인딩
        foreach (Transform child in grid.transform)
        {
            var item = child.GetComponent<UI_DailyItem>();
            item.Init();
        }
    }

    void GetRewardButton(PointerEventData _)
    {
        var btn = GetButton((int)Buttons.UI_GetRewardButton);
        var label = btn.GetComponentInChildren<TextMeshProUGUI>();

        // 중복 클릭 방지
        if (!btn.interactable) return;
        btn.interactable = false;

        Managers.Reward.DailyGift(result =>
        {
            if (!result.committed)
            {
                // 이미 수령/오류
                label.text = "오늘 수령 완료";
                // 최신 캐시 기준으로 다시 그리드
                var vm = Managers.Reward.PrepareDailyState();
                RefreshDailyGridByStreak(vm.streakToShow);
                return;
            }

            // 성공: 버튼/그리드/머니 즉시 반영
            label.text = "오늘 수령 완료";
            RefreshDailyGridByStreak(result.streak);

            // 로비에 있다면 머니 텍스트 업데이트(아래 UI_Lobby에 메서드 추가됨)
            var lobby = Managers.UI.SceneUI as UI_Lobby;
            lobby?.SetMoneyText(result.seedMoney);
        });
    }
}
