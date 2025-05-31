using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
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

    int callMoney = 0;
    public const float AUTO_DIE_TIMER = 60.0f;

    public HoldemBetManager()
    {
        _isBetting = false;
    }

    public void Init(UI_Holdem ui)
    {
        _isBetting = false;
        _holdemUI = ui;
    }

    public void BaseBetting(int playerSB, int playerBB)
    {
        string pUID = HoldemGameControl.Players.GetPlayerUID(playerSB);
        int dAmount = GetBaseBetAmount(Managers.CurrentDifficulty, true);
        SyncSystem.Instacne.HoldemBetMoneyToTarget(pUID, dAmount);

        pUID = HoldemGameControl.Players.GetPlayerUID(playerBB);
        dAmount = GetBaseBetAmount(Managers.CurrentDifficulty, false);
        SyncSystem.Instacne.HoldemBetMoneyToTarget(pUID, dAmount);
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

        // 버튼 비활성화
        BetButtonDisable();

        // 배팅 시작 표시
        if (IsBetting == false)
        {
            _isBetting = true;
        }

        if (HoldemGameControl.Players.GetPlayerUID(curPlayer) != User.NowUser.nickName)
            return;

        //배팅 종료 경우 체크
        if (IsBetEnd(curPlayer))
        {
            // 1명만 남은 경우?
            // 정상 베팅 종료의 경우
            SyncSystem.Instacne.HoldemNextStage();
            return;
        }

        // 내가 이미 죽엇다면 처리
        if (HoldemGameControl.Players.GetPlayerState(curPlayer) == false)
        {
            SyncSystem.Instacne.HoldemNextStage(1);
            return;
        }
        // 내가 예약 죽음햇다면 처리
        if (HoldemGameControl.Players.GetPlayerDieReserve(curPlayer) == true)
        {
            SyncSystem.Instacne.HoldemBetProcess(curPlayer, "Die");
            return;
        }
        // 알맞은 버튼 키기
        CalBetAndButtonSwitch();
    }

    public void CalBetAndButtonSwitch()
    {
        _holdemUI.BetButtonInteractiveSwitch("Call", true);       // 여기까지 왔다면 call은 항상 on,  어차피 계산은 마스터가 할거임
        _holdemUI.BetButtonInteractiveSwitch("Double", true);     // double도 항상 on, 어차피 계산은 마스터가 할거임 
                                                                  // 근데 예전 코드를 보니까 배팅을 햇다면 막앗던데 그랫던 이유가 있나??
                                                                  // 플레이어들 돈만 모자르지 않으면 무한정 레이즈 가능 아니엇음?
        _holdemUI.BetButtonInteractiveSwitch("Die", true);
        _holdemUI.BetButtonInteractiveSwitch("Quater", true);     // 레이즈 머니 + potmoney의 1/4     ex) pot : 500, 이번 레이즈 : 300 -> quater 시 300 + (500+300)*0.25
        _holdemUI.BetButtonInteractiveSwitch("Half", true);       // 레이즈 머니 + potmoney의 1/2     ex) pot : 500, 이번 레이즈 : 300 -> half 시 300 + (500+300)*0.5
        _holdemUI.BetButtonInteractiveSwitch("AllIn", true);

        // 어쩌다 보니 다 키게 되었는데 진짜 다키는거아님? 올인같은 돈없는 특수 상황들 제외하면
    }

    bool IsBetEnd(int curPlayer)
    {
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
    }

    public IEnumerator AutoDieTimer(float duration)
    {
        yield return new WaitForSeconds(duration);

        // 현재 플레이어가 7초 동안 버튼을 누르지 않았을 경우 Die 처리
        //Debug.Log($"Player {players[currentPlayerIndex].Name} didn't respond. Automatically choosing Die.");
        //if (PhotonNetwork.IsMasterClient) OnButtonClicked("Die");
    }

    public void BetProcess(int curPlayer, string betType)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;



        SyncSystem.Instacne.HoldemNextStage(1);
    }

    public void CurrentPlayerBetting(string betType)
    {

    }

}
