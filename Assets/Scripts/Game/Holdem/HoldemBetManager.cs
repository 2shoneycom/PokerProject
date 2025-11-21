using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Chat.UtilityScripts;
using Photon.Pun;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;

public class HoldemBetManager
{
    UI_Holdem _holdemUI;
    HoldemGameControl _control;

    public const float AUTO_DIE_TIMER = 10.0f;

    bool _isBetting = false;
    public bool IsBetting { get { return _isBetting; } }

    string[] BetType =
{
        "Die",
        "Call",
        "Double",
        "Quater",
        "Half",
        "AllIn"
    };

    public Dictionary<string, Tuple<bool, int>> CurBetMoney;

    public int AGM;
    public bool IsAnyoneAllIn;
    public bool IsBeforeAllIn;

    int curBetPlayer = 0;
    public int CurBetPlayer
    {
        get { return curBetPlayer; }
        set { curBetPlayer = value; }
    }

    public HoldemBetManager(HoldemGameControl control)
    {
        _isBetting = false;
        _control = control;
    }

    public void Init(UI_Holdem ui)
    {
        _isBetting = false;
        _holdemUI = ui;
        CurBetPlayer = 0;
        AGM = 0;
        IsAnyoneAllIn = false;
        IsBeforeAllIn = false;
        CurBetMoney = new Dictionary<string, Tuple<bool, int>>();
    }

    public void BaseBetting(int playerIndex, bool isSB)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemBetManager.cs 파일의 BaseBetting 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        string pUID = _control.Players.GetPlayerUID(playerIndex);
        int dAmount = GetBaseBetAmount(Managers.CurrentDifficulty, isSB);
        _control.Sync.HoldemBetMoneyToTarget(pUID, dAmount);

