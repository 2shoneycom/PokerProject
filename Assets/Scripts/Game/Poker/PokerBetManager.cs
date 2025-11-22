using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerBetManager
{
    UI_Poker _pokerUI;
    PokerGameControl _control;

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
    public const float AUTO_DIE_TIMER = 10.0f;

    public PokerBetManager(PokerGameControl control)
    {
        _isBetting = false;
        _control = control;
        CurBetMoney = new Dictionary<string, Tuple<bool, int>>();
    }

    public void Init(UI_Poker ui)
    {
        _isBetting = false;
        _pokerUI = ui;
        CurBetPlayer = 0;
        AGM = 0;
        IsAnyoneAllIn = false;
        IsBeforeAllIn = false;
        CurBetMoney.Clear();
    }

    public void BaseBetting(int playerIndex)
    {
        string pUID = _control.Players.GetPlayerUID(playerIndex);
        int dAmount = GetBaseBetAmount(Managers.CurrentDifficulty);
        _control.Sync.PokerBetMoneyToTarget(pUID, dAmount);

        _control.Sync.SyncPokerMyBetting(playerIndex, dAmount);
    }

    public int GetBaseBetAmount(Define.Difficulty diff)
    {
        int baseBet;

        switch (diff)
        {
            case Define.Difficulty.Beginner:
                baseBet = 1000;
                break;
            case Define.Difficulty.Amateur:
                baseBet = 10000;
                break;
            case Define.Difficulty.Pro:
                baseBet = 100000;
                break;
            default:
                baseBet = 1000; // 기본값 설정
                break;
        }
        return baseBet;
    }

    public void HandleBet(int curPlayer)
    {
        Debug.Log("Poker Handel Bet 11111111111111111111111111111111111111");
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
        Debug.Log("Poker Handel Bet 22222222222222222222222222222222222222");

        _pokerUI.SetOnTurnPlayer(_control.ConvertGameToUI(curPlayer) + 1);

        if (PhotonNetwork.IsMasterClient)
        {
            if (_control.Players.IsOneLeft || IsBetEnd())
            {
                // 1명 남앗거나 정상 배팅 종료의 경우
                Debug.Log("bet end in IsBetEnd");
                _control.Sync.PokerBetEnd();
                return;
            }

            // 내가 이미 죽엇다면 처리
            if (_control.Players.GetPlayerState(CurBetPlayer) == false)
            {
                _control.Sync.PokerNextStage_V2(1);
                return;
            }

            // 내가 예약 죽음햇다면 처리
            if (_control.Players.GetPlayerDieReserve(CurBetPlayer) == true)
            {
                PlayerBetSelected("Die");
                return;
            }
        }
        Debug.Log("Poker Handel Bet 33333333333333333333333333333333333333333");

        if (_control.Players.GetPlayerUID(curPlayer) != User.NowUser.GetUid())
            return;
        Debug.Log("Poker Handel Bet 4444444444444444444444444444444444444");

        _control.Sync.SyncPokerIsTurn(CurBetPlayer, true);
        // 알맞은 버튼 키기
        CalBetAndButtonSwitch();
    }

    void CalAGM(bool isRoundStart = false)
    {
        int new_val = int.MaxValue;
        for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
        {
            if (_control.Players.GetPlayerState(i) == false || _control.Players.GetPlayerUID(i) == "")
                continue;

            int sm = _control.Players.GetPlayerSeedMoney(i) + _control.Players.GetPlayerBet(i);
            if (sm < new_val)
                new_val = sm;
        }

        if (AGM != new_val)
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
            if (bet == "Die")
            {
                _pokerUI.BetButtonInteractiveSwitch(bet, true);
            }
            else
            {
                _pokerUI.BetButtonInteractiveSwitch(bet, CurBetMoney[bet].Item1);
                _pokerUI.BetMoneyTextUpdate(bet, CurBetMoney[bet].Item2, CurBetMoney[bet].Item1);
            }
        }
    }

    void CalBet()
    {
        int highestBetMoney = _control.Players.FindHighestBet();
        int curPlayerBetMoney = _control.Players.GetPlayerBet(CurBetPlayer);

        int lowestSeedMoney = _control.Players.GetLowestPlayerSeedMoney();
        int curPlayerOriginMoney = _control.Players.GetOriginPlayerMoney(CurBetPlayer);

        if (IsAnyoneAllIn == true)
        {
            foreach (string bet in BetType)
            {
                if (bet == "Die")
                {
                    CurBetMoney[bet] = Tuple.Create(true, 0);
                }
                else
                {
                    switch (bet)
                    {
                        case "Call":
                            CurBetMoney[bet] = Tuple.Create(true, highestBetMoney - curPlayerBetMoney);
                            break;

                        default:
                            CurBetMoney[bet] = Tuple.Create(false, 0);
                            break;
                    }
                }
            }
            return;
        }

        foreach (string bet in BetType)
        {
            int curBetAmount = highestBetMoney - curPlayerBetMoney;
            bool isOn = true;

            switch (bet)
            {
                case "Die":
                    isOn = true;
                    curBetAmount = 0;
                    break;

                case "Call":
                    isOn = highestBetMoney <= Math.Min(curPlayerOriginMoney, AGM);
                    break;

                case "Double":
                    isOn = Math.Max(GetBaseBetAmount(Managers.CurrentDifficulty), highestBetMoney * 2) <= Math.Min(curPlayerOriginMoney, AGM);
                    curBetAmount = Math.Max(GetBaseBetAmount(Managers.CurrentDifficulty), highestBetMoney * 2) - curPlayerBetMoney;
                    break;

                case "Quater":
                    curBetAmount = curBetAmount + (_control.PotMoney + curBetAmount) / 4;
                    isOn = curBetAmount <= Math.Min(curPlayerOriginMoney, AGM);
                    curBetAmount -= curPlayerBetMoney;
                    break;

                case "Half":
                    curBetAmount = curBetAmount + (_control.PotMoney + curBetAmount) / 2;
                    isOn = curBetAmount <= Math.Min(curPlayerOriginMoney, AGM);
                    curBetAmount -= curPlayerBetMoney;
                    break;

                case "AllIn":
                    curBetAmount = AGM - curPlayerBetMoney;
                    break;
            }
            CurBetMoney[bet] = Tuple.Create(isOn, curBetAmount);
        }
    }

    bool IsBetEnd()
    {
        if (_control.Players.NowPlayerNum - _control.Players.GetDeadPlayerNum() == 1)
        {
            _control.Sync.SyncPokerIsOneLeft(true);
            return true;
        }

        if (IsBeforeAllIn == true)
        {
            return true;
        }

        if (_control.Players.GetPlayerIsBet(CurBetPlayer) == true &&
            _control.Players.GetPlayerBet(CurBetPlayer) == _control.Players.FindHighestBet())
        {
            return true;
        }

        return false;
    }

    void BetButtonDisable()
    {
        for (int i = 0; i < BetType.Length; i++)
        {
            _pokerUI.BetButtonInteractiveSwitch(BetType[i], false);
        }
        _pokerUI.BetMoneyTextUpdate("", 0, false, true);

        if (_control.Players.GetPlayerState(User.NowGamePlayer.GameIndex))      // Die예약을 위해 죽지 않앗다면 die는 항상 활성화
            _pokerUI.BetButtonInteractiveSwitch("Die", true);
    }

    public IEnumerator AutoDieTimer(float duration)
    {
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            _pokerUI.SetTimerText(duration);
            yield return null;
        }

        duration = 0;
        _pokerUI.SetTimerText(duration);

        // 현재 플레이어가 n초 동안 버튼을 누르지 않았을 경우 Die 처리
        Debug.Log($"Player {curBetPlayer} didn't respond. Automatically choosing Die.");

        if (PhotonNetwork.IsMasterClient)
            PlayerBetSelected("Die");
    }

    public void BetProcess(int curPlayer, string betType, int betAmount)
    {
        _control.Players.UpdatePlayerTurn(curPlayer, false);

        if (betType != "Die")
        {
            if (betType == "AllIn") IsAnyoneAllIn = true;

            _control.Players.UpdatePlayerBetting(curPlayer, betAmount, true);
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
        _isBetting = false;
        _control.Players.ClearBetSetting();
        BetButtonDisable();
        _pokerUI.UpdateBetMoney();

        if (IsAnyoneAllIn == true)
            IsBeforeAllIn = true;

        // 어차피 이 함수는 모두가 호출하니
        _control.NextStage();
    }

    public void PlayerBetSelected(string betType)
    {
        if (betType != "Die")
        {
            User.NowUser.PokerBettingMoney(User.NowUser.GetUid(), CurBetMoney[betType].Item2);
        }
        _control.Sync.PokerBetProcess(CurBetPlayer, betType, CurBetMoney[betType].Item2);
    }
}
