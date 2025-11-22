using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PokerGameControl : MonoBehaviour
{
    public const int MAX_PLAYER_NUM = 5;
    public const float RESULT_SHOW_TIME = 5.0f;

    PokerPlayerManager _playerManager;
    public PokerPlayerManager Players { get { return _playerManager; } }

    PokerBetManager _betManager;
    public PokerBetManager Bet { get { return _betManager; } }

    PokerCardManager _cardManager;
    public PokerCardManager Card { get { return _cardManager; } }

    PokerResultManager _resultManager;
    public PokerResultManager Result { get { return _resultManager; } }

    UI_Poker _pokerUI;
    UI_PokerCardPopup _cardPopup;

    SyncSystem _syncSystem;
    public SyncSystem Sync {  get { return _syncSystem; } }

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

    int _nowCardLen = 0;
    public int CardLen
    {
        get
        {
            switch (StageCount)
            {
                case 9:
                    return 4;
                case 12:
                    return 5;
                case 15:
                    return 6;
                case 18:
                    return 7;
                default:
                    return _nowCardLen;
            }
        }
    }

    private Coroutine dieTimer;

    int _curPlayer;
    int _potMoney;

    public int PotMoney
    {
        get { return _potMoney; }
        set
        {
            _potMoney = value;
            _pokerUI.UpdatePotMoney();
        }
    }

    void Start()
    {
        Debug.Log("Start2");
        _betManager = new PokerBetManager(this);
        _playerManager = new PokerPlayerManager(this);
        _cardManager = new PokerCardManager(this);
        _resultManager = new PokerResultManager(this);

        _pokerUI = (UI_Poker)Managers.UI.SceneUI;
    }

    public void StartGame()
    {
        if (IsPlaying)
            return;

        if (User.NowGamePlayer.SeatIndex == -1)
            return;

        isPlaying = true;

        _pokerUI.UISwitch(true);

        Players.GameSetting();
        Card.Init();
        Bet.Init(_pokerUI);

        User.NowUser.PokerSyncSeedMoney();
        PotMoney = 0;
        StageCount = 0;
        StageDetail = 0;
        _nowCardLen = 0;

        if (PhotonNetwork.IsMasterClient)
            ProcessStage();
    }

    public void NextStage(int state = 0)        // 1은 스테이지 세부 사항 카운트 증가
    {
        _pokerUI.ResetOnTurnPlayer();

        if (state == 0)
        {
            StageCount++;
            StageDetail = 0;
        }
        else if (state == 1)
        {
            _nowCardLen = 0;
            StageDetail++;
        }
        else
        {
            _nowCardLen++;
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
                StartCoroutine(Sync.SyncPokerPlayerUID());
                break;

            // 카드 Shuffle
            case 1:
                Card.ShuffleCard();

                StartCoroutine(Sync.SyncPokerDeck());
                break;

            // 첫번째 시작 플레이어 랜덤 선택
            case 2:
                DecideFirstPlayer();

                StartCoroutine(Sync.SyncPokerFirstPlayerIndex(_curPlayer));
                break;

            // 선택된 플레이어부터 기본금 배팅
            case 3:
                {
                    if (StageDetail >= MAX_PLAYER_NUM)
                    {
                        StartCoroutine(Sync.PokerNextStage());
                        break;
                    }

                    int toPlayer = (_curPlayer + StageDetail) % MAX_PLAYER_NUM;
                    string pUID = Players.GetPlayerUID(toPlayer);
                    if (pUID == "")
                    {
                        StartCoroutine(Sync.PokerNextStage(1));
                        break;
                    }

                    Bet.BaseBetting(toPlayer);
                    int baseBetAmount = Bet.GetBaseBetAmount(Managers.CurrentDifficulty);

                    StartCoroutine(Sync.SyncPokerPotMoney(PotMoney + baseBetAmount, 1));
                    break;
                }

            // 선택된 플레이어부터 4장씩 받기 (일단 비공개)
            case 4:
                {
                    if (StageDetail >= MAX_PLAYER_NUM)
                    {
                        StartCoroutine(Sync.PokerNextStage());
                        break;
                    }

                    int toPlayer = (_curPlayer + StageDetail) % MAX_PLAYER_NUM;
                    string pUID = Players.GetPlayerUID(toPlayer);
                    if (pUID == "")
                    {
                        StartCoroutine(Sync.PokerNextStage(1));
                        break;
                    }
                    if (CardLen >= 4)
                    {
                        StartCoroutine(Sync.PokerNextStage(1));
                        break;
                    }

                    StartCoroutine(Card.DealingCard(toPlayer));
                    break;
                }

            // 애니메이션 로딩
            case 5:
                StartCoroutine(AnimLoadingTime(2f));
                break;

            // 필요없는 1장, 공개할 카드 1장 선택
            case 6:
                StartCoroutine(Sync.PokerMakeCardSelPopup());
                break;

            // 전달받은 선택을 토대로 카드 정리
            case 7:
                StartCoroutine(Sync.PokerArrangeSelectedCard());
                break;

            //공개된 카드 중 가장 패가 낮은 플레이어 선택
            case 8:
                //////////////////////////////////// test
                {
                    string curPlayer = Result.GetWinner(3, true);
                    _curPlayer = Players.GetPlayerGameIndexByUID(curPlayer);
                    Debug.Log(Players.GetPlayerNickNameByUID(curPlayer));
                    StartCoroutine(Sync.SyncPokerCurrentPlayer(_curPlayer));
                }
                break;

            //선택된 플레이어부터 1장씩 받음 (공개)
            case 9:
            case 12:
            case 15:
            //선택된 플레이어부터 1장씩 받음 (비공개)
            case 18:
                {
                    // 타이머 끄기 (타이머는 monobehaviour 필요)
                    StartCoroutine(Sync.PokerAutoDieTimerSwitch(false));

                    if (StageDetail >= MAX_PLAYER_NUM)
                    {
                        StartCoroutine(Sync.PokerNextStage());
                        break;
                    }

                    int toPlayer = (_curPlayer + StageDetail) % MAX_PLAYER_NUM;
                    string pUID = Players.GetPlayerUID(toPlayer);
                    if (pUID == "")
                    {
                        StartCoroutine(Sync.PokerNextStage(1));
                        break;
                    }

                    StartCoroutine(Card.DealingCard(toPlayer));
                }
                break;

            //선택된 플레이어부터 베팅 시작 (4th 스트리트)
            case 11:
            //선택된 플레이어부터 베팅 시작 (5th 스트리트)
            case 14:
            //선택된 플레이어부터 베팅 시작 (6th 스트리트)
            case 17:
            //선택된 플레이어부터 마지막 베팅 시작 (7th 스트리트)
            case 19:
                // 타이머 끄기 (타이머는 monobehaviour 필요)
                StartCoroutine(Sync.PokerAutoDieTimerSwitch(false));

                // 배팅 시작 인원 정하기
                if (Bet.IsBetting == false)
                    Bet.CurBetPlayer = _curPlayer;

                Bet.CurBetPlayer = GetNextPlayerIndex(Bet.CurBetPlayer);

                // 타이머 키기
                StartCoroutine(Sync.PokerAutoDieTimerSwitch(true));

                StartCoroutine(Sync.PokerBetStart(Bet.CurBetPlayer));
                break;

            //공개된 카드 중 가장 패가 높은 플레이어 선택
            case 10:
            case 13:
            case 16:
                //////////////////////////////////// test
                {
                    int cardLen = 0;
                    if (StageCount == 10)
                        cardLen = 5;
                    if (StageCount == 13)
                        cardLen = 6;
                    if (StageCount == 16)
                        cardLen = 7;

                    string curPlayer = Result.GetWinner(cardLen);
                    _curPlayer = Players.GetPlayerGameIndexByUID(curPlayer);
                    StartCoroutine(Sync.SyncPokerCurrentPlayer(_curPlayer));
                }
                break;

            // 결과 발표
            case 20:
                // 타이머 끄기 (타이머는 monobehaviour 필요)
                StartCoroutine(Sync.PokerAutoDieTimerSwitch(false));

                StartCoroutine(EndGame());

                break;

            case 21:
                // UI 보여주기 & 플레이어 카드 공개
                StartCoroutine(Sync.SyncPokerResultUI());

                break;

            // 새로운 게임 준비
            case 22:
                StartCoroutine(Sync.PokerClearGame());
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

    void DecideFirstPlayer()
    {
        int ranNum = -1;
        do
        {
            ranNum = Random.Range(0, MAX_PLAYER_NUM);
        } while (Players.GetPlayerUID(ranNum) == "");
        _curPlayer = ranNum;
    }

    public void SetFirstPlayer(int index)
    {
        _curPlayer = index;

        NextStage();
    }

    public void SetCurrentPlayer(int index)
    {
        _curPlayer = index;
        NextStage();
    }

    public int ConvertUItoGame(int index)
    {
        switch (index)
        {
            case 2: return 4;
            case 3: return 2;
            case 4: return 3;
            default: return index;
        }
    }

    public int ConvertGameToUI(int index)
    {
        switch (index)
        {
            case 2: return 3;
            case 3: return 4;
            case 4: return 2;
            default: return index;
        }
    }

    IEnumerator AnimLoadingTime(float time)
    {
        yield return new WaitForSeconds(time);
        StartCoroutine(Sync.PokerNextStage());
    }

    public void CardSelPopupOn()
    {
        if (!IsPlaying)
            return;

        _cardPopup = Managers.UI.ShowPopupUI<UI_PokerCardPopup>();
        _cardPopup.SetControl(this);
    }

    public void SelectedCardIndex(int delCardIndex, int openCardIndex)
    {
        Sync.SyncPokerPlayerCardSel(User.NowGamePlayer.GameIndex, delCardIndex, openCardIndex);
        StartCoroutine(RPCLoadingTime(0.2f));
    }

    IEnumerator RPCLoadingTime(float time)
    {
        yield return new WaitForSeconds(time);
        _cardPopup.ClosePopupUI();
        NextStage();
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
            _pokerUI.TimerSwitch(isOn);
            dieTimer = StartCoroutine(Bet.AutoDieTimer(PokerBetManager.AUTO_DIE_TIMER));
        }
        else
        {
            if (dieTimer != null)
            {
                _pokerUI.TimerSwitch(isOn);
                StopCoroutine(dieTimer);
                Debug.Log("Time stop");
            }
        }
    }

    IEnumerator EndGame()
    {
        // 우승자 리스트 가져오기
        List<string> winnerList = new List<string>();
        winnerList.Add(Result.GetWinner(PokerCardManager.PLAYER_CARD_NUM));
        for (int i = 0; i < winnerList.Count; i++)
        {
            foreach (string winnerUID in winnerList)
            {
                // 우승자에게 돈주기
                Sync.IncreaseMoneyToTarget(winnerUID, PotMoney / winnerList.Count);

                Debug.Log("우승자: " + winnerUID);
            }
        }
        Sync.SyncPokerWinnerList(winnerList.ToArray());
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(Sync.PokerNextStage(0));
    }

    public void ShowResult()
    {
        // 플레이어 카드 보이기
        Players.ShowPlayerCard();
        // 팟머니 0으로
        PotMoney = 0;
        _pokerUI.SetWinnerPanel(true);
    }

    public void ClearGame()
    {
        StartCoroutine(loadingForGameEnd());
    }

    IEnumerator loadingForGameEnd()
    {
        isPlaying = false;

        // 자신 게임 관련 초기화 (사실 베팅금만 초기화)
        User.NowGamePlayer.ClearSetting();

        yield return new WaitForSeconds(0.3f);

        // 플레이어 카드 삭제
        Players.ClearGameSetting();

        _pokerUI.UpdateBetMoney();

        // 인원수 체크를 하고 2 이상이면 바로 시작
        if (Managers.Seat.GetOccupiedCount() >= 2 && PhotonNetwork.IsMasterClient)
        {
            Sync.PokerStartSync();
        }
        else
        {
            _pokerUI.UISwitch(false);
            _pokerUI.BetUISwitch(false);
        }
    }

    public void UpdatePlayerSeedMoneyUI()
    {
        _pokerUI.UpdateSeedMoney();
    }

    public void UpdatePlayerBetMoneyUI()
    {
        _pokerUI.UpdateBetMoney();
    }

    public void SetSyncSystem(SyncSystem syncSystem)
    {
        _syncSystem = syncSystem;
    }

    public SyncSystem GetSyncSystem()
    {
        return _syncSystem;
    }


    //public IEnumerator PlayerEnterHoldemRoom(float time, Player newPlayer)
    //{
    //    yield return new WaitForSeconds(time);

    //    if (IsPlaying)
    //    {
    //        GiveHoldemGameControlSyncData(newPlayer);
    //        Players.GiveHoldemPlayerManagerSyncData(newPlayer);
    //        Card.GiveHoldemCardManagerSyncData(newPlayer);
    //    }
    //}

    //void GiveHoldemGameControlSyncData(Player newPlayer)
    //{

    //}
}
