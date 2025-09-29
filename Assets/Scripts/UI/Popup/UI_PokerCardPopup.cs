using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_PokerCardPopup : UI_Popup
{
    enum Texts
    {
        UI_BackText,
    }

    enum Images
    {
        UI_Card1,
        UI_Card2,
        UI_Card3,
        UI_Card4,
    }

    public const float CARD_SEL_TIME = 10.2f;
    private Coroutine cardSelTimer;

    bool firstSel = true;
    int firstSelCardIndex = -1;
    int secondSelCardIndex = -1;

    public override void Init()
    {
        base.Init();

        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));

        CardImageInit();
        CardBind();
        cardSelTimer = StartCoroutine(CardSelTimer(CARD_SEL_TIME));
    }

    void CardImageInit()
    {
        for (int i = 0; i < 4; i++)
        {
            int cardDetail = PokerGameControl.Players.PlayerCards[User.NowGamePlayer.GameIndex, i];
            Image target = GetImage((int)Enum.Parse(typeof(Images), $"UI_Card{i + 1}"));
            target.sprite = PokerGameControl.Card.GetRightCardImage(cardDetail);
        }
    }

    void CardBind()
    {
        for (int i = 0; i < 4; i++)
        {
            string go = $"UI_Card{i + 1}";
            int num = i;
            GetImage((int)Enum.Parse(typeof(Images), go)).gameObject.BindEvent(PointerEventData => { CardSel(num); });
        }
    }

    void CardSel(int cardIndex)
    {
        if (firstSel)
        {
            firstSel = false;
            firstSelCardIndex = cardIndex;
            // 시각 효과
            DelCardUISwitch(cardIndex, true);

            GetText((int)Texts.UI_BackText).text = "공개할 카드를 선택하세요";
        }
        else
        {
            if (firstSelCardIndex == cardIndex || secondSelCardIndex == cardIndex)
                return;

            if (secondSelCardIndex != -1)
            {
                CardUISwitch(secondSelCardIndex, false);
            }
            secondSelCardIndex = cardIndex;
            CardUISwitch(cardIndex, true);
        }
    }

    void DelCardUISwitch(int cardIndex, bool isOn)
    {
        Image target = GetImage((int)Enum.Parse(typeof(Images), $"UI_Card{cardIndex + 1}"));

        ColorUtility.TryParseHtmlString("#878787", out Color targetColor);
        target.color = targetColor;
        target.gameObject.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
    }

    void CardUISwitch(int cardIndex, bool isOn)
    {
        Image target = GetImage((int)Enum.Parse(typeof(Images), $"UI_Card{cardIndex + 1}"));
        ColorUtility.TryParseHtmlString("#FFFFFF", out Color targetColor);

        if (isOn)
        {
            target.color = targetColor;
            target.gameObject.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        }
        else
        {
            target.color = targetColor;
            target.gameObject.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
        }
    }

    public void TimerRunOutAutoSel()
    {
        if (firstSel)        // 첫번째 선택도 안했을 경우
            CardSel(3);

        if (firstSelCardIndex == 2)
            CardSel(3);
        else
            CardSel(2);
    }

    IEnumerator CardSelTimer(float time)
    {
        Debug.Log("Timer Start");
        while (time > 0.2f)
        {
            time -= Time.deltaTime;
            //_holdemUI.SetTimerText(time - 0.2f);
            yield return null;
        }

        //_holdemUI.SetTimerText(0f);

        // 현재 플레이어가 n초 동안 카드를 누르지 않았을 경우 랜덤 선택
        if (firstSelCardIndex == -1 || secondSelCardIndex == -1)
            TimerRunOutAutoSel();
        Debug.Log("Timer End");
        yield return new WaitForSeconds(time);
        PokerGameControl.Control.SelectedCardIndex(firstSelCardIndex, secondSelCardIndex);
    }
}