using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackBetManager
{
    UI_BlackJack _jackUI;

    bool _isBetting = false;
    public bool IsBetting { get { return _isBetting; } }

    string[] BetType =
{
        "DoubleDown",
        "Split",
        "Stand",
        "Hit",
        "BlackJack",
    };

    public const float AUTO_DIE_TIMER = 10.0f;

    int curBetPlayer = -1;
    public int CurBetPlayer {  get { return curBetPlayer; } }

    public JackBetManager()
    {
        _isBetting = false;
    }

    public void Init(UI_BlackJack ui)
    {
        _isBetting = false;
        _jackUI = ui;
    }

    public void JackBetting(int playerIndex, int amount)
    {
        string pUID = JackGameControl.Players.GetPlayerUID(playerIndex);
        SyncSystem.Sync.JackBetMoneyToTarget(pUID, amount);

        SyncSystem.Sync.SyncJackMyBetting(playerIndex, amount);
    }

    public void JackBettingReset(int playerIndex)
    {
        if (User.NowGamePlayer.BetMoney == 0)
            return;

        User.NowUser.JackResetBetting();
        SyncSystem.Sync.SyncJackMyBettingReset(playerIndex);
    }

    public void JackBettingConfirm(int playerIndex)
    {
        if (User.NowGamePlayer.BetMoney == 0)
            return;

        _jackUI.FirstBetEarlyEnd();
        SyncSystem.Sync.SyncJackIsBet(playerIndex, true);
    }

    public void UpdateCurBetPlayer(int playerIndex)
    {
        curBetPlayer = playerIndex;
    }
}
