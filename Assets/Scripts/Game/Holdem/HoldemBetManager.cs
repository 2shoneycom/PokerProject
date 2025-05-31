using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldemBetManager
{
    UI_Holdem _holdemUI;

    public HoldemBetManager(UI_Holdem ui)
    {
        _holdemUI = ui;
    }

    public void CalBetAndButtonSwitch()
    {

    }

    public void BaseBetting(int playerSB, int playerBB)
    {
        string pUID = HoldemGameControl.Players.GetPlayerUID(playerSB);
        int dAmount = GetBaseBetAmount(Managers.CurrentDifficulty, true);
        SyncSystem.Instacne.DecreaseMoneyToTarget(pUID, dAmount);

        pUID = HoldemGameControl.Players.GetPlayerUID(playerBB);
        dAmount = GetBaseBetAmount(Managers.CurrentDifficulty, false);
        SyncSystem.Instacne.DecreaseMoneyToTarget(pUID,dAmount);
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
}
