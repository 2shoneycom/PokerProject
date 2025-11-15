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
    int blackJackBaseBetAmount = 0;
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
        Debug.Log($"#{++Define.DEBUG_INDEX} GamePlayer.cs 파일의 SetBetMoney 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        betMoney = amount;
    }

    public void ClearSetting()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} GamePlayer.cs 파일의 ClearSetting 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        betMoney = 0;
        blackJackBaseBetAmount = 0;
    }

    public void SetBlackJackBaseBet()
    {
        blackJackBaseBetAmount = betMoney;
    }

    public int GetBlackJackBaseBet()
    {
        return blackJackBaseBetAmount;
    }
}
