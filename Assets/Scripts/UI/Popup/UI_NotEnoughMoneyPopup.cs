using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_NotEnoughMoneyPopup : UI_Popup
{
    enum GameObjects
    {
        UI_PopupClose,
    }

    enum Texts
    {
        UI_InfoText,
    }

    public override void Init()
    {
        base.Init();
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        BindEvent(GetGameObject((int)GameObjects.UI_PopupClose), (PointerEventData) => { ClosePopupUI(); });
        SetText();
    }

    void SetText()
    {
        string type = "";
        string difficulty = "";
         
        switch (Managers.CurrentGameType)
        {
            case Define.GameType.Holdem:
                type = "Holdem";
                break;
            case Define.GameType.Poker:
                type = "Holdem";
                break;
            case Define.GameType.BlackJack:
                type = "Holdem";
                break;
            default:
                type = "None";
                break;
        }

        switch (Managers.CurrentDifficulty)
        {
            case Define.Difficulty.Beginner:
                difficulty = "Beginner";
                break;
            case Define.Difficulty.Amateur:
                difficulty = "Beginner";
                break;
            case Define.Difficulty.Pro:
                difficulty = "Beginner";
                break;
            default:
                difficulty = "None";
                break;
        }

        TextMeshProUGUI text = GetText((int)Texts.UI_InfoText);
        text.text = $"{type}의 \n{difficulty}를 플레이하기에 충분한 돈이 없습니다!!";
    }
}
