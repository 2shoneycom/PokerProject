using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;

public class HoldemGameControl : MonoBehaviour
{
    private static HoldemGameControl instance;
    public static HoldemGameControl Control
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
            _betManager = new HoldemBetManager();
            _holdemaPlayers = new HoldemPlayerManager();
            _cardManager = new HoldemCardManager();
            _resultManager = new HoldemResultManager();
        }
        else
        {
            Destroy(gameObject); // 씬 안에서 중복 생성 방지
        }
    }

    public const int MAX_PLAYER_NUM = 7;
    public const float RESULT_SHOW_TIME = 10.0f;

    HoldemPlayerManager _holdemaPlayers;
    public static HoldemPlayerManager Players { get { return Control._holdemaPlayers; } }

    HoldemBetManager _betManager;
    public static HoldemBetManager Bet { get { return Control._betManager; } }

    HoldemCardManager _cardManager;
    public static HoldemCardManager Card { get { return Control._cardManager; } }

    HoldemResultManager _resultManager;
    public static HoldemResultManager Result { get { return Control._resultManager; } }


    HoldemScene _scene;
    UI_Holdem _holdemUI;


    bool isPlaying = false;
    public bool IsPlaying { get { return isPlaying; } }

    int _stageCount = 0;
    public int StageCount { 
        get { return _stageCount; }
        set {  _stageCount = value; }
    }

    int _stageDetail = 0;
    public int StageDetail
    {
        get { return _stageDetail; }
        set { _stageDetail = value; }
    }

    private Coroutine dieTimer;

    int _playerD;
    int _playerSB;
    int _playerBB;
    int _potMoney;
    public int PotMoney {  
        get { return _potMoney; } 
        set {
            _potMoney = value;
            _holdemUI.UpdatePotMoney();
        }
    }

    void Start()
    {
        _scene = (HoldemScene)Managers.Scene.CurrentScene;
        _holdemUI = (UI_Holdem)Managers.UI.SceneUI;
    }

    public void StartGame()
    {
        if (User.NowHoldemPlayer.SeatIndex == -1)
            return;

        isPlaying = true;
        _holdemUI.UISwitch(true);

        Players.GameSetting();
        Card.Init();
        Bet.Init(_holdemUI);

        User.NowUser.HoldemSyncSeedMoney();
        PotMoney = 0;
        StageCount = 0;

        if(PhotonNetwork.IsMasterClient)
            ProcessStage();
    }

    public void NextStage(int state = 0)        // 1은 스테이지 세부 사항 카운트 증가
    {
        if(state == 0)
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

    void ProcessStage()
    {
        switch(StageCount)
        {
            // 자리 Setting
            case 0:
                SyncSystem.Instacne.SyncHoldemPlayerUID();
                SyncSystem.Instacne.HoldemNextStage();
                break;

            // 카드 Shuffle
            case 1:
                Card.ShuffleCard();

                SyncSystem.Instacne.SyncHoldemDeck();
                SyncSystem.Instacne.HoldemNextStage();
                break;

            // 딜러 선택
            case 2:
                DecideDealer();

                SyncSystem.Instacne.SyncHoldemDealerIndex(_playerD);
                SyncSystem.Instacne.HoldemNextStage();
                break;

            // 기본 베팅    sb -> bb 순으로
            case 3:
                _betManager.BaseBetting(_playerSB, _playerBB);
                int baseBetAmount = _betManager.GetBaseBetAmount(Define.Difficulty.Beginner, true) + _betManager.GetBaseBetAmount(Define.Difficulty.Beginner, false);

                SyncSystem.Instacne.SyncHoldemPotMoney(PotMoney + baseBetAmount);
                SyncSystem.Instacne.HoldemNextStage();
                break;

            // 첫번째 카드 배부    sb부터 받음
            case 4:
            // 두번째 카드 배부    sb부터 받음
            case 5:
                    if (StageDetail >= MAX_PLAYER_NUM)
                    {
                        SyncSystem.Instacne.HoldemNextStage();
                        break;
                    }

                    int toPlayer = (_playerSB + StageDetail) % MAX_PLAYER_NUM;
                    StartCoroutine(Card.DealingCard(0, toPlayer));
                    break;

            // 배팅 1     프리플랍 -> bb의 다음사람(언더더건)부터 시작 / 2인일 경우엔 딜러부터  // 2인일때 무조건 bb다음이 딜러여서 따로 처리 필요 x
            case 6:
                {
                    // 타이머 끄기 (타이머는 monobehaviour 필요)
                    SyncSystem.Instacne.HoldemAutoDieTimerSwitch(false);

                    // 배팅 처리 넘기기
                    int curBetPlayer = (_playerBB + StageDetail + 1) % MAX_PLAYER_NUM;
                    string pUID = Players.GetPlayerUID(curBetPlayer);

                    // 플레이어가 없는 자리면 넘어가기
                    if (pUID == "")
                        SyncSystem.Instacne.HoldemNextStage(1);
                    else
                        SyncSystem.Instacne.HoldemBetStart(curBetPlayer);

                    // 타이머 키기
                    SyncSystem.Instacne.HoldemAutoDieTimerSwitch(true);
                }
                break;

            // 오픈 카드 3
            case 7:
                 // 타이머 끄기
                SyncSystem.Instacne.HoldemAutoDieTimerSwitch(false);

                if (StageDetail >= 3)
                {
                    SyncSystem.Instacne.HoldemNextStage();
                    break;
                }

                StartCoroutine(Card.DealingCard(1));
                break;

            // 배팅 2     플랍 -> sb 부터 배팅 / 2인일 경우엔 bb부터
            case 8:
            // 배팅 3     턴 -> sb 부터 배팅 / 2인일 경우엔 bb부터
            case 10:
            // 배팅 4     리버 -> sb 부터 배팅 / 2인일 경우엔 bb부터
            case 12:
                {
                    // 타이머 끄기 (타이머는 monobehaviour 필요)
                    SyncSystem.Instacne.HoldemAutoDieTimerSwitch(false);

                    // 배팅 처리 넘기기
                    int curBetPlayer = (_playerSB + StageDetail) % MAX_PLAYER_NUM;
                    if(Players.NowPlayerNum == 2)
                        curBetPlayer = (_playerBB + StageDetail) % MAX_PLAYER_NUM;

                    string pUID = Players.GetPlayerUID(curBetPlayer);

                    // 플레이어가 없는 자리면 넘어가기
                    if (pUID == "")
                        SyncSystem.Instacne.HoldemNextStage(1);
                    else
                        SyncSystem.Instacne.HoldemBetStart(curBetPlayer);

                    // 타이머 키기
                    SyncSystem.Instacne.HoldemAutoDieTimerSwitch(true);
                }
                break;

            // 오픈 카드 1
            case 9:
                // 타이머 끄기
                SyncSystem.Instacne.HoldemAutoDieTimerSwitch(false);

                if (StageDetail >= 1)
                {
                    SyncSystem.Instacne.HoldemNextStage();
                    break;
                }

                StartCoroutine(Card.DealingCard(1));
                break;

            // 오픈 카드 1
            case 11:
                // 타이머 끄기
                SyncSystem.Instacne.HoldemAutoDieTimerSwitch(false);

                if (StageDetail >= 1)
                {
                    SyncSystem.Instacne.HoldemNextStage();
                    break;
                }

                StartCoroutine(Card.DealingCard(1));
                break;

            // 참가자에게 카드 정보 공유 요청
            case 13:
                // 타이머 끄기
                SyncSystem.Instacne.HoldemAutoDieTimerSwitch(false);

                SyncSystem.Instacne.RequestPlayerCardDetail();

                StartCoroutine(Util.LoadingTime(0.3f));         // 혹시 모르는 로딩 추가

                SyncSystem.Instacne.HoldemNextStage();
                break;

            // 결과 발표
            case 14:
                Debug.Log("Result Time!");

                EndGame();

                // UI 보여주기
                StartCoroutine(Util.LoadingTime(0.3f));         // 혹시 모르는 로딩 추가
                SyncSystem.Instacne.SyncHoldemResultUI(true);
                SyncSystem.Instacne.HoldemNextStage();
                // 상대방 카드도 보일수 있으면 보이기

                break;

            // 결과창 대기 타이머
            case 15:
                StartCoroutine(Util.LoadingTime(RESULT_SHOW_TIME));         // 아 결과창에서 나가면 바로 process 시작해서 무한로딩 가능해보이는데

                SyncSystem.Instacne.SyncHoldemResultUI(false);
                SyncSystem.Instacne.HoldemNextStage();
                break;

            // 새로운 게임 준비
            case 16:

                break;
        }
    }

    int GetNextPlayerIndex(int index)
    {
        do
        {
            index = (index + 1) % MAX_PLAYER_NUM;
        } while (Players.GetPlayerUID(index) == "");
        return index;
    }

    void DecideDealer()
    {
        int ranNum = -1;
        do
        {
            ranNum = Random.Range(0, MAX_PLAYER_NUM);
        } while (Players.GetPlayerUID(ranNum) == "");
        _playerD = ranNum;
    }

    public void SetDealer(int index)
    {
        _playerD = index;

        if (Players.NowPlayerNum == 2)
            _playerSB = _playerD;
        else
            _playerSB = GetNextPlayerIndex(_playerD);

        _playerBB = GetNextPlayerIndex(_playerSB);
    }

    public int ConvertUItoGame(int index)
    {
        switch (index)
        {
            case 2: return 6;
            case 3: return 2;
            case 4: return 5;
            case 5: return 3;
            case 6: return 4;
            default: return index;
        }
    }

    public int ConvertGameToUI(int index)
    {
        switch (index)
        {
            case 2: return 3;
            case 3: return 5;
            case 4: return 6;
            case 5: return 4;
            case 6: return 2;
            default: return index;
        }
    }

    public void AutoDieTimerSwitch(bool isOn)
    {
        if (!IsPlaying)
            return;

        if (isOn)
        {
            if(dieTimer != null)        // 왜인지 모르겟는데 타이머가 2번 작동함
            {
                StopCoroutine(dieTimer);
                Debug.Log("Duplicate Time Handle");
            }
            Debug.Log("Timer start");
            dieTimer = StartCoroutine(Bet.AutoDieTimer(HoldemBetManager.AUTO_DIE_TIMER));
        }
        else
        {
            if(dieTimer != null)
            {
                StopCoroutine(dieTimer);
                Debug.Log("Time stop");
            }
        }
    }

    void EndGame()
    {
        // 우승자 리스트 가져오기
        List<string> winnerList = Result.GetWinner();
        for (int i = 0; i < winnerList.Count; i++)
        {
            foreach (string winnerUID in winnerList)
            {
                // 우승자에게 돈주기
                SyncSystem.Instacne.IncreaseMoneyToTarget(winnerUID, PotMoney / winnerList.Count);

                Debug.Log("우승자: " + winnerUID);
            }
        }

        SyncSystem.Instacne.SyncHoldemWinnerList(winnerList.ToArray());
    }

    public void ShowResult(bool isOn)
    {
        _holdemUI.SetWinnerPanel(isOn);
    }
}
