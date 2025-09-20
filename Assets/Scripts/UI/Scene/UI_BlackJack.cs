using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_BlackJack : UI_Scene
{
    enum Buttons
    {
        UI_RoomButton,
        UI_Player1_Button,
        UI_Player2_Button,
        UI_Player3_Button,
        UI_Player4_Button,
        UI_Player5_Button,
        UI_ButtonLL,
        UI_ButtonLM,
        UI_ButtonRM,
        UI_ButtonRR,
    }

    enum Texts
    {
        UI_ButtonLL_Text,
        UI_ButtonLM_Text,
        UI_ObjectMM_Text,
        UI_ButtonRM_Text,
        UI_ButtonRR_Text,
        UI_ChipButtonLL_Text,
        UI_ChipButtonLM_Text,
        UI_ChipButtonMM_Text,
        UI_ChipButtonRM_Text,
        UI_ChipButtonRR_Text,
        UI_TimerText,
        UI_RoomButton_Text,
        UI_Player1_NameText,
        UI_Player2_NameText,
        UI_Player3_NameText,
        UI_Player4_NameText,
        UI_Player5_NameText,
        UI_TmpWinnerShow_Text,
        UI_Player1_BetText,
        UI_Player2_BetText,
        UI_Player3_BetText,
        UI_Player4_BetText,
        UI_Player5_BetText,
        UI_Player1_BetStatusText,
        UI_Player2_BetStatusText,
        UI_Player3_BetStatusText,
        UI_Player4_BetStatusText,
        UI_Player5_BetStatusText,
        UI_Dealer_BetStatusText,
        UI_Player1_BetScoreText,
        UI_Player2_BetScoreText,
        UI_Player3_BetScoreText,
        UI_Player4_BetScoreText,
        UI_Player5_BetScoreText,
        UI_Player1_SeedMoneyText,
        UI_Player2_SeedMoneyText,
        UI_Player3_SeedMoneyText,
        UI_Player4_SeedMoneyText,
        UI_Player5_SeedMoneyText,
    }

    enum Images
    {

    }

    enum GameObjects
    {
        UI_ObjectMM,
        UI_ChipButtonLL,
        UI_ChipButtonLM,
        UI_ChipButtonMM,
        UI_ChipButtonRM,
        UI_ChipButtonRR,
        UI_ButtonLL_Block,
        UI_ButtonLM_Block,
        UI_ButtonRM_Block,
        UI_ButtonMM_Block,
        UI_ButtonRR_Block,
        UI_Backspace,
        UI_IconFriend,
        UI_Timer,
        UI_DeckBoard,
        UI_Player1,
        UI_Player2,
        UI_Player3,
        UI_Player4,
        UI_Player5,
        UI_Block,
        UI_Player1_Bet,
        UI_Player2_Bet,
        UI_Player3_Bet,
        UI_Player4_Bet,
        UI_Player5_Bet,
    }

    bool isRoomOpened = false;

    public override void Init()
    {
        base.Init();


        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        StartCoroutine(LoadingScreenSwitch(false, 2f));

        //SettingUIIconPos();

        SeatBind();
        ChipButtonBind();
        BetUISwitch(false);
        TimerSwitch(false);

        BindEvent(GetGameObject((int)GameObjects.UI_Backspace), LeaveRoomClicked);
        BindEvent(GetGameObject((int)GameObjects.UI_IconFriend), IconFriendClicked);

        SetRoomButton(isRoomOpened);
    }

    IEnumerator LoadingScreenSwitch(bool isOn, float time)
    {
        // Debug.Log("코루틴 시작");  // 로그 찍기
        yield return new WaitForSeconds(time);
        //Debug.Log("2초 후");
        GetGameObject((int)GameObjects.UI_Block).SetActive(isOn);
    }

    void SeatBind()
    {
        for (int i = 0; i < JackGameControl.MAX_PLAYER_NUM; i++)
        {
            string go = $"UI_Player{i + 1}_Button";
            int num = i;

            GetButton((int)Enum.Parse(typeof(Buttons), go)).gameObject.BindEvent(PointerEventData =>
            {
                Managers.Seat.HaveSeat(User.NowUser.GetUid(), User.NowUser.GetNickName(), num);
            });
        }
    }

    void ChipButtonBind()
    {
        GetGameObject((int)GameObjects.UI_ChipButtonLL).BindEvent
            (PointerEventData => { JackGameControl.Bet.JackBetting(User.NowGamePlayer.GameIndex, 0, 500); });

        GetGameObject((int)GameObjects.UI_ChipButtonLM).BindEvent
            (PointerEventData => { JackGameControl.Bet.JackBetting(User.NowGamePlayer.GameIndex, 0, 1000); });

        GetGameObject((int)GameObjects.UI_ChipButtonMM).BindEvent
            (PointerEventData => { JackGameControl.Bet.JackBetting(User.NowGamePlayer.GameIndex, 0, 2000); });

        GetGameObject((int)GameObjects.UI_ChipButtonRM).BindEvent
            (PointerEventData => { JackGameControl.Bet.JackBetting(User.NowGamePlayer.GameIndex, 0, 4000); });

        GetGameObject((int)GameObjects.UI_ChipButtonRR).BindEvent
            (PointerEventData => { JackGameControl.Bet.JackBetting(User.NowGamePlayer.GameIndex, 0, 8000); });
    }

    public void FirstBetSetting()
    {
        GetGameObject((int)GameObjects.UI_ButtonMM_Block).SetActive(false);
        GetGameObject((int)GameObjects.UI_ObjectMM).SetActive(true);

        ResetButtonSetting();
        ConfirmButtonSetting();
    }

    public void FirstBetEarlyEnd()
    {
        GetGameObject((int)GameObjects.UI_ButtonMM_Block).SetActive(true);
        //ChipUISwitch(false);
        Button bt = GetButton((int)Buttons.UI_ButtonLL);
        bt.interactable = false;
        bt = GetButton((int)Buttons.UI_ButtonRR);
        bt.interactable = false;
    }

    public void FirstBetEnd()
    {
        BettingSetting();
    }

    void ResetButtonSetting()
    {
        Button bt = GetButton((int)Buttons.UI_ButtonLL);

        bt.onClick.RemoveAllListeners();
        bt.onClick.AddListener(() => JackGameControl.Bet.JackBettingReset(User.NowGamePlayer.GameIndex));
        bt.interactable = true;

        GetText((int)Texts.UI_ButtonLL_Text).text = "리셋";
        ColorUtility.TryParseHtmlString("#AD3232", out Color targetColor);
        bt.gameObject.GetComponent<Image>().color = targetColor;
    }

    void ConfirmButtonSetting()
    {
        Button bt = GetButton((int)Buttons.UI_ButtonRR);

        bt.onClick.RemoveAllListeners();
        bt.onClick.AddListener(() => JackGameControl.Bet.JackBettingConfirm(User.NowGamePlayer.GameIndex));
        bt.interactable = true;

        GetText((int)Texts.UI_ButtonRR_Text).text = "게임시작";
        ColorUtility.TryParseHtmlString("#402FC0", out Color targetColor);
        bt.gameObject.GetComponent<Image>().color = targetColor;
    }

    public void BettingSetting()
    {
        GetGameObject((int)GameObjects.UI_ButtonMM_Block).SetActive(false);
        GetGameObject((int)GameObjects.UI_ObjectMM).SetActive(false);
        DoubleDownButtonSetting();
        SplitButtonSetting();
        StandButtonSetting();
        HitButtonSetting();
    }

    public void NowPlayerBetSettingSwitch(bool isOn)
    {
        if (isOn == false)
        {
            GetButton((int)Buttons.UI_ButtonLL).interactable = isOn;
            GetButton((int)Buttons.UI_ButtonLM).interactable = isOn;
        }
        else
        {
            if (isHit == true)
            {
                GetButton((int)Buttons.UI_ButtonLL).interactable = false;
                isHit = false;
            }
            else
            {
                GetButton((int)Buttons.UI_ButtonLL).interactable = true;
            }

            if (JackGameControl.Players.IsPlayerCanSplit(User.NowGamePlayer.GameIndex))
                GetButton((int)Buttons.UI_ButtonLM).interactable = true;
            else
                GetButton((int)Buttons.UI_ButtonLM).interactable = false;
        }
        GetButton((int)Buttons.UI_ButtonRM).interactable = isOn;
        GetButton((int)Buttons.UI_ButtonRR).interactable = isOn;
    }

    void DoubleDownButtonSetting()
    {
        Button bt = GetButton((int)Buttons.UI_ButtonLL);

        bt.onClick.RemoveAllListeners();
        bt.onClick.AddListener(() => DoubleDownClicked());
        bt.interactable = false;

        GetText((int)Texts.UI_ButtonLL_Text).text = "더블 다운";
        ColorUtility.TryParseHtmlString("#8832AD", out Color targetColor);
        bt.gameObject.GetComponent<Image>().color = targetColor;
    }

    void DoubleDownClicked()
    {
        // 1장만 더 받는 조건으로 돈을 2배로 검
        // 돈 2배로 베팅
        int baseBet = User.NowGamePlayer.GetBlackJackBaseBet();
        JackGameControl.Bet.JackBetting(User.NowGamePlayer.GameIndex, JackGameControl.Control.PlayerSplit, baseBet);
        // 1장 받기
        StartCoroutine(JackGameControl.Card.DealingCard(User.NowGamePlayer.GameIndex, JackGameControl.Control.PlayerSplit));
        // 베팅 종료
        SyncSystem.Sync.JackNormalBetEnd();
    }

    void SplitButtonSetting()
    {
        Button bt = GetButton((int)Buttons.UI_ButtonLM);

        bt.onClick.RemoveAllListeners();
        bt.onClick.AddListener(() => SplitClicked());
        bt.interactable = false;

        GetText((int)Texts.UI_ButtonLM_Text).text = "스플릿";
        ColorUtility.TryParseHtmlString("#443B3B", out Color targetColor);
        bt.gameObject.GetComponent<Image>().color = targetColor;
    }

    void SplitClicked()
    {
        // 1. 카드 나눠짐
        // 2. 새로운 카드 1장 받음
        // 3. 새롭게 배팅 시작
        SyncSystem.Sync.JackPlayerSplitSetting(User.NowGamePlayer.GameIndex, JackGameControl.Control.PlayerSplit);
    }

    void StandButtonSetting()
    {
        Button bt = GetButton((int)Buttons.UI_ButtonRM);

        bt.onClick.RemoveAllListeners();
        bt.onClick.AddListener(() => StandClicked());
        bt.interactable = false;

        GetText((int)Texts.UI_ButtonRM_Text).text = "스탠드";
        ColorUtility.TryParseHtmlString("#A62929", out Color targetColor);
        bt.gameObject.GetComponent<Image>().color = targetColor;
    }

    public void StandClicked()
    {
        // 카드 그만 받기
        SyncSystem.Sync.JackNormalBetEnd();
    }

    bool isHit = false;
    void HitButtonSetting()
    {
        Button bt = GetButton((int)Buttons.UI_ButtonRR);

        bt.onClick.RemoveAllListeners();
        bt.onClick.AddListener(() => HitClicked());
        bt.interactable = false;

        GetText((int)Texts.UI_ButtonRR_Text).text = "히트";
        ColorUtility.TryParseHtmlString("#199A73", out Color targetColor);
        bt.gameObject.GetComponent<Image>().color = targetColor;
    }

    int curPlayerIndex = -1;
    int curPlayerSplit = -1;
    void HitClicked()
    {
        isHit = true;
        curPlayerIndex = User.NowGamePlayer.GameIndex;
        curPlayerSplit = JackGameControl.Control.PlayerSplit;

        SyncSystem.Sync.JackStopBetTimer();

        StartCoroutine(GiveCardWaitRestart());
    }

    public void SetIsHit(bool value) { isHit = value; }

    IEnumerator GiveCardWaitRestart()
    {
        // 카드 1장 더 받기
        StartCoroutine(JackGameControl.Card.DealingCard(curPlayerIndex, curPlayerSplit));
        yield return new WaitForSeconds(1f);

        if (JackGameControl.Players.GetPlayerIsGameEnd(curPlayerIndex, curPlayerSplit) == false)
        {
            var score = JackGameControl.Players.GetPlayerCardScore(curPlayerIndex, curPlayerSplit);
            if (score.Item1 == 21 || score.Item2 == 21)
                yield break;

            // 타이머 초기화
            SyncSystem.Sync.JackRestartBetTimer();
        }
    }

    public void ChipUISwitch(bool isOn)
    {
        foreach (GameObjects go in Enum.GetValues(typeof(GameObjects)))
        {
            if (go.ToString().Contains("Chip"))
            {
                GetGameObject((int)go).SetActive(isOn);
            }
        }
    }

    public void BetUISwitch(bool isOn)
    {
        for (int i = 1; i <= JackGameControl.MAX_PLAYER_NUM; i++)
        {
            if (isOn == true)
            {
                int gameIndex = i - 1;
                if (JackGameControl.Players.GetPlayerUID(gameIndex) == "")
                    continue;
            }

            GetGameObject((int)Enum.Parse(typeof(GameObjects), $"UI_Player{i}_Bet")).SetActive(isOn);
        }

        ChipUISwitch(isOn);
    }

    public void AllBlockSwitch(bool isOn)
    {
        foreach (GameObjects go in Enum.GetValues(typeof(GameObjects)))
        {
            if (go.ToString().Contains("Block"))
            {
                GetGameObject((int)go).SetActive(isOn);
            }
        }

        if (!isOn)
            GetText((int)Texts.UI_ObjectMM_Text).text = "";
        else
            GetText((int)Texts.UI_ObjectMM_Text).text = "관전중";
    }

    public void TimerSwitch(bool isOn)
    {
        GetGameObject((int)GameObjects.UI_Timer).SetActive(isOn);
    }

    public void SetTimerText(float time)
    {
        GetText((int)Texts.UI_TimerText).text = time.ToString("F1");    //time.Tostring("F1")는 소숫점 첫째자리까지만 표기
    }

    public void GameStartButtonSetting()
    {
        GameObject go = GetGameObject((int)GameObjects.UI_ButtonRR_Block);
        go.SetActive(false);

        Button bt = GetButton((int)Buttons.UI_ButtonRR);
        bt.onClick.RemoveAllListeners();
        bt.onClick.AddListener(GameStartButtonClicked);

        GetText((int)Texts.UI_ButtonRR_Text).text = "게임시작";
        ColorUtility.TryParseHtmlString("#402FC0", out Color targetColor);
        bt.gameObject.GetComponent<Image>().color = targetColor;
    }

    void GameStartButtonClicked()
    {
        GetButton((int)Buttons.UI_ButtonRR).interactable = false;
        // 게임 시작
        SyncSystem.Sync.JackStartSync();
        Debug.Log("1");
    }

    void IconFriendClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_InviteFriendPopup>();
    }

    void SetRoomButton(bool isRoomOpened)
    {
        if (!isRoomOpened)
        {
            ColorUtility.TryParseHtmlString("#FF0000", out Color targetColor);
            GetButton((int)Buttons.UI_RoomButton).GetComponent<Image>().color = targetColor;

            GetText((int)Texts.UI_RoomButton_Text).text = "방 공개";
            BindEvent(GetButton((int)Buttons.UI_RoomButton).gameObject, OpenRoomClicked);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#CFBFBF", out Color targetColor);
            GetButton((int)Buttons.UI_RoomButton).GetComponent<Image>().color = targetColor;

            GetText((int)Texts.UI_RoomButton_Text).text = "방 이동";
            BindEvent(GetButton((int)Buttons.UI_RoomButton).gameObject, MoveRoomClicked);
        }
    }

    void OpenRoomClicked(PointerEventData data)
    {

    }

    void MoveRoomClicked(PointerEventData data)
    {

    }

    public void UpdatePlayerName(int index, string pNickName)
    {
        GetText((int)Enum.Parse(typeof(Texts), $"UI_Player{index}_NameText")).text = pNickName;
    }

    public void UpdatePlayerButton(int index, bool isOn = false)
    {
        GetButton((int)Enum.Parse(typeof(Buttons), $"UI_Player{index}_Button")).gameObject.SetActive(isOn);
    }

    public void UpdatePlayerBetStatusText(int index, string status)
    {
        GetText((int)Enum.Parse(typeof(Texts), $"UI_Player{index}_BetStatusText")).text = status;
    }

    public void UpdatePlayerBetScoreText(int index, string status)
    {
        string str = $"UI_Player{index}_BetScoreText";
        GetText((int)Enum.Parse(typeof(Texts), str)).text = status;
    }

    public void UpdateDealerStatusText(string status)
    {
        GetText((int)Texts.UI_Dealer_BetStatusText).text = status;
    }

    public GameObject GetPlayerGameObjcet(int index)
    {
        return GetGameObject((int)Enum.Parse(typeof(GameObjects), $"UI_Player{index + 1}"));
    }

    public void UpdateBetMoney()
    {
        for (int i = 1; i <= JackGameControl.MAX_PLAYER_NUM; i++)
        {
            int gameIndex = i - 1;
            string text = "";

            for(int j = 0; j < JackGameControl.MAX_SPLIT_NUM; j++)
            {
                int bet = JackGameControl.Players.GetPlayerBet(gameIndex, j);

                if (j == 0)
                    text += bet.ToString();
                else
                    text += "/" + bet.ToString();
            }

            GetText((int)Enum.Parse(typeof(Texts), $"UI_Player{i}_BetText")).text = text;
        }
    }

    public void UpdateSeedMoney()
    {
        for (int i = 1; i <= JackGameControl.MAX_PLAYER_NUM; i++)
        {
            int gameIndex = i - 1;
            GetText((int)Enum.Parse(typeof(Texts), $"UI_Player{i}_SeedMoneyText")).text = JackGameControl.Players.GetPlayerSeedMoney(gameIndex).ToString();
        }
    }

    private void LeaveRoomClicked(PointerEventData data)
    {
        BlackJackScene jackScene = (BlackJackScene)Managers.Scene.CurrentScene;
        jackScene.RequestLeaveRoom();
    }

}
