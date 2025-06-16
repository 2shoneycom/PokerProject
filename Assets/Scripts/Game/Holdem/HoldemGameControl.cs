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
    public const float RESULT_SHOW_TIME = 5.0f;

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

    bool isFirst = true;

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

    int _playerD;
    int _playerSB;
    int _playerBB;
    int _potMoney;

    public int PotMoney
    {
        get { return _potMoney; }
        set
        {
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
        Debug.Log("Start");
        if (isPlaying)
            return;

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

    void ProcessStage()
    {
        switch (StageCount)
        {
            // 자리 Setting
            case 0:
                StartCoroutine(SyncSystem.Sync.SyncHoldemPlayerUID());
                break;

            // 카드 Shuffle
            case 1:
                Card.ShuffleCard();

                StartCoroutine(SyncSystem.Sync.SyncHoldemDeck());
                break;

            // 딜러 선택
            case 2:
                DecideDealer();

                StartCoroutine(SyncSystem.Sync.SyncHoldemDealerIndex(_playerD));
                break;

            // 기본 베팅    sb
            case 3:
                {
                    Bet.BaseBetting(_playerSB, true);
                    int baseBetAmount = Bet.GetBaseBetAmount(Managers.CurrentDifficulty, true);

                    StartCoroutine(SyncSystem.Sync.SyncHoldemPotMoney(PotMoney + baseBetAmount, true));
                }
                break;

            // 기본 베팅    bb 순으로
            case 4:
                {
                    Bet.BaseBetting(_playerBB, false);
                    int baseBetAmount = Bet.GetBaseBetAmount(Managers.CurrentDifficulty, false);

                    StartCoroutine(SyncSystem.Sync.SyncHoldemPotMoney(PotMoney + baseBetAmount, true));
                }
                break;

            // 첫번째 카드 배부    sb부터 받음
            case 5:
            // 두번째 카드 배부    sb부터 받음
            case 6:
                if (StageDetail >= MAX_PLAYER_NUM)
                {
                    StartCoroutine(SyncSystem.Sync.HoldemNextStage());
                    break;
                }

                int toPlayer = (_playerSB + StageDetail) % MAX_PLAYER_NUM;
                string pUID = Players.GetPlayerUID(toPlayer);
                if(pUID == "")
                {
                    StartCoroutine(SyncSystem.Sync.HoldemNextStage(1));
                    break;
                }

                StartCoroutine(Card.DealingCard(0, toPlayer));
                break;

            // 배팅 1     프리플랍 -> bb의 다음사람(언더더건)부터 시작 / 2인일 경우엔 딜러부터  // 2인일때 무조건 bb다음이 딜러여서 따로 처리 필요 x
            case 7:
                Debug.Log("첫 배팅( 프리 플랍)");
                // 타이머 끄기 (타이머는 monobehaviour 필요)
                StartCoroutine(SyncSystem.Sync.HoldemAutoDieTimerSwitch(false));

                // 어차피 베팅 끝날때 까지 계속 뺑글뺑글 돌텐데 저렇게 1씩 증가시키는게 의미 있나 해서 바꿔봄
                if (Bet.IsBetting == false)
                    Bet.CurBetPlayer = _playerBB;

                Bet.CurBetPlayer = GetNextPlayerIndex(Bet.CurBetPlayer);

                // 타이머 키기
                StartCoroutine(SyncSystem.Sync.HoldemAutoDieTimerSwitch(true));

                StartCoroutine(SyncSystem.Sync.HoldemBetStart(Bet.CurBetPlayer));
                break;

            // 오픈 카드 3
            case 8:
                // 타이머 끄기
                StartCoroutine(SyncSystem.Sync.HoldemAutoDieTimerSwitch(false));

                if (StageDetail >= 3)
                {
                    StartCoroutine(SyncSystem.Sync.HoldemNextStage());
                    break;
                }

                StartCoroutine(Card.DealingCard(1));
                break;

            // 배팅 2     플랍 -> sb 부터 배팅 / 2인일 경우엔 bb부터
            case 9:
            // 배팅 3     턴 -> sb 부터 배팅 / 2인일 경우엔 bb부터
            case 11:
            // 배팅 4     리버 -> sb 부터 배팅 / 2인일 경우엔 bb부터
            case 13:
                Debug.Log($"다른 배팅  {StageCount},  {StageDetail}");
                // 타이머 끄기 (타이머는 monobehaviour 필요)
                StartCoroutine(SyncSystem.Sync.HoldemAutoDieTimerSwitch(false));

                if (Bet.IsBetting == false)
                {
                    if (Players.NowPlayerNum == 2)
                        Bet.CurBetPlayer = _playerBB;
                    else
                        Bet.CurBetPlayer = _playerSB;
                }
                else
                {
                    Bet.CurBetPlayer = GetNextPlayerIndex(Bet.CurBetPlayer);
                }

                // 타이머 키기
                StartCoroutine(SyncSystem.Sync.HoldemAutoDieTimerSwitch(true));

                StartCoroutine(SyncSystem.Sync.HoldemBetStart(Bet.CurBetPlayer));
                break;

            // 오픈 카드 1
            case 10:
            case 12:
                // 타이머 끄기
                StartCoroutine(SyncSystem.Sync.HoldemAutoDieTimerSwitch(false));

                if (StageDetail >= 1)
                {
                    StartCoroutine(SyncSystem.Sync.HoldemNextStage());
                    break;
                }

                StartCoroutine(Card.DealingCard(1));
                break;

            // 결과 발표
            case 14:
                // 타이머 끄기
                StartCoroutine(SyncSystem.Sync.HoldemAutoDieTimerSwitch(false));

                EndGame();

                // UI 보여주기 & 플레이어 카드 공개
                StartCoroutine(SyncSystem.Sync.SyncHoldemResultUI());

                break;

            // 새로운 게임 준비
            case 15:
                StartCoroutine(SyncSystem.Sync.HoldemClearGame());

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
        if (isFirst)
        {
            int ranNum = -1;
            do
            {
                ranNum = Random.Range(0, MAX_PLAYER_NUM);
            } while (Players.GetPlayerUID(ranNum) == "");
            _playerD = ranNum;
        }
        else
        {
            _playerD = GetNextPlayerIndex(_playerD);
        }
    }

    public void SetDealer(int index)
    {
        _playerD = index;

        if (Players.NowPlayerNum == 2)
            _playerSB = _playerD;
        else
            _playerSB = GetNextPlayerIndex(_playerD);

        _playerBB = GetNextPlayerIndex(_playerSB);

        NextStage();
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
            if (dieTimer != null)        // 왜인지 모르겟는데 타이머가 2번 작동함
            {
                StopCoroutine(dieTimer);
                Debug.Log("Duplicate Time Handle");
            }
            Debug.Log("Timer start");
            dieTimer = StartCoroutine(Bet.AutoDieTimer(HoldemBetManager.AUTO_DIE_TIMER));
        }
        else
        {
            if (dieTimer != null)
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
                SyncSystem.Sync.IncreaseMoneyToTarget(winnerUID, PotMoney / winnerList.Count);

                Debug.Log("우승자: " + winnerUID);
            }
        }
        // 팟머니 0으로
        StartCoroutine(SyncSystem.Sync.SyncHoldemPotMoney(0));
        SyncSystem.Sync.SyncHoldemWinnerList(winnerList.ToArray());
    }

    public void ShowResult()
    {
        // 플레이어 카드 보이기
        Players.ShowPlayerCard();

        _holdemUI.SetWinnerPanel(true);
    }

    public void ClearGame()
    {
        isPlaying = false;

        // 자신 게임 관련 초기화 (사실 베팅금만 초기화)
        User.NowHoldemPlayer.ClearSetting();

        // 딜러 카드 삭제 및 관련 초기화
        Card.ClearDealerCard();

        // 플레이어 카드 삭제
        Players.ClearGameSetting();

        _holdemUI.UpdateBetMoney();

        // 인원수 체크를 하고 2 이상이면 바로 시작
        if (Managers.Seat.GetOccupiedCount() >= 2)
        {
            StartGame();
        }
        else
        {
            isFirst = true;
            _holdemUI.UISwitch(false);
            _holdemUI.BetUISwitch(false);
        }
    }

    public void Request_SyncHoldemPotMoney(int amount, bool isNextStage = false)     // 코루틴 호출시키기 위한 함수
    {
        StartCoroutine(SyncSystem.Sync.SyncHoldemPotMoney(PotMoney + amount, isNextStage));
    }

    public void UpdatePlayerBetMoneyUI()
    {
        _holdemUI.UpdateBetMoney();
    }
}
