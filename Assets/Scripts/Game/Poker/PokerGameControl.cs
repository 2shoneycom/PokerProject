using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PokerGameControl : MonoBehaviour
{
    private static PokerGameControl instance;
    public static PokerGameControl Control
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
            _betManager = new PokerBetManager();
            _playerManager = new PokerPlayerManager();
            _cardManager = new PokerCardManager();
            //_resultManager = new HoldemResultManager();
        }
        else
        {
            Destroy(gameObject); // 씬 안에서 중복 생성 방지
        }
    }

    public const int MAX_PLAYER_NUM = 5;
    public const float RESULT_SHOW_TIME = 5.0f;


    PokerPlayerManager _playerManager;
    public static PokerPlayerManager Players { get { return Control._playerManager; } }

    PokerBetManager _betManager;
    public static PokerBetManager Bet { get { return Control._betManager; } }

    PokerCardManager _cardManager;
    public static PokerCardManager Card { get { return Control._cardManager; } }

    //HoldemResultManager _resultManager;
    //public static HoldemResultManager Result { get { return Control._resultManager; } }


    UI_Poker _pokerUI;


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
        _holdemUI = (UI_Holdem)Managers.UI.SceneUI;
    }

    public void StartGame()
    {
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
            isFirst = false;
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
        User.NowGamePlayer.ClearSetting();

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

    void GiveHoldemGameControlSyncData(Player newPlayer)
    {

    }
}
