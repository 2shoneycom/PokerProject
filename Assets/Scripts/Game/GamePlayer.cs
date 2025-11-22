using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayer
{
    int seatedIndex = -1;
    Define.GameType curGameType = Define.GameType.None;

    HoldemGameControl holdemControl;
    PokerGameControl pokerControl;
    JackGameControl jackControl;

    public int SeatIndex { get { return seatedIndex; } }

    public int GameIndex
    {
        get
        {
            switch (curGameType)
            {
                case Define.GameType.Holdem:
                    if (holdemControl == null)
                        return 0;
                    return holdemControl.ConvertUItoGame(seatedIndex);

                case Define.GameType.Poker:
                    if (pokerControl == null)
                        return 0;
                    return pokerControl.ConvertUItoGame(seatedIndex);

                case Define.GameType.BlackJack:
                    if (jackControl == null)
                        return 0;
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

    public GamePlayer(HoldemGameControl control)
    {
        Init();
        holdemControl = control;
    }

    public GamePlayer(PokerGameControl control)
    {
        Init();
        pokerControl = control;
    }

    public GamePlayer(JackGameControl control)
    {
        Init();
        jackControl = control;
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

        if (User.NowUser.IsEnoughMoney())
            return;

        switch (curGameType)
        {
            case Define.GameType.Holdem:
                User.NowUser.SetIsNotEnough(true);
                UI_Holdem scene = (UI_Holdem)Managers.UI.SceneUI;
                scene.RoomLeave();
                break;

            case Define.GameType.Poker:


            case Define.GameType.BlackJack:


            default:
                break;

        }
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
