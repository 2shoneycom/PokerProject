using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PokerBetManager
{
    UI_Poker _pokerUI;

    bool _isBetting = false;
    public bool IsBetting {  get { return _isBetting; }}

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
        set { curBetPlayer = value; }
    }
    public const float AUTO_DIE_TIMER = 10.0f;

    public PokerBetManager()
    {
        _isBetting = false;
    }

    public void Init(UI_Poker ui)
    {
        _isBetting = false;
        _pokerUI = ui;
        CurBetPlayer = 0;
    }

    public void BaseBetting(int playerIndex)
    {
        string pUID = PokerGameControl.Players.GetPlayerUID(playerIndex);
        int dAmount = GetBaseBetAmount(Managers.CurrentDifficulty);
        SyncSystem.Sync.PokerBetMoneyToTarget(pUID, dAmount);

        SyncSystem.Sync.SyncPokerMyBetting(playerIndex, dAmount);
    }

    //임의로 정한 값
    public int GetBaseBetAmount(Define.Difficulty diff)
    {
        int baseBet = 500;

        return baseBet * 2;
    }

    public void HandleBet(int curPlayer)
    {
        // 관전자는 리턴
        if (!PokerGameControl.Control.IsPlaying)
            return;

        CurBetPlayer = curPlayer;

        // 버튼 비활성화
        BetButtonDisable();

        // 배팅 시작 표시
        if (IsBetting == false)
        {
            _isBetting = true;
        }

        if (PokerGameControl.Players.GetPlayerUID(curPlayer) != User.NowUser.GetUid())
            return;

        if (PokerGameControl.Players.IsOneLeft || IsBetEnd())
        {
            // 1명 남앗거나 정상 배팅 종료의 경우
            Debug.Log("bet end in IsBetEnd");
            SyncSystem.Sync.PokerBetEnd();
            return;
        }

        // 내가 이미 죽엇다면 처리
        if (PokerGameControl.Players.GetPlayerState(CurBetPlayer) == false)
        {
            SyncSystem.Sync.PokerNextStage_V2(1);
            return;
        }

        // 내가 예약 죽음햇다면 처리
        if (PokerGameControl.Players.GetPlayerDieReserve(CurBetPlayer) == true)
        {
            PlayerBetSelected("Die");
            return;
        }

        SyncSystem.Sync.SyncPokerIsTurn(CurBetPlayer, true);
        // 알맞은 버튼 키기
        CalBetAndButtonSwitch();
    }

    void CalBetAndButtonSwitch()
    {
        if(PokerGameControl.Players.GetPlayerIsCall(CurBetPlayer) == false)
        {
            for (int i = 0; i < BetType.Length; i++)
            {
                _pokerUI.BetButtonInteractiveSwitch(BetType[i], true);
            }
        }
        else
        {
            _pokerUI.BetButtonInteractiveSwitch("Die", true);
            _pokerUI.BetButtonInteractiveSwitch("Call", true);
        }
    }

    bool IsBetEnd()
    {
        if (PokerGameControl.Players.NowPlayerNum - PokerGameControl.Players.GetDeadPlayerNum() == 1)
        {
            SyncSystem.Sync.SyncPokerIsOneLeft(true);
            return true;
        }

        if (PokerGameControl.Players.FindBetEndTerm() == true &&
            User.NowGamePlayer.BetMoney == PokerGameControl.Players.FindHighestBet())
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

        if (PokerGameControl.Players.GetPlayerState(User.NowGamePlayer.GameIndex))      // Die예약을 위해 죽지 않앗다면 die는 항상 활성화
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
        PokerGameControl.Players.UpdatePlayerTurn(curPlayer, false);

        if (betType != "Die")
        {
            PokerGameControl.Players.UpdatePlayerBetting(curPlayer, betAmount, true);
            PokerGameControl.Control.PotMoney = PokerGameControl.Control.PotMoney + betAmount;
        }
        else
        {
            PokerGameControl.Players.SetDeadPlayerNum(PokerGameControl.Players.GetDeadPlayerNum() + 1);
            PokerGameControl.Players.UpdatePlayerState(curPlayer, false);
        }
        PokerGameControl.Control.NextStage(1);
    }

    public void CurrentStageBetEnd()
    {
        _isBetting = false;
        PokerGameControl.Players.ClearBetSetting();
        BetButtonDisable();

        // 어차피 이 함수는 모두가 호출하니
        PokerGameControl.Control.NextStage();
    }

    public void PlayerBetSelected(string betType)
    {
        int highestBetMoney = PokerGameControl.Players.FindHighestBet();
        int curPlayerBetMoney = PokerGameControl.Players.GetPlayerBet(CurBetPlayer);
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
                    curBetAmount = GetBaseBetAmount(Managers.CurrentDifficulty);
                }
                else
                {
                    curBetAmount *= 2;
                }
                break;

            case "Half":
                // 현재 레이즈 금액 체크, 레이즈 머니 배팅 + 팟머니 * 0.5 만큼 더 레이즈
                // Debug.Log($"Player {curPlayer} Half");

                curBetAmount = curBetAmount + (PokerGameControl.Control.PotMoney + curBetAmount) / 2;
                break;

            case "Quater":
                // 현재 레이즈 금액 체크, 레이즈 머니 배팅 + 팟머니 * 0.25 만큼 더 레이즈
                //Debug.Log($"Player {curPlayer} Quater");

                curBetAmount = curBetAmount + (PokerGameControl.Control.PotMoney + curBetAmount) / 4;
                break;

            case "AllIn":
                // 올인 / 현재 플레이어 중 최소 금액 찾고, 내 시드 머니가 그거보다 많으면 그거만큼 배팅
                //Debug.Log($"Player {curPlayer} AllIn");

                curBetAmount = PokerGameControl.Players.GetLowestPlayerSeedMoney();
                break;
        }

        if (betType != "Die")
        {
            User.NowUser.PokerBettingMoney(User.NowUser.GetUid(), curBetAmount);
        }
        SyncSystem.Sync.PokerBetProcess(CurBetPlayer, betType, curBetAmount);
    }
}
