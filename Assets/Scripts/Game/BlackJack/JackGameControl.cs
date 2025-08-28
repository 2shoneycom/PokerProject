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
    public const float INSURANCE_SEL_TIME = 10.2f;

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
    private Coroutine insuranceTimer;

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
                StartCoroutine(SyncSystem.Sync.JackNoticeBlackJack());
                break;

            // 딜러 카드 판별 -> 첫장 A / 첫장 10 / 그외
            case 9:
                StartCoroutine(SyncSystem.Sync.JackIsDealerIsA());
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

    public void RequestDeckShuffle()
    {
        StartCoroutine(SyncSystem.Sync.SyncJackDeck());
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
                if (DetectBettingAllPass())
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

    bool DetectBettingAllPass()
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
        Debug.Log("UpdatePlayerBetScoreUI");
        string text = "";
        Tuple<int, int> score = Players.CalculatePlayerBetScore(playerIndex);
        Debug.Log("UpdatePlayerBetScoreUI Y");

        text += score.Item1;
        if(score.Item2 != -1)
        {
            text += "/";
            text += score.Item2;
        }
        Debug.Log("UpdatePlayerBetScoreText");

        _jackUI.UpdatePlayerBetScoreText(playerIndex + 1, text);
        Debug.Log("UpdatePlayerBetScoreText Y");

    }

    public void UpdatePlayerBetStatusUI(int playerIndex, string text)
    {
        _jackUI.UpdatePlayerBetStatusText(playerIndex + 1, text);
    }

    public void JudgeDealerIsAOrAbove10()
    {
        if (!IsPlaying) return;

        int dealerFirstCardNum = Card.GetDealerCardDetail(0);
        dealerFirstCardNum = Card.GetCardNum(dealerFirstCardNum);

        if(dealerFirstCardNum == 1)     // 블랙잭인 플레이어는 이븐머니 / 일반 플레이어는 인슈어런스, 이후 딜러가 카드 확인
        {
            SetInsurance();
        }
        else if (dealerFirstCardNum >= 10)      // 딜러가 나머지 카드 확인
        {
            DealerSecondCardCheck();
        }
        else     // 정상 진행
        {
            NextStage();
        }
    }

    void SetInsurance()
    {
        UI_JackInsurancePopup _popup = Managers.UI.ShowPopupUI<UI_JackInsurancePopup>();

        var score = Players.GetPlayerCardScore(User.NowGamePlayer.GameIndex);
        if (score.Item1 == 21 || score.Item2 == 21)
        {
            _popup.SetBool(true);
        }

        _jackUI.TimerSwitch(true);
        insuranceTimer = StartCoroutine(InsuranceSelTimer(INSURANCE_SEL_TIME));
    }

    IEnumerator InsuranceSelTimer(float time)
    {
        Debug.Log("Timer Start");
        while (time > 0.2f)
        {
            time -= Time.deltaTime;
            _jackUI.SetTimerText(time - 0.2f);

            if (PhotonNetwork.IsMasterClient)
            {
                if (DetectInsuranceAllPass())
                    SyncSystem.Sync.JackInsuranceAllPass();
            }
            yield return null;
        }

        _jackUI.SetTimerText(0f);

        // 현재 플레이어가 n초 동안 카드를 누르지 않았을 경우 'No' 선택
        if (Players.GetPlayerIsInsurance(User.NowGamePlayer.GameIndex) == 0)
            SyncSystem.Sync.SyncJackIsInsurance(User.NowGamePlayer.GameIndex, -1);

        Debug.Log("Timer End");
        yield return new WaitForSeconds(time);
        InsuranceAllPass();
    }

    bool DetectInsuranceAllPass()
    {
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            if (Players.GetPlayerUID(i) == "") continue;

            if (Players.GetPlayerIsInsurance(i) == 0)
                return false;
        }
        return true;
    }

    public void InsuranceAllPass()
    {
        if (insuranceTimer != null)
        {
            StopCoroutine(insuranceTimer);
        }
        _jackUI.SetTimerText(0f);
        _jackUI.TimerSwitch(false);

        DealerSecondCardCheck();
    }

    void DealerSecondCardCheck()
    {
        if (!IsPlaying) return;

        //////////////////////////////// 딜러가 카드를 확인 애니메이션
        ///
        Debug.Log("DealerSecondCardCheck multi call?");
        var score = Card.GetDealerCardScore();
        if(score.Item1 == 21 || score.Item2 == 21)
        {
            _jackUI.UpdateDealerStatusText("블랙잭입니다!");

            // 이후 추가 행동
            DealerBlackJack();
        }
        else
        {
            _jackUI.UpdateDealerStatusText("블랙잭이 아닙니다!");
        }
        NextStage();
    }

    void DealerBlackJack()
    {
        if (!IsPlaying) return;
        Debug.Log("DealerBlackJack multi call?");
        var score = Players.GetPlayerCardScore(User.NowGamePlayer.GameIndex);
        if (score.Item1 == 21 || score.Item2 == 21)
        {
            if(Players.GetPlayerIsInsurance(User.NowGamePlayer.GameIndex) == 1)
            {
                // 플레이어 2배로 회수
                PlayerEvenMoney();
            }
            else
            {
                // 플레이어 원금 회수
                PlayerPush();
            }
        }
        else
        {
            if (Players.GetPlayerIsInsurance(User.NowGamePlayer.GameIndex) == 1)
            {
                // 플레이어 인슈어런스 (원금 회수)
                PlayerInsurance();
            }
            else
            {
                // 플레이어 패배
                PlayerLose();
            }
        }

    }

    void PlayerBlackJack()
    {
        MoneySetting((int)(User.NowGamePlayer.BetMoney * 2.5));
    }

    void PlayerEvenMoney()
    {
        MoneySetting(User.NowGamePlayer.BetMoney * 2);

    }

    void PlayerInsurance()
    {
        MoneySetting(User.NowGamePlayer.BetMoney);
    }

    void PlayerPush()
    {
        MoneySetting(User.NowGamePlayer.BetMoney);
    }

    void PlayerLose()
    {
        MoneySetting(0);
    }

    void MoneySetting(int amount)
    {
        Debug.Log("MoneySetting multi call?");
        User.NowUser.IncreaseMoney(User.NowUser.GetUid(), amount);
        SyncSystem.Sync.SyncJackMyBettingReset(User.NowGamePlayer.GameIndex);

        /////////
        /// UI 처리
        /////////
        
        SyncSystem.Sync.SyncJackIsGameEnd(User.NowGamePlayer.GameIndex, true);
    }

    public void DetectGameEndAllPass()
    {
        if (!IsPlaying) return;

        bool isPass = true;
        for(int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            if (Players.GetPlayerUID(i) == "") continue;

            if (Players.GetPlayerIsGameEnd(i) == false)
                isPass = false;
        }

        if (isPass) Debug.Log("DetectGameEndAllPass true");
        else Debug.Log("DetectGameEndAllPass false");

        if (!isPass) return;

        SyncSystem.Sync.JackGameEnd();
    }

    public void ClearGame()
    {
        StartCoroutine(GameEnd(RESULT_SHOW_TIME));
    }

    public IEnumerator GameEnd(float time)
    {
        yield return new WaitForSeconds(time);

        isPlaying = false; 

        // 자신 게임 관련 초기화 (사실 베팅금만 초기화)
        User.NowGamePlayer.ClearSetting();

        // 플레이어 카드 삭제
        Players.ClearGameSetting();

        // 딜러 카드 삭제
        Card.ClearDealerCard();

        // 블랙잭은 항상 게임시작
        StartGame();
    }
}
