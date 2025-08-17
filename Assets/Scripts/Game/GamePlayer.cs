using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayer
{
    int seatedIndex = -1;
    Define.GameType curGameType = Define.GameType.None;

    public int SeatIndex { get { return seatedIndex; } }

    public int GameIndex { 
        get {
            switch (curGameType)
            {
                case Define.GameType.Holdem:
                    return HoldemGameControl.Control.ConvertUItoGame(seatedIndex);
                case Define.GameType.Poker:
                    return PokerGameControl.Control.ConvertUItoGame(seatedIndex);
                case Define.GameType.BlackJack:
                    return seatedIndex;
                default:
                    return 0;
            }
        } 
    }

    int betMoney;
    public int BetMoney { get { return betMoney; } }

    public GamePlayer()
    {
        Init();
    }

    void Init()
    {
        curGameType = Managers.CurrentGameType;
    }

    public void SetSeatIndex(int idx)
    {
        seatedIndex = idx;
    }

    public void SetBetMoney(int amount)
    {
        betMoney = amount;
    }

    public void ClearSetting()
    {
        betMoney = 0;
    }
}
