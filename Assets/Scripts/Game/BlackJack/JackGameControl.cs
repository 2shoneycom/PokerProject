using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class JackGameControl : MonoBehaviour
{
    private static JackGameControl instance;
    public static JackGameControl Control
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            _betManager = new JackBetManager();
            _playerManager = new JackPlayerManager();
            _cardManager = new JackCardManager();
            //_resultManager = new PokerResultManager();
        }
        else
        {
            Destroy(gameObject); // 씬 안에서 중복 생성 방지
        }
    }

    public const int MAX_PLAYER_NUM = 5;
    public const float RESULT_SHOW_TIME = 5.0f;
    public const float FIRST_BETTING_TIME = 15.0f;

    JackPlayerManager _playerManager;
    public static JackPlayerManager Players { get { return Control._playerManager; } }

    JackBetManager _betManager;
    public static JackBetManager Bet { get { return Control._betManager; } }

    JackCardManager _cardManager;
    public static JackCardManager Card { get { return Control._cardManager; } }

    //PokerResultManager _resultManager;
    //public static PokerResultManager Result { get { return Control._resultManager; } }

    UI_BlackJack _jackUI;

    bool isPlaying = false;
    public bool IsPlaying { get { return isPlaying; } }

    int _stageCount = 0;
    public int StageCount
    {
        get { return _stageCount; }
        set { _stageCount = value; }
    }

    int _stageDetail = 0;
    public int StageDetail
    {
        get { return _stageDetail; }
        set { _stageDetail = value; }
    }

    private Coroutine dieTimer;
    private Coroutine betTimer;

    int _curPlayer;


    void Start()
    {
        _jackUI = (UI_BlackJack)Managers.UI.SceneUI;
    }

    public void StartGame()
    {
        if (IsPlaying)
            return;

        if (User.NowGamePlayer.SeatIndex == -1)
            return;

        isPlaying = true;

        _jackUI.AllBlockSwitch(false);

        Players.GameSetting();
        Card.Init();
        Bet.Init(_jackUI);

        User.NowUser.JackSyncSeedMoney();
        StageCount = 0;
        StageDetail = 0;

        if (PhotonNetwork.IsMasterClient)
            ProcessStage();
    }

    public void NextStage(int state = 0)        // 1은 스테이지 세부 사항 카운트 증가
    {
        if (state == 0)
        {
            StageCount++;
            StageDetail = 0;
        }
        else
        {
            StageDetail++;
        }

        if (PhotonNetwork.IsMasterClient)
            ProcessStage();
    }

    public void ProcessStage()
    {
        switch (StageCount)
        {
            // 자리 Setting
            case 0:
                StartCoroutine(SyncSystem.Sync.SyncJackPlayerUID());
                break;

            // 카드 Shuffle
            case 1:
                Card.ShuffleCard();

                StartCoroutine(SyncSystem.Sync.SyncJackDeck());
                break;

            // 첫번째 시작 플레이어는 항상 오른쪽 끝
            case 2:
                DecideFirstPlayer();

                StartCoroutine(SyncSystem.Sync.SyncJackFirstPlayerIndex(_curPlayer));
                break;

            // 모두 다같이 한번에 기본 베팅
            case 3:
                StartCoroutine(SyncSystem.Sync.StartFirstBetting());
                break;

            // 가장 오른쪽 사람부터 카드 1장씩
            case 4:
            case 6:
                if (StageDetail >= MAX_PLAYER_NUM)
                {
                    StartCoroutine(SyncSystem.Sync.JackNextStage());
                    break;
                }

                int toPlayer = (_curPlayer + StageDetail) % MAX_PLAYER_NUM;
                string pUID = Players.GetPlayerUID(toPlayer);

                if (pUID == "")
                {
                    StartCoroutine(SyncSystem.Sync.JackNextStage(1));
                    break;
                }

                StartCoroutine(Card.DealingCard(toPlayer));
                break;

            // 딜러 카드 1장
            case 5:
            case 7:
                StartCoroutine(Card.DealingCard());
                break;

            // 플레이어 블랙잭 판별
            case 8:
                Players.FindPlayerBlackJack();
                break;

            // 딜러 카드 판별 -> 첫장 A / 첫장 10 / 그외
            case 9:

                break;

            default:
                break;
        }
    }


    void DecideFirstPlayer()
    {
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            if(Players.GetPlayerUID(i) != "")
            {
                _curPlayer = i;
                return;
            }
        }
    }

    public void SetFirstPlayer(int index)
    {
        _curPlayer = index;

        NextStage();
    }

    public void StartFirstBet()
    {
        _jackUI.FirstBetSetting();

        _jackUI.TimerSwitch(true);
        betTimer = StartCoroutine(FirstBetTimer(FIRST_BETTING_TIME));
    }

    IEnumerator FirstBetTimer(float time)
    {
        Debug.Log("Timer Start");
        while (time > 0)
        {
            time -= Time.deltaTime;
            _jackUI.SetTimerText(time);

            if (PhotonNetwork.IsMasterClient)
            {
                if (DetectAllPass())
                    SyncSystem.Sync.FirstBettingAllPass();
            }

            yield return null;
        }

        _jackUI.SetTimerText(0f);

        // 현재 플레이어가 n초 동안 베팅을 누르지 않았을 경우 자동 1 배팅 선택
        if (User.NowGamePlayer.BetMoney == 0)
            Bet.JackBetting(User.NowGamePlayer.GameIndex, 500);

        FirstBetAllPass();
    }

    public void FirstBetAllPass()
    {
        if(betTimer != null)
        {
            StopCoroutine(betTimer);
        }
        StartCoroutine(FirstBetEnd(1f));
    }

    IEnumerator FirstBetEnd(float time)
    {
        _jackUI.SetTimerText(0f);
        _jackUI.TimerSwitch(false);

        Debug.Log("Timer End");

        _jackUI.FirstBetEnd();

        yield return new WaitForSeconds(time);

        NextStage();
    }

    bool DetectAllPass()
    {
        for(int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            if (Players.GetPlayerUID(i) == "") continue;

            if (Players.GetPlayerIsBet(i) == false)
                return false;
        }
        return true;
    }

    public void UpdatePlayerSeedMoneyUI()
    {
        _jackUI.UpdateSeedMoney();
    }

    public void UpdatePlayerBetMoneyUI()
    {
        _jackUI.UpdateBetMoney();
    }

    public void UpdatePlayerBetScoreUI(int playerIndex)
    {
        string text = "";
        Tuple<int, int> score = Players.CalculatePlayerBetScore(playerIndex);

        text += score.Item1;
        if(score.Item2 != -1)
        {
            text += "/";
            text += score.Item2;
        }

        _jackUI.UpdatePlayerBetScoreText(playerIndex + 1, text);
    }

    public void UpdatePlayerBetStatusUI(int playerIndex, string text)
    {
        _jackUI.UpdatePlayerBetStatusText(playerIndex + 1, text);
    }
}
