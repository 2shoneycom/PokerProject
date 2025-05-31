using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldemPlayerManager
{
    string[] holdemPlayerUID;
    int _nowPlayerNum;
    public int NowPlayerNum { get { return _nowPlayerNum; } }

    int highestDealPlayer = 0;
    int[] playerBettingMoney;
    bool[] playerIsBet;
    bool[] playerIsAlive;
    bool[] playerDieReserve;

    public HoldemPlayerManager(int max_num)
    {
        holdemPlayerUID = new string[max_num];
        playerBettingMoney = new int[max_num];
        playerIsBet = new bool[max_num];
        playerIsAlive = new bool[max_num];
        playerDieReserve = new bool[max_num];
    }

    public void GameSetting()
    {
        for (int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            playerBettingMoney[i] = 0;
            playerIsAlive[i] = true;
            playerIsBet[i] = false;
            playerDieReserve[i] = false;
        }
    }

    public void UpdatePlayerUID(int seatIdx, string UID)
    {
        _nowPlayerNum++;

        if (UID == SeatManager.DEFAULT_NULL_SEAT)
        {
            UID = "";
            _nowPlayerNum--;
        }

        int gameIndex = HoldemGameControl.Control.ConvertUItoGame(seatIdx);
        holdemPlayerUID[gameIndex] = UID;
    }

    public string GetPlayerUID(int index)
    {
        return holdemPlayerUID[index];
    }

    public int GetPlayerBet(int index)
    {
        return playerBettingMoney[index];
    }

    public bool GetPlayerIsBet(int index)
    {
        return playerIsBet[index];
    }

    public bool GetPlayerState(int index)
    {
        return playerIsAlive[index];
    }

    public bool GetPlayerDieReserve(int index)
    {
        return playerDieReserve[index];
    }

    public int FindHighestBet()
    {
        int max_bet = 0;
        for(int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            if(max_bet < playerBettingMoney[i])
                max_bet = playerBettingMoney[i];
        }
        return max_bet;
    }

    public void UpdatePlayerBetting(int index, int amount)      // User가 자신의 HoldemPlayer에서 SetBetMoney을 호출할때 마다 동기화
    {
        playerBettingMoney[index] = amount;
        playerIsBet[index] = true;
    }

}
