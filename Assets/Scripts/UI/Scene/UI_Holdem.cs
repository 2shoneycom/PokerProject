using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Holdem : UI_Scene
{
    const float originIconSize = 1.3f;
    const float biggerIconSize = 1.8f;

    enum Buttons
    {
        UI_Buttons_Die,
        UI_Buttons_Call,
        UI_Buttons_Double,
        UI_Buttons_Quater,
        UI_Buttons_Half,
        UI_Buttons_AllIn,
        UI_RoomButton,
        UI_GameStartButton,
    }

    enum Texts
    {
        UI_Player1_NameText,
        UI_Player2_NameText,
        UI_Player3_NameText,
        UI_Player4_NameText,
        UI_Player5_NameText,
        UI_Player6_NameText,
        UI_Player7_NameText,
        UI_PotMoney_Text,
        UI_RoomButton_Text,
        UI_TmpWinnerShow_Text,
        UI_Player1_BetText,
        UI_Player2_BetText,
        UI_Player3_BetText,
        UI_Player4_BetText,
        UI_Player5_BetText,
        UI_Player6_BetText,
        UI_Player7_BetText,
        UI_Player1_SeedMoneyText,
        UI_Player2_SeedMoneyText,
        UI_Player3_SeedMoneyText,
        UI_Player4_SeedMoneyText,
        UI_Player5_SeedMoneyText,
        UI_Player6_SeedMoneyText,
        UI_Player7_SeedMoneyText,
        UI_TimerText,
        UI_Buttons_Call_Text,
        UI_Buttons_CallMoney_Text,
        UI_Buttons_DoubleMoney_Text,
        UI_Buttons_HalfMoney_Text,
        UI_Buttons_QuaterMoney_Text,
        UI_Buttons_AllInMoney_Text,
    }

    enum Images
    {
        UI_Player1,
        UI_Player2,
        UI_Player3,
        UI_Player4,
        UI_Player5,
        UI_Player6,
        UI_Player7,
        UI_Player1_Icon,
        UI_Player2_Icon,
        UI_Player3_Icon,
        UI_Player4_Icon,
        UI_Player5_Icon,
        UI_Player6_Icon,
        UI_Player7_Icon,
        UI_PotMoney_Icon,
    }

    enum GameObjects
    {
        UI_Backspace,
        UI_IconFriend,
        UI_TmpWinnerShow,
        UI_Timer,
        UI_Player1_Panel,
        UI_Player2_Panel,
        UI_Player3_Panel,
        UI_Player4_Panel,
        UI_Player5_Panel,
        UI_Player6_Panel,
        UI_Player7_Panel,
        UI_Player1_Bet,
        UI_Player2_Bet,
        UI_Player3_Bet,
        UI_Player4_Bet,
        UI_Player5_Bet,
        UI_Player6_Bet,
        UI_Player7_Bet,
        UI_Block,
    }

    string[] BetType =
{
        "Die",
        "Call",
        "Double",
        "Quater",
        "Half",
        "AllIn"
    };

    Image onTurnPlayer = null;
    HoldemGameControl _control;

    public override void Init()
    {
        //        base.Init();

        Managers.UI.SetWorldSpaceUI(gameObject);

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));
        winnerIndex = new List<int>();

        HoldemScene holdemScene = (HoldemScene)Managers.Scene.CurrentScene;
        _control = holdemScene.GetControl();

        SettingUIIconPos();

        SeatBind();
        BetButtonBind();
        UISwitch(false);
        BetUISwitch(false);
        TimerSwitch(false);

        GetGameObject((int)GameObjects.UI_TmpWinnerShow).SetActive(false);
        GetButton((int)Buttons.UI_GameStartButton).gameObject.SetActive(false);
        BindEvent(GetGameObject((int)GameObjects.UI_Backspace), LeaveRoomClicked);
        BindEvent(GetGameObject((int)GameObjects.UI_IconFriend), IconFriendClicked);

        SetRoomButton();

        StartCoroutine(LoadingScreenSwitch(false, 2f));
    }

    private void Update()
    {
        OnTurnEffect();
    }

    void OnTurnEffect()
    {
        if (onTurnPlayer == null) return;

        // 시간에 따라 Hue 값 변경 (0~1 범위를 순환)
        float h = Mathf.PingPong(Time.time * Managers.UI.effectSpeed, 1f);
        Color rainbow = Color.HSVToRGB(h, 1f, 1f);

        onTurnPlayer.material.SetColor("_SolidOutline", rainbow);
    }

    public void SetOnTurnPlayer(int playerIndex)
    {
        onTurnPlayer = GetImage((int)Enum.Parse(typeof(Images), $"UI_Player{playerIndex}"));
        onTurnPlayer.material = Managers.UI.OnTurnMaterial;
    }

    public void ResetOnTurnPlayer()
    {
        Image exTurnPlayer = onTurnPlayer;
        onTurnPlayer = null;

        if (exTurnPlayer != null)
            exTurnPlayer.material = Managers.UI.OffTurnMaterial;
    }

    IEnumerator LoadingScreenSwitch(bool isOn, float time)
    {
        // Debug.Log("코루틴 시작");  // 로그 찍기
        yield return new WaitForSeconds(time);
        //Debug.Log("2초 후");
        GetGameObject((int)GameObjects.UI_Block).SetActive(isOn);
    }

    void SettingUIIconPos()
    {
        GameObject go = GetGameObject((int)GameObjects.UI_Backspace);
        // 0,0 은 왼쪽 아래, 1,1 은 오른쪽 위
        go.transform.position =
            Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.nearClipPlane));
        // Z 축이 -가 되서 클릭이 안되는 현상 발생.
        go.transform.position = SetZeroZ(go.transform);

        go = GetGameObject((int)GameObjects.UI_IconFriend);
        go.transform.position =
            Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
        go.transform.position = SetZeroZ(go.transform);

        RectTransform toRect = GetGameObject((int)GameObjects.UI_IconFriend).GetComponent<RectTransform>();
        RectTransform targetRect = GetButton((int)Buttons.UI_RoomButton).GetComponent<RectTransform>();

        // 기준 오브젝트의 왼쪽 중앙 위치를 구함
        Vector3 leftCenterLocal = new Vector3(-toRect.rect.width, -toRect.rect.height * 0.5f, 0);
        Vector3 leftCenterWorld = toRect.TransformPoint(leftCenterLocal);

        // A 오브젝트를 해당 위치로 이동
        targetRect.position = leftCenterWorld;
    }

    Vector3 SetZeroZ(Transform transform)
    {
        Vector3 zeroZ = transform.position;
        zeroZ.z = 0;
        return zeroZ;
    }

    public void UISwitch(bool isOn)
    {
        foreach (int idx in Enum.GetValues(typeof(Buttons)))
        {
            if (idx == (int)Buttons.UI_RoomButton || idx == (int)Buttons.UI_GameStartButton)
                continue;

            GetButton(idx).interactable = false;
            GetButton(idx).gameObject.SetActive(isOn);
        }
        if (isOn)
            BetMoneyTextUpdate("", 0, false, true);
        GetImage((int)Images.UI_PotMoney_Icon).gameObject.SetActive(isOn);
    }

    public void BetUISwitch(bool isOn)
    {
        if (isOn == false)      // 게임 안할때라는 의미
        {
            foreach (GameObjects go in Enum.GetValues(typeof(GameObjects)))
            {
                if (go.ToString().Contains("Bet"))
                {
                    GetGameObject((int)go).SetActive(isOn);
                }
            }
        }
        else        // 게임 중이라면 참여하는 플레이어만 on
        {
            for (int i = 1; i <= 7; i++)
            {
                int gameIndex = _control.ConvertUItoGame(i - 1);
                if (_control.Players.GetPlayerUID(gameIndex) == "")
                    continue;

                GetGameObject((int)Enum.Parse(typeof(GameObjects), $"UI_Player{i}_Bet")).SetActive(isOn);
            }
        }
    }

    public void GameStartButtonOn(bool isOn = true)
    {
        GameObject bt = GetButton((int)Buttons.UI_GameStartButton).gameObject;
        bt.SetActive(isOn);

        bt.DisBindEvent(GameStartButtonClicked);
        bt.BindEvent(GameStartButtonClicked);
    }

    void GameStartButtonClicked(PointerEventData data)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} UI_Holdem.cs 파일의 GameStartButtonClicked 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        GetButton((int)Buttons.UI_GameStartButton).gameObject.SetActive(false);

        // 게임 시작
        _control.Sync.HoldemStartSync();
    }

    public void BetButtonInteractiveSwitch(string betType, bool isOn)
    {
        string type = $"UI_Buttons_{betType}";
        GetButton((int)Enum.Parse(typeof(Buttons), type)).interactable = isOn;
    }

    public void UpdatePlayerName(int index, string pNickName)
    {
        GetText((int)Enum.Parse(typeof(Texts), $"UI_Player{index}_NameText")).text = pNickName;
    }

    public void UpdatePlayerIcon(int index, string pNickName, bool isOn = false)
    {
        GetGameObject((int)Enum.Parse(typeof(GameObjects), $"UI_Player{index}_Panel")).SetActive(isOn);
    }

    public void UpdatePotMoney()
    {
        GetText((int)Texts.UI_PotMoney_Text).text = $"{_control.PotMoney}";
    }

    public void UpdateBetMoney()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} UI_Holdem.cs 파일의 UpdateBetMoney 함수 실행"); // 디버깅 추적용 (25.11.12 승헌

        for (int i = 1; i <= 7; i++)
        {
            int gameIndex = _control.ConvertUItoGame(i - 1);
            GetText((int)Enum.Parse(typeof(Texts), $"UI_Player{i}_BetText")).text = _control.Players.GetPlayerBet(gameIndex).ToString();
        }
    }

    public void UpdateSeedMoney()
    {
        for (int i = 1; i <= 7; i++)
        {
            int gameIndex = _control.ConvertUItoGame(i - 1);
            GetText((int)Enum.Parse(typeof(Texts), $"UI_Player{i}_SeedMoneyText")).text = _control.Players.GetPlayerSeedMoney(gameIndex).ToString();
        }
    }

    public void TimerSwitch(bool isOn)
    {
        GetGameObject((int)GameObjects.UI_Timer).SetActive(isOn);
    }

    public void SetTimerText(float time)
    {
        GetText((int)Texts.UI_TimerText).text = time.ToString("F1");    //time.Tostring("F1")는 소숫점 첫째자리까지만 표기
    }

    void SeatBind()
    {
        for (int i = 0; i < 7; i++)
        {
            string go = $"UI_Player{i + 1}_Panel";
            int num = i;
            GetGameObject((int)Enum.Parse(typeof(GameObjects), go)).BindEvent(PointerEventData =>
            {
                Managers.Seat.HaveSeat(User.NowUser.GetUid(), User.NowUser.GetNickName(), num);
            });
        }
    }

    void SetRoomButton()
    {
        bool isRoomOpened = PhotonNetwork.CurrentRoom.IsVisible;

        if (PhotonNetwork.IsMasterClient == false) isRoomOpened = true;

        if (!isRoomOpened)
            SetRoomOpen();
        else
            SetRoomMove();
    }

    void SetRoomOpen()
    {
        DisBindEvent(GetButton((int)Buttons.UI_RoomButton).gameObject, MoveRoomClicked);
        ColorUtility.TryParseHtmlString("#FF0000", out Color targetColor);
        GetButton((int)Buttons.UI_RoomButton).GetComponent<Image>().color = targetColor;

        GetText((int)Texts.UI_RoomButton_Text).text = "방 공개";
        BindEvent(GetButton((int)Buttons.UI_RoomButton).gameObject, OpenRoomClicked);
    }

    void SetRoomMove()
    {
        DisBindEvent(GetButton((int)Buttons.UI_RoomButton).gameObject, OpenRoomClicked);
        ColorUtility.TryParseHtmlString("#CFBFBF", out Color targetColor);
        GetButton((int)Buttons.UI_RoomButton).GetComponent<Image>().color = targetColor;

        GetText((int)Texts.UI_RoomButton_Text).text = "방 이동";
        BindEvent(GetButton((int)Buttons.UI_RoomButton).gameObject, MoveRoomClicked);
    }

    public void BetMoneyTextUpdate(string betType, int betMoney, bool isOn, bool isFirst = false)
    {
        if(isFirst == true)
        {
            foreach(string bet in BetType)
            {
                if (bet == "Die") continue;
                GetText((int)Enum.Parse(typeof(Texts), $"UI_Buttons_{bet}Money_Text")).gameObject.SetActive(false);
            }
            return;
        }

        if (betType == "Die") return;

        TextMeshProUGUI text = GetText((int)Enum.Parse(typeof(Texts), $"UI_Buttons_{betType}Money_Text"));

        text.text = betMoney.ToString("N0");
        text.gameObject.SetActive(isOn);

        if(betType == "Call")
        {
            if (betMoney == 0) GetText((int)Texts.UI_Buttons_Call_Text).text = "체크";
            else GetText((int)Texts.UI_Buttons_Call_Text).text = "콜";
        }
    }

    void IconFriendClicked(PointerEventData data)
    {
        Managers.UI.ShowPopupUI<UI_InviteFriendPopup>();
    }

    void BetButtonBind()            // interactable 체크 귀찮아서 onclick 이벤트로 추가함
    {
        Button bt = null;

        bt = GetButton((int)Buttons.UI_Buttons_Call);
        bt.onClick.AddListener(() => RequestBet("Call"));

        bt = GetButton((int)Buttons.UI_Buttons_Die);
        bt.onClick.AddListener(() => RequestBet("Die"));

        bt = GetButton((int)Buttons.UI_Buttons_Double);
        bt.onClick.AddListener(() => RequestBet("Double"));

        bt = GetButton((int)Buttons.UI_Buttons_Quater);
        bt.onClick.AddListener(() => RequestBet("Quater"));

        bt = GetButton((int)Buttons.UI_Buttons_Half);
        bt.onClick.AddListener(() => RequestBet("Half"));

        bt = GetButton((int)Buttons.UI_Buttons_AllIn);
        bt.onClick.AddListener(() => RequestBet("AllIn"));
    }

    void RequestBet(string betType)
    {
        if (_control.Players.GetPlayerTurn(User.NowGamePlayer.GameIndex) == false)
        {
            if (betType == "Die")       // 자신의 턴이 아니면 die만 켜져있어서 die만 누를테지만 혹시 모르니
            {
                if (_control.Players.GetPlayerDieReserve(User.NowGamePlayer.GameIndex) == false)
                    _control.Sync.SyncHoldemDieReserve(User.NowGamePlayer.GameIndex, true);
                else
                    _control.Sync.SyncHoldemDieReserve(User.NowGamePlayer.GameIndex, false);
            }
            return;     // 자신의 턴이 아닐때 die가 아니면 모두 리턴
        }

        _control.Bet.PlayerBetSelected(betType);
    }

    void OpenRoomClicked(PointerEventData data)
    {
        Managers.Photon.OpenRoomToPublic();
        SetRoomMove();
    }

    void MoveRoomClicked(PointerEventData data)
    {
        Define.Difficulty difficulty = Managers.CurrentDifficulty;
        Define.GameType gameType = Managers.CurrentGameType;
        Managers.Photon.JoinOtherGame(difficulty, gameType);
    }

    public GameObject GetPlayerGameObjcet(int index)
    {
        return GetImage((int)index).gameObject;
    }

    List<int> winnerIndex;
    public void SetWinnerPanel(bool isOn)
    {
        GameObject pl = GetGameObject((int)GameObjects.UI_TmpWinnerShow);
        pl.transform.localScale = Vector3.zero;
        pl.SetActive(isOn);
        BetButtonInteractiveSwitch("Die", false);

        if (!isOn)
            return;

        var panelText = GetText((int)Texts.UI_TmpWinnerShow_Text);

        panelText.text = "Winner!!\n\n";

        List<string> wList = _control.Players.GetWinnerList();
        winnerIndex.Clear();

        bool amIWin = false;
        for (int i = 0; i < wList.Count; i++)
        {
            panelText.text += _control.Players.GetPlayerNickNameByUID(wList[i]);

            if (wList[i] == User.NowUser.GetUid())
                amIWin = true;

            winnerIndex.Add(_control.Players.GetPlayerGameIndexByUID(wList[i]));

            if (i != wList.Count - 1)
            {
                panelText.text += "\n";
            }
        }

        if (amIWin) Managers.Audio.PlaySFX(Define.SFX.Win);
        else Managers.Audio.PlaySFX(Define.SFX.Lose);

        pl.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad);
        WinnerIconBigger();
        StartCoroutine(WaitWinnerPanel(HoldemGameControl.RESULT_SHOW_TIME));
    }

    void WinnerIconBigger()
    {
        for (int i = 0; i < winnerIndex.Count; i++)
        {
            int uiI = _control.ConvertGameToUI(winnerIndex[i]);
            GameObject go = GetImage((int)Enum.Parse(typeof(Images), $"UI_Player{uiI + 1}")).gameObject;
            go.transform.DOScale(Vector3.one * biggerIconSize, 1.0f).SetEase(Ease.InOutQuad);
        }
    }

    void WinnerIconOrigin()
    {
        for (int i = 0; i < winnerIndex.Count; i++)
        {
            int uiI = _control.ConvertGameToUI(winnerIndex[i]);
            GameObject go = GetImage((int)Enum.Parse(typeof(Images), $"UI_Player{uiI + 1}")).gameObject;
            go.transform.DOScale(Vector3.one * originIconSize, 0.3f);
        }
    }

    IEnumerator WaitWinnerPanel(float sec)
    {
        yield return new WaitForSeconds(sec);

        WinnerIconOrigin();
        SetWinnerPanel(false);
        _control.NextStage();
    }

    private void LeaveRoomClicked(PointerEventData data)
    {
        RoomLeave();
    }

    public void RoomLeave()
    {
        HoldemScene holdemScene = (HoldemScene)Managers.Scene.CurrentScene;
        holdemScene.RequestLeaveRoom();
    }
}