        _control.Sync.SyncHoldemMyBetting(playerIndex, dAmount);
        //SyncSystem.Sync.SyncHoldemPlayerIsBet(playerIndex, true);
    }

    //임의로 정한 값
    public int GetBaseBetAmount(Define.Difficulty diff, bool isSB)
    {
        int baseBet;

        switch (diff)
        {
            case Define.Difficulty.Beginner:
                baseBet = 500;
                break;
            case Define.Difficulty.Amateur:
                baseBet = 5000;
                break;
            case Define.Difficulty.Pro:
                baseBet = 50000;
                break;
            default:
                baseBet = 500; // 기본값 설정
                break;
        }

        if (isSB)
            return baseBet;
        else
            return baseBet * 2;
    }

    public void HandleBet(int curPlayer)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemBetManager.cs 파일의 HandleBet 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)
        Debug.Log($"HandleBet 함수의 curPlayer: {curPlayer}"); // 디버깅 추적용 (25.11.15 승헌)

        // 관전자는 리턴
        if (!_control.IsPlaying)
            return;

        CurBetPlayer = curPlayer;

        // 버튼 비활성화
        BetButtonDisable();

        // 배팅 시작 표시
        if (IsBetting == false)
        {
            CalAGM(true);
            _isBetting = true;
        }

        _holdemUI.SetOnTurnPlayer(_control.ConvertGameToUI(curPlayer) + 1);

        if (PhotonNetwork.IsMasterClient)
        {
            if (_control.Players.IsOneLeft)
            {
                Debug.Log("HandleBet(#1): 1명만 남았음");
                _control.Sync.HoldemBetEnd();
                return;
            }
            if (IsBetEnd())
            {
                Debug.Log("HandleBet(#2): 정상적 베팅 종료");
                _control.Sync.HoldemBetEnd();
                return;
            }
            // 이미 죽엇다면 처리
            if (_control.Players.GetPlayerState(CurBetPlayer) == false)
            {
                Debug.Log($"HandleBet(#3): {CurBetPlayer}번째 플레이어는 폴드함");
                _control.Sync.HoldemNextStage_V2(1);
                return;
            }
            // 예약 죽음햇다면 처리
            if (_control.Players.GetPlayerDieReserve(CurBetPlayer) == true)
            {
                Debug.Log($"HandleBet(#4): {CurBetPlayer}번째 플레이어는 예약 폴드");
                PlayerBetSelected("Die");
                return;
            }
        }

        if (_control.Players.GetPlayerUID(curPlayer) != User.NowUser.GetUid())
            return;

        _control.Sync.SyncHoldemIsTurn(CurBetPlayer, true);
        // 알맞은 버튼 키기
        CalBetAndButtonSwitch();
    }

    void CalAGM(bool isRoundStart = false)
    {
        int new_val = int.MaxValue;
        for (int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            if (_control.Players.GetPlayerState(i) == false || _control.Players.GetPlayerUID(i) == "")
                continue;

            int sm = _control.Players.GetPlayerSeedMoney(i) + _control.Players.GetPlayerBet(i);
            if (sm < new_val)
                new_val = sm;
        }
        
        if(AGM != new_val)
        {
            IsAnyoneAllIn = false;
        }
        AGM = new_val;

        if (isRoundStart == true && AGM == 0)
            IsBeforeAllIn = true;
    }

    void CalBetAndButtonSwitch()
    {
        CalBet();

        foreach (string bet in BetType)
        {
            if(bet == "Die")
            {
                _holdemUI.BetButtonInteractiveSwitch(bet, true);
            }
            else
            {
                _holdemUI.BetButtonInteractiveSwitch(bet, CurBetMoney[bet].Item1);
                _holdemUI.BetMoneyTextUpdate(bet, CurBetMoney[bet].Item2, CurBetMoney[bet].Item1);
            }
        }
    }

    void CalBet()
    {
        int highestBetMoney = _control.Players.FindHighestBet();
        int curPlayerBetMoney = _control.Players.GetPlayerBet(CurBetPlayer);

        int lowestSeedMoney = _control.Players.GetLowestPlayerSeedMoney();
        int curPlayerOriginMoney = _control.Players.GetOriginPlayerMoney(CurBetPlayer);

        if(IsAnyoneAllIn == true)
        {
            foreach (string bet in BetType)
            {
                if (bet == "Die") continue;

                switch (bet)
                {
                    case "Call":
                        CurBetMoney[bet] = Tuple.Create<bool, int>(true, highestBetMoney - curPlayerBetMoney);
                        break;

                    default:
                        CurBetMoney[bet] = Tuple.Create<bool, int>(false, 0);
                        break;
                }
            }
            return;
        }

        foreach (string bet in BetType)
        {
            int curBetAmount = highestBetMoney - curPlayerBetMoney;
            bool isOn = true;

            if (bet == "Die") continue;

            switch (bet)
            {
                case "Call":
                    isOn = highestBetMoney <= Math.Min(curPlayerOriginMoney, AGM);
                    break;

                case "Double":
                    isOn = Math.Max(GetBaseBetAmount(Managers.CurrentDifficulty, false), highestBetMoney * 2) <= Math.Min(curPlayerOriginMoney, AGM);
                    curBetAmount = Math.Max(GetBaseBetAmount(Managers.CurrentDifficulty, false), highestBetMoney * 2) - curPlayerBetMoney;
                    break;

                case "Quater":
                case "Half":
                    curBetAmount = curBetAmount + (_control.PotMoney + curBetAmount) / 4;
                    isOn = curBetAmount <= Math.Min(curPlayerOriginMoney, AGM);
                    curBetAmount -= curPlayerBetMoney;
                    break;

                case "AllIn":
                    curBetAmount = AGM - curPlayerBetMoney;
                    break;
            }
            CurBetMoney[bet] = Tuple.Create(isOn,curBetAmount);
        }
    }

    bool IsBetEnd()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemBetManager.cs 파일의 IsBetEnd 함수 실행"); // 디버깅 추적용 (25.11.12 승헌

        // 베팅 종료라고 판단되는 경우들

        if (_control.Players.NowPlayerNum - _control.Players.GetDeadPlayerNum() == 1)
        {
            _control.Sync.SyncHoldemIsOneLeft(true);
            return true;
        }

        if(IsBeforeAllIn== true)
        {
            return true;
        }

        if (_control.Players.GetPlayerIsBet(CurBetPlayer) &&
            _control.Players.GetPlayerBet(CurBetPlayer) == _control.Players.FindHighestBet())
        {
            Debug.Log($"highest bet money : {_control.Players.FindHighestBet()}");
            Debug.Log($"{curBetPlayer}의 bet money : {User.NowGamePlayer.BetMoney}");
            return true;
        }
        return false;
    }

    void BetButtonDisable()
    {
        for (int i = 0; i < BetType.Length; i++)
        {
            _holdemUI.BetButtonInteractiveSwitch(BetType[i], false);
        }
        _holdemUI.BetMoneyTextUpdate("", 0, false, true);

        if (_control.Players.GetPlayerState(User.NowGamePlayer.GameIndex))      // Die예약을 위해 죽지 않앗다면 die는 항상 활성화
            _holdemUI.BetButtonInteractiveSwitch("Die", true);
    }

    public IEnumerator AutoDieTimer(float duration)
    {
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            _holdemUI.SetTimerText(duration);
            yield return null;
        }

        duration = 0;
        _holdemUI.SetTimerText(duration);

        // 현재 플레이어가 n초 동안 버튼을 누르지 않았을 경우 Die 처리
        Debug.Log($"Player {curBetPlayer} didn't respond. Automatically choosing Die.");

        if (PhotonNetwork.IsMasterClient)
            PlayerBetSelected("Die");
    }

    public void BetProcess(int curPlayer, string betType, int betAmount)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemBetManager.cs 파일의 BetProcess 함수 실행"); // 디버깅 추적용 (25.11.12 승헌

        _control.Players.UpdatePlayerTurn(curPlayer, false);

        if (betType != "Die")
        {
            if (betType == "AllIn") IsAnyoneAllIn = true;

            _control.Players.UpdatePlayerBetting(curPlayer, betAmount);
            _control.PotMoney = _control.PotMoney + betAmount;
        }
        else
        {
            CalAGM();
            _control.Players.SetDeadPlayerNum(_control.Players.GetDeadPlayerNum() + 1);
            _control.Players.UpdatePlayerState(curPlayer, false);
        }
        _control.NextStage(1);
    }

    public void CurrentStageBetEnd()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemBetManager.cs 파일의 CurrentStageBetEnd 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _isBetting = false;
        _control.Players.ClearBetSetting();
        BetButtonDisable();
        _holdemUI.UpdateBetMoney();

        if(IsAnyoneAllIn == true)
            IsBeforeAllIn = true;

        // 어차피 이 함수는 모두가 호출하니
        _control.NextStage();
    }

    public void PlayerBetSelected(string betType)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemBetManager.cs 파일의 PlayerBetSelected 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        if (betType != "Die")
        {
            User.NowUser.HoldemBettingMoney(User.NowUser.GetUid(), CurBetMoney[betType].Item2);
        }
        _control.Sync.HoldemBetProcess(CurBetPlayer, betType, CurBetMoney[betType].Item2);
    }
}
