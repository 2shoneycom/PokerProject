using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;

public class HoldemGameControl : MonoBehaviour
{
    public const int MAX_PLAYER_NUM = 7;
    public const float RESULT_SHOW_TIME = 5.0f;

    HoldemPlayerManager _playerManager;
    public HoldemPlayerManager Players { get { return _playerManager; } }

    HoldemBetManager _betManager;
    public HoldemBetManager Bet { get { return _betManager; } }

    HoldemCardManager _cardManager;
    public HoldemCardManager Card { get { return _cardManager; } }

    HoldemResultManager _resultManager;
    public HoldemResultManager Result { get { return _resultManager; } }

    UI_Holdem _holdemUI;
    SyncSystem _syncSystem;
    public SyncSystem Sync {  get { return _syncSystem; } }

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
        _betManager = new HoldemBetManager(this);
        _playerManager = new HoldemPlayerManager(this);
        _cardManager = new HoldemCardManager(this);
        _resultManager = new HoldemResultManager(this);

        _holdemUI = (UI_Holdem)Managers.UI.SceneUI;
    }

    public void StartGame()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemGameControl.cs 파일의 StartGame 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        if (isPlaying)
            return;

        if (User.NowGamePlayer.SeatIndex == -1)
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
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemGameControl.cs 파일의 NextStage 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemUI.ResetOnTurnPlayer();

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
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemGameControl.cs 파일의 ProcessStage 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)
        Debug.Log($"ProcessStage 함수의 StageCount: {StageCount}"); // 디버깅 추적용 (25.11.15 승헌)

        switch (StageCount)
        {
            // 자리 Setting
            case 0:
                StartCoroutine(_syncSystem.SyncHoldemPlayerUID());
                break;

            // 카드 Shuffle
            case 1:
                Card.ShuffleCard();

                StartCoroutine(_syncSystem.SyncHoldemDeck());
                break;

            // 딜러 선택
            case 2:
                DecideDealer();

                StartCoroutine(_syncSystem.SyncHoldemDealerIndex(_playerD));
                break;

            // 기본 베팅    sb
            case 3:
                {
                    Bet.BaseBetting(_playerSB, true);
                    int baseBetAmount = Bet.GetBaseBetAmount(Managers.CurrentDifficulty, true);

                    StartCoroutine(_syncSystem.SyncHoldemPotMoney(PotMoney + baseBetAmount, true));
                }
                break;

            // 기본 베팅    bb 순으로
            case 4:
                {
                    Bet.BaseBetting(_playerBB, false);
                    int baseBetAmount = Bet.GetBaseBetAmount(Managers.CurrentDifficulty, false);

                    StartCoroutine(_syncSystem.SyncHoldemPotMoney(PotMoney + baseBetAmount, true));
                }
                break;

            // 첫번째 카드 배부    sb부터 받음
            case 5:
            // 두번째 카드 배부    sb부터 받음
            case 6:
                if (StageDetail >= MAX_PLAYER_NUM)
                {
                    StartCoroutine(_syncSystem.HoldemNextStage());
                    break;
                }

                int toPlayer = (_playerSB + StageDetail) % MAX_PLAYER_NUM;
                string pUID = Players.GetPlayerUID(toPlayer);
                if(pUID == "")
                {
                    StartCoroutine(_syncSystem.HoldemNextStage(1));
                    break;
                }

                StartCoroutine(Card.DealingCard(0, toPlayer));
                break;

            // 배팅 1     프리플랍 -> bb의 다음사람(언더더건)부터 시작 / 2인일 경우엔 딜러부터  // 2인일때 무조건 bb다음이 딜러여서 따로 처리 필요 x
            case 7:
                // 타이머 끄기 (타이머는 monobehaviour 필요)
                StartCoroutine(_syncSystem.HoldemAutoDieTimerSwitch(false));

                // 어차피 베팅 끝날때 까지 계속 뺑글뺑글 돌텐데 저렇게 1씩 증가시키는게 의미 있나 해서 바꿔봄
                if (Bet.IsBetting == false)
                    Bet.CurBetPlayer = _playerBB;

                Bet.CurBetPlayer = GetNextPlayerIndex(Bet.CurBetPlayer);

                // 타이머 키기
                StartCoroutine(_syncSystem.HoldemAutoDieTimerSwitch(true));

                StartCoroutine(_syncSystem.HoldemBetStart(Bet.CurBetPlayer));
                break;

            // 오픈 카드 3
            case 8:
                // 타이머 끄기
                StartCoroutine(_syncSystem.HoldemAutoDieTimerSwitch(false));

                if (StageDetail >= 3)
                {
                    StartCoroutine(_syncSystem.HoldemNextStage());
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
                // 타이머 끄기 (타이머는 monobehaviour 필요)
                StartCoroutine(_syncSystem.HoldemAutoDieTimerSwitch(false));

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
                StartCoroutine(_syncSystem.HoldemAutoDieTimerSwitch(true));

                StartCoroutine(_syncSystem.HoldemBetStart(Bet.CurBetPlayer));
                break;

            // 오픈 카드 1
            case 10:
            case 12:
                // 타이머 끄기
                StartCoroutine(_syncSystem.HoldemAutoDieTimerSwitch(false));

                if (StageDetail >= 1)
                {
                    StartCoroutine(_syncSystem.HoldemNextStage());
                    break;
                }

                StartCoroutine(Card.DealingCard(1));
                break;

            // 결과 발표
            case 14:
                // 타이머 끄기
                StartCoroutine(_syncSystem.HoldemAutoDieTimerSwitch(false));

                StartCoroutine(EndGame());

                break;

            case 15:
                // UI 보여주기 & 플레이어 카드 공개
                StartCoroutine(_syncSystem.SyncHoldemResultUI());

                break;

            // 새로운 게임 준비
            case 16:
                StartCoroutine(_syncSystem.HoldemClearGame());

                break;
        }
    }

    int GetNextPlayerIndex(int index)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemGameControl.cs 파일의 GetNextPlayerIndex 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        do
        {
            index = (index + 1) % MAX_PLAYER_NUM;
        } while (Players.GetPlayerUID(index) == "");
        return index;
    }

    void DecideDealer()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemGameControl.cs 파일의 DecideDealer 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        if (isFirst)
        {
            int ranNum = -1;
            do
            {
                ranNum = Random.Range(0, MAX_PLAYER_NUM);
            } while (Players.GetPlayerUID(ranNum) == "");
            _playerD = ranNum;
            isFirst = false;
        }
        else
        {
            _playerD = GetNextPlayerIndex(_playerD);
        }
    }

    public void SetDealer(int index)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemGameControl.cs 파일의 SetDealer 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

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
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemGameControl.cs 파일의 AutoDieTimerSwitch 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

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
            _holdemUI.TimerSwitch(isOn);
            dieTimer = StartCoroutine(Bet.AutoDieTimer(HoldemBetManager.AUTO_DIE_TIMER));
        }
        else
        {
            if (dieTimer != null)
            {
                _holdemUI.TimerSwitch(isOn);
                StopCoroutine(dieTimer);
                Debug.Log("Time stop");
            }
        }
    }

    IEnumerator EndGame()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemGameControl.cs 파일의 EndGame 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        // 우승자 리스트 가져오기
        List<string> winnerList = Result.GetWinner();
        for (int i = 0; i < winnerList.Count; i++)
        {
            foreach (string winnerUID in winnerList)
            {
                // 우승자에게 돈주기
                _syncSystem.IncreaseMoneyToTarget(winnerUID, PotMoney / winnerList.Count);

                Debug.Log("우승자: " + winnerUID);
            }
        }
        //StartCoroutine(SyncSystem.Sync.SyncHoldemPotMoney(0));
        _syncSystem.SyncHoldemWinnerList(winnerList.ToArray());
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(_syncSystem.HoldemNextStage(0));
    }

    public void ShowResult()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemGameControl.cs 파일의 ShowResult 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        // 플레이어 카드 보이기
        Players.ShowPlayerCard();
        // 팟머니 0으로
        PotMoney = 0;
        _holdemUI.SetWinnerPanel(true);
    }

    public void ClearGame()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemGameControl.cs 파일의 ClearGame 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        StartCoroutine(loadingForGameEnd());
    }

    IEnumerator loadingForGameEnd()
    {
        isPlaying = false;

        // 자신 게임 관련 초기화 (사실 베팅금만 초기화)
        User.NowGamePlayer.ClearSetting();

        yield return new WaitForSeconds(0.3f);

        // 딜러 카드 삭제 및 관련 초기화
        Card.ClearDealerCard();

        // 플레이어 카드 삭제
        Players.ClearGameSetting();

        _holdemUI.UpdateBetMoney();

        // 인원수 체크를 하고 2 이상이면 바로 시작
        if (Managers.Seat.GetOccupiedCount() >= 2 && PhotonNetwork.IsMasterClient)
        {
            Sync.HoldemStartSync();
        }
        else
        {
            isFirst = true;
            _holdemUI.UISwitch(false);
            _holdemUI.BetUISwitch(false);
        }
    }

    public void UpdatePlayerSeedMoneyUI()
    {
        _holdemUI.UpdateSeedMoney();
    }

    public void UpdatePlayerBetMoneyUI()
    {
        _holdemUI.UpdateBetMoney();
    }

    public IEnumerator PlayerEnterHoldemRoom(float time, Player newPlayer)
    {
        yield return new WaitForSeconds(time);

        if (IsPlaying)
        {
            GiveHoldemGameControlSyncData(newPlayer);
            Players.GiveHoldemPlayerManagerSyncData(newPlayer);
            Card.GiveHoldemCardManagerSyncData(newPlayer);
        }
    }

    public void SetSyncSystem(SyncSystem syncSystem)
    {
        _syncSystem = syncSystem;
    }

    void GiveHoldemGameControlSyncData(Player newPlayer)
    {

    }
}
