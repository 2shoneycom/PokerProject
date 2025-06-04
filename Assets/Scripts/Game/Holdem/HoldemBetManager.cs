using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
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
        SyncSystem.Sync.SyncHoldemPlayerIsBet(playerIndex, true);
    }

    //임의로 정한 값
    public int GetBaseBetAmount(Define.Difficulty diff, bool isSB)
    {
        int baseBet = 500;

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
        Debug.Log($"Player {curPlayer} Turn!");
        // 버튼 비활성화
        BetButtonDisable();

        // 배팅 시작 표시
        if (IsBetting == false)
        {
            _isBetting = true;
        }

        if (HoldemGameControl.Players.GetPlayerUID(curPlayer) != User.NowUser.GetNickName())
            return;

        if (HoldemGameControl.Players.IsOneLeft || IsBetEnd(curPlayer))
        {
            // 1명 남앗거나 정상 배팅 종료의 경우
            SyncSystem.Sync.HoldemBetEnd();
            return;
        }

        // 내가 이미 죽엇다면 처리
        if (HoldemGameControl.Players.GetPlayerState(curPlayer) == false)
        {
            SyncSystem.Sync.HoldemNextStage_V2(1);
            return;
        }

        // 내가 예약 죽음햇다면 처리
        if (HoldemGameControl.Players.GetPlayerDieReserve(curPlayer) == true)
        {
            SyncSystem.Sync.HoldemBetProcess(curPlayer, "Die");
            return;
        }

        SyncSystem.Sync.SyncHoldemIsTurn(curPlayer, true);
        // 알맞은 버튼 키기
        CalBetAndButtonSwitch();
    }

    void CalBetAndButtonSwitch()
    {
        //for (int i = 0; i < BetType.Length; i++)
        //{
        //    _holdemUI.BetButtonInteractiveSwitch(BetType[i], true);
        //}

        _holdemUI.BetButtonInteractiveSwitch("Call", true);       // 여기까지 왔다면 call은 항상 on,  어차피 계산은 마스터가 할거임
        _holdemUI.BetButtonInteractiveSwitch("Double", true);     // double도 항상 on, 어차피 계산은 마스터가 할거임 
                                                                  // 근데 예전 코드를 보니까 배팅을 햇다면 막앗던데 그랫던 이유가 있나??
                                                                  // 플레이어들 돈만 모자르지 않으면 무한정 레이즈 가능 아니엇음?
        _holdemUI.BetButtonInteractiveSwitch("Die", true);
        _holdemUI.BetButtonInteractiveSwitch("Quater", true);     // 레이즈 머니 + potmoney의 1/4     ex) pot : 500, 이번 레이즈 : 300 -> quater 시 300 + (500+300)*0.25
        _holdemUI.BetButtonInteractiveSwitch("Half", true);       // 레이즈 머니 + potmoney의 1/2     ex) pot : 500, 이번 레이즈 : 300 -> half 시 300 + (500+300)*0.5
        _holdemUI.BetButtonInteractiveSwitch("AllIn", true);

        // 어쩌다 보니 다 키게 되었는데 진짜 다키는거아님? 올인같은 돈없는 특수 상황들 제외하면


        // 이제 배팅 구현하다 보니 만약에 레이즈 금액 없으면 half랑 quater은 안키는게 나을듯?
        // 긍까 체크들 할때는 double 즉 첫 레이즈만 가능하게
        // 이게 아래 if문 조건이 맞나?
        if(HoldemGameControl.Players.FindHighestBet() == User.NowHoldemPlayer.BetMoney)
        {
            _holdemUI.BetButtonInteractiveSwitch("Quater", false);
            _holdemUI.BetButtonInteractiveSwitch("Half", false);
        }
    }

    bool IsBetEnd(int curPlayer)
    {
        if(HoldemGameControl.Players.NowPlayerNum - HoldemGameControl.Players.GetDeadPlayerNum() == 1){
            SyncSystem.Sync.SyncHoldemIsOneLeft(true);
            return true;
        }

        if(HoldemGameControl.Players.GetPlayerIsBet(curPlayer) && 
            HoldemGameControl.Players.GetPlayerBet(curPlayer) == HoldemGameControl.Players.FindHighestBet())
        {
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

        if (HoldemGameControl.Players.GetPlayerState(User.NowHoldemPlayer.GameIndex))      // Die예약을 위해 죽지 않앗다면 die는 항상 활성화
            _holdemUI.BetButtonInteractiveSwitch("Die", true);
    }

    public IEnumerator AutoDieTimer(float duration)
    {
        yield return new WaitForSeconds(duration);

        // 현재 플레이어가 7초 동안 버튼을 누르지 않았을 경우 Die 처리
        Debug.Log($"Player {curBetPlayer} didn't respond. Automatically choosing Die.");

        if (PhotonNetwork.IsMasterClient)
            BetProcess(curBetPlayer, "Die");
    }

    public void BetProcess(int curPlayer, string betType)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        SyncSystem.Sync.SyncHoldemPlayerIsBet(curPlayer, true);

        int highestBetMoney = HoldemGameControl.Players.FindHighestBet();
        int curPlayerBetMoney = HoldemGameControl.Players.GetPlayerBet(curPlayer);
        int curBetAmount = highestBetMoney - curPlayerBetMoney;

        switch (betType)
        {
            case "Die":
                Debug.Log($"Player {curPlayer} Die");

                // deadplayernum 증가
                SyncSystem.Sync.SyncHoldemDeadPlayerNum(HoldemGameControl.Players.GetDeadPlayerNum() + 1);
                // isalive false로
                SyncSystem.Sync.SyncHoldemPlayerIsAlive(curPlayer, false);

                break;

            case "Call":
                // 현재 레이즈 금액 체크, 현재 베팅 금액과 같을시 check
                if(curBetAmount == 0)
                {
                    Debug.Log($"Player {curPlayer} Checked");
                }
                else
                {
                    Debug.Log($"Player {curPlayer} Call");
                }
                break;

            case "Double":
                // 현재 레이즈 금액 체크, 레이즈 머니 배팅 + 레이즈 머니 만큼 더 레이즈
                Debug.Log($"Player {curPlayer} Double");

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
                Debug.Log($"Player {curPlayer} Half");

                curBetAmount = curBetAmount + (HoldemGameControl.Control.PotMoney + curBetAmount) / 2;
                break;

            case "Quater":
                // 현재 레이즈 금액 체크, 레이즈 머니 배팅 + 팟머니 * 0.25 만큼 더 레이즈
                Debug.Log($"Player {curPlayer} Quater");

                curBetAmount = curBetAmount + (HoldemGameControl.Control.PotMoney + curBetAmount) / 4;
                break;

            case "AllIn":
                // 올인 / 현재 플레이어 중 최소 금액 찾고, 내 시드 머니가 그거보다 많으면 그거만큼 배팅
                Debug.Log($"Player {curPlayer} AllIn");

                break;
        }

        if(betType != "Die")
        {
            SyncSystem.Sync.HoldemBetMoneyToTarget(HoldemGameControl.Players.GetPlayerUID(curPlayer), curBetAmount);
            HoldemGameControl.Control.Request_SyncHoldemPotMoney(curBetAmount);
        }
        SyncSystem.Sync.HoldemNextStage_V2(1);
    }

    public void CurrentStageBetEnd()
    {
        _isBetting = false;
        HoldemGameControl.Players.ClearBetSetting();

        // 어차피 이 함수는 모두가 호출하니
        HoldemGameControl.Control.NextStage();
    }

    public void PlayerBetSelected(string betType)
    {
        SyncSystem.Sync.SyncHoldemIsTurn(User.NowHoldemPlayer.GameIndex, false);
        //BetButtonDisable();
        SyncSystem.Sync.HoldemBetProcess(curBetPlayer, betType);
    }
}
