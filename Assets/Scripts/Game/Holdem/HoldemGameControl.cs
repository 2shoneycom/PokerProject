using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;

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
            _betManager = new HoldemBetManager(_holdemUI);
            _holdemaPlayers = new HoldemPlayerManager(MAX_PLAYER_NUM);
            _cardManager = new HoldemCardManager();
        }
        else
        {
            Destroy(gameObject); // 씬 안에서 중복 생성 방지
        }
    }

    public const int MAX_PLAYER_NUM = 7;

    HoldemPlayerManager _holdemaPlayers;
    public static HoldemPlayerManager Players { get { return Control._holdemaPlayers; } }

    HoldemBetManager _betManager;
    public static HoldemBetManager Bet { get { return Control._betManager; } }

    HoldemCardManager _cardManager;
    public static HoldemCardManager Card { get { return Control._cardManager; } }


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
        isPlaying = true;
        _holdemUI.UISwitch(true);
        Card.Init();

        _betManager.CalBetAndButtonSwitch();
        PotMoney = 0;

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
        Debug.Log($"now stage {StageCount}, now detail {StageDetail}");
        switch(StageCount)
        {
            case 0:         // 자리 Setting
                SyncSystem.Instacne.SyncHoldemPlayerUID();
                break;

            case 1:         // 카드 Shuffle
                Card.ShuffleCard();
                SyncSystem.Instacne.SyncHoldemDeck();
                break;

            case 2:         // 딜러 선택
                DecideDealer();
                SyncSystem.Instacne.SyncHoldemDealerIndex(_playerD);
                break;

            case 3:         // 기본 베팅    sb -> bb 순으로
                _betManager.BaseBetting(_playerSB, _playerBB);
                int baseBetAmount = _betManager.GetBaseBetAmount(Define.Difficulty.Beginner, true) + _betManager.GetBaseBetAmount(Define.Difficulty.Beginner, false);
                SyncSystem.Instacne.SyncHoldemPotMoney(baseBetAmount);
                break;

            case 4:         // 첫번째 카드 배부    sb부터 받음
            case 5:         // 두번째 카드 배부    sb부터 받음
                    if (StageDetail >= MAX_PLAYER_NUM)
                    {
                        SyncSystem.Instacne.HoldemNextStage();
                        break;
                    }

                    int toPlayer = (_playerSB + StageDetail) % MAX_PLAYER_NUM;
                    StartCoroutine(Card.DealingCard(0, toPlayer));
                    break;

            case 6:         // 배팅 1     프리플랍 -> bb의 다음사람(언더더건)부터 시작 / 2인일 경우엔 딜러부터
                SyncSystem.Instacne.HoldemNextStage(); // test
                break;

            case 7:         // 오픈 카드 3
                if (StageDetail >= 3)
                {
                    StageDetail = 0;
                    SyncSystem.Instacne.HoldemNextStage();
                    break;
                }

                StartCoroutine(Card.DealingCard(1));
                break;

            case 8:         // 배팅 2     플랍 -> sb 부터 배팅 / 2인일 경우엔 bb부터
                SyncSystem.Instacne.HoldemNextStage(); // test
                break;

            case 9:         // 오픈 카드 1
                if (StageDetail >= 1)
                {
                    SyncSystem.Instacne.HoldemNextStage();
                    break;
                }

                StartCoroutine(Card.DealingCard(1));
                break;

            case 10:         // 배팅 3     턴 -> sb 부터 배팅 / 2인일 경우엔 bb부터
                SyncSystem.Instacne.HoldemNextStage(); // test
                break;

            case 11:         // 오픈 카드 1
                if (StageDetail >= 1)
                {
                    SyncSystem.Instacne.HoldemNextStage();
                    break;
                }

                StartCoroutine(Card.DealingCard(1));
                break;

            case 12:         // 배팅 4     리버 -> sb 부터 배팅 / 2인일 경우엔 bb부터
                break;

            case 13:         // 결과 발표
                break;

            case 14:         // 
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

}
