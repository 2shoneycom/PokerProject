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

    int curBetPlayer = 0;
    public int CurBetPlayer
    {
        get { return curBetPlayer; }
        set {  curBetPlayer = value; }
    }
    public const float AUTO_DIE_TIMER = 10.0f;

    public HoldemBetManager()
    {
        _isBetting = false;
    }

    public void Init(UI_Holdem ui)
    {
        _isBetting = false;
        _holdemUI = ui;
        CurBetPlayer = 0;
    }

    public void BaseBetting(int playerIndex, bool isSB)
    {
        string pUID = HoldemGameControl.Players.GetPlayerUID(playerIndex);
        int dAmount = GetBaseBetAmount(Managers.CurrentDifficulty, isSB);
        SyncSystem.Sync.HoldemBetMoneyToTarget(pUID, dAmount);

        SyncSystem.Sync.SyncHoldemMyBetting(playerIndex, dAmount);
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
        // 관전자는 리턴
        if (!HoldemGameControl.Control.IsPlaying)
            return;

        CurBetPlayer = curPlayer;

        // 버튼 비활성화
        BetButtonDisable();

        // 배팅 시작 표시
        if (IsBetting == false)
        {
            _isBetting = true;
        }

        _holdemUI.SetOnTurnPlayer(HoldemGameControl.Control.ConvertGameToUI(curPlayer) + 1);

        if (HoldemGameControl.Players.GetPlayerUID(curPlayer) != User.NowUser.GetUid())
            return;

        if (HoldemGameControl.Players.IsOneLeft || IsBetEnd())
        {
            // 1명 남앗거나 정상 배팅 종료의 경우
            Debug.Log("bet end in IsBetEnd");
            SyncSystem.Sync.HoldemBetEnd();
            return;
        }

        // 내가 이미 죽엇다면 처리
        if (HoldemGameControl.Players.GetPlayerState(CurBetPlayer) == false)
        {
            SyncSystem.Sync.HoldemNextStage_V2(1);
            return;
        }

        // 내가 예약 죽음햇다면 처리
        if (HoldemGameControl.Players.GetPlayerDieReserve(CurBetPlayer) == true)
        {
            PlayerBetSelected("Die");
            return;
        }

        SyncSystem.Sync.SyncHoldemIsTurn(CurBetPlayer, true);
        // 알맞은 버튼 키기
        CalBetAndButtonSwitch();
    }

    void CalBetAndButtonSwitch()
    {
        // 정현이의 의견을 받아 죽지 않은 모든 상황에서 모든 베팅 가능하게 했음
        for (int i = 0; i < BetType.Length; i++)
        {
            _holdemUI.BetButtonInteractiveSwitch(BetType[i], true);
        }
    }

    bool IsBetEnd()
    {
        if(HoldemGameControl.Players.NowPlayerNum - HoldemGameControl.Players.GetDeadPlayerNum() == 1)
        {
            SyncSystem.Sync.SyncHoldemIsOneLeft(true);
            return true;
        }

        if(HoldemGameControl.Players.GetPlayerIsBet(CurBetPlayer) && 
            User.NowGamePlayer.BetMoney == HoldemGameControl.Players.FindHighestBet())
        {
            Debug.Log($"highest bet money : {HoldemGameControl.Players.FindHighestBet()}");
            Debug.Log($"my bet money : {User.NowGamePlayer.BetMoney}");
            return true;
        }
        return false;
    }

    void BetButtonDisable()
    {
        for(int i = 0; i < BetType.Length; i++)
        {
            _holdemUI.BetButtonInteractiveSwitch(BetType[i], false);
        }

        if (HoldemGameControl.Players.GetPlayerState(User.NowGamePlayer.GameIndex))      // Die예약을 위해 죽지 않앗다면 die는 항상 활성화
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
        HoldemGameControl.Players.UpdatePlayerTurn(curPlayer, false);

        if (betType != "Die")
        {
            HoldemGameControl.Players.UpdatePlayerBetting(curPlayer, betAmount);
            HoldemGameControl.Control.PotMoney = HoldemGameControl.Control.PotMoney + betAmount;
        }
        else
        {
            HoldemGameControl.Players.SetDeadPlayerNum(HoldemGameControl.Players.GetDeadPlayerNum() + 1);
            HoldemGameControl.Players.UpdatePlayerState(curPlayer, false);
        }
        HoldemGameControl.Control.NextStage(1);
    }

    public void CurrentStageBetEnd()
    {
        _isBetting = false;
        HoldemGameControl.Players.ClearBetSetting();
        BetButtonDisable();

        // 어차피 이 함수는 모두가 호출하니
        HoldemGameControl.Control.NextStage();
    }

    public void PlayerBetSelected(string betType)
    {
        int highestBetMoney = HoldemGameControl.Players.FindHighestBet();
        int curPlayerBetMoney = HoldemGameControl.Players.GetPlayerBet(CurBetPlayer);
        int curBetAmount = highestBetMoney - curPlayerBetMoney;

        switch (betType)
        {
            case "Die":
                Debug.Log($"Player {CurBetPlayer} Die");

                //// deadplayernum 증가
                //SyncSystem.Sync.SyncHoldemDeadPlayerNum(HoldemGameControl.Players.GetDeadPlayerNum() + 1);
                //// isalive false로
                //SyncSystem.Sync.SyncHoldemPlayerIsAlive(CurBetPlayer, false);
                break;

            case "Call":
                // 현재 레이즈 금액 체크, 현재 베팅 금액과 같을시 check
                if (curBetAmount == 0)
                {
                    //Debug.Log($"Player {curPlayer} Checked");
                }
                else
                {
                    //Debug.Log($"Player {curPlayer} Call");
                }
                break;

            case "Double":
                // 현재 레이즈 금액 체크, 레이즈 머니 배팅 + 레이즈 머니 만큼 더 레이즈
                //Debug.Log($"Player {curPlayer} Double");

                if (curBetAmount == 0)
                {
                    curBetAmount = GetBaseBetAmount(Managers.CurrentDifficulty, false);
                }
                else
                {
                    curBetAmount *= 2;
                }
                break;

            case "Half":
                // 현재 레이즈 금액 체크, 레이즈 머니 배팅 + 팟머니 * 0.5 만큼 더 레이즈
               // Debug.Log($"Player {curPlayer} Half");

                curBetAmount = curBetAmount + (HoldemGameControl.Control.PotMoney + curBetAmount) / 2;
                break;

            case "Quater":
                // 현재 레이즈 금액 체크, 레이즈 머니 배팅 + 팟머니 * 0.25 만큼 더 레이즈
                //Debug.Log($"Player {curPlayer} Quater");

                curBetAmount = curBetAmount + (HoldemGameControl.Control.PotMoney + curBetAmount) / 4;
                break;

            case "AllIn":
                // 올인 / 현재 플레이어 중 최소 금액 찾고, 내 시드 머니가 그거보다 많으면 그거만큼 배팅
                //Debug.Log($"Player {curPlayer} AllIn");

                curBetAmount = HoldemGameControl.Players.GetLowestPlayerSeedMoney();
                break;
        }

        if(betType != "Die")
        {
            User.NowUser.HoldemBettingMoney(User.NowUser.GetUid(), curBetAmount);
        }
        SyncSystem.Sync.HoldemBetProcess(CurBetPlayer, betType, curBetAmount);
    }
}
