using System.Collections;
using System.Collections.Generic;
using Photon.Chat.UtilityScripts;
using UnityEngine;

public class HoldemPlayer
{
    int seatedIndex = -1;
    public int SeatIndex {  get { return seatedIndex; } }

    public int GameIndex { get { return HoldemGameControl.Control.ConvertUItoGame(seatedIndex); } }

    int betMoney;
    public int BetMoney { get { return betMoney; } }

    public HoldemPlayer()
    {
        Init();
    }

    void Init()
    {

    }

    public void SetSeatIndex(int idx)
    {
        seatedIndex = idx;
    }

    public void SetBetMoney(int amount, bool ttt = true)
    {
        betMoney = amount;

        if (ttt)
        {
            SyncSystem.Sync.SyncHoldemMyBetting(GameIndex, amount);
        }
    }

    public void ClearSetting()
    {
        betMoney = 0;
    }
}
