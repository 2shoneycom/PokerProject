using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackBetManager
{
    UI_BlackJack _jackUI;
    JackGameControl _control;

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

    public JackBetManager(JackGameControl control)
    {
        _control = control;
        _isBetting = false;
    }

    public void Init(UI_BlackJack ui)
    {
        _isBetting = false;
        _jackUI = ui;
    }

    public void JackBetting(int playerIndex, int splitNum, int amount)
    {
        string pUID = _control.Players.GetPlayerUID(playerIndex);
        _control.Sync.JackBetMoneyToTarget(pUID, amount);

        _control.Sync.SyncJackMyBetting(playerIndex, splitNum, amount);
    }

    public void JackBettingReset(int playerIndex)
    {
        if (User.NowGamePlayer.BetMoney == 0)
            return;

        User.NowUser.JackResetBetting();
        _control.Sync.SyncJackMyBettingReset(playerIndex, 0);
    }

    public void JackBettingConfirm(int playerIndex)
    {
        if (User.NowGamePlayer.BetMoney == 0)
            return;

        _jackUI.FirstBetEarlyEnd();
        _control.Sync.SyncJackIsBet(playerIndex, true);
    }

    public void UpdateCurBetPlayer(int playerIndex)
    {
        curBetPlayer = playerIndex;
    }
}
