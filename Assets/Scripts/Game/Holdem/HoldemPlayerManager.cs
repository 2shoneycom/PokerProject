using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldemPlayerManager
{
    string[] holdemPlayerUID;
    int _nowPlayerNum;
    public int NowPlayerNum { get { return _nowPlayerNum; } }

    int deadPlayerNum = 0;
    int[] playerSeedMoney;
    int[] playerBettingMoney;
    bool[] playerIsBet;
    bool[] playerIsAlive;
    bool[] playerIsTurn;
    bool[] playerDieReserve;
    bool isOneLeft;
    public bool IsOneLeft
    {
        get { return isOneLeft; }
        set { isOneLeft = value; }
    }

    List<(GameObject, GameObject)> playerCardGO;
    int[,] playerCardDetails;
    public int[,] PlayerCards {  get { return playerCardDetails; } }

    List<string> winnerList;    // UI 용도

    public HoldemPlayerManager()
    {
        holdemPlayerUID = new string[HoldemGameControl.MAX_PLAYER_NUM];
        playerSeedMoney = new int[HoldemGameControl.MAX_PLAYER_NUM];
        playerBettingMoney = new int[HoldemGameControl.MAX_PLAYER_NUM];
        playerIsBet = new bool[HoldemGameControl.MAX_PLAYER_NUM];
        playerIsAlive = new bool[HoldemGameControl.MAX_PLAYER_NUM];
        playerIsTurn = new bool[HoldemGameControl.MAX_PLAYER_NUM];
        playerDieReserve = new bool[HoldemGameControl.MAX_PLAYER_NUM];
        playerCardDetails = new int[HoldemGameControl.MAX_PLAYER_NUM, HoldemPlayer.MAX_CARD_NUM];
        playerCardGO = new List<(GameObject, GameObject)>();

        winnerList = new List<string>();
    }

    public void GameSetting()
    {
        playerCardGO.Clear();
        for (int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            playerBettingMoney[i] = 0;
            playerIsBet[i] = false;
            playerIsAlive[i] = true;
            playerIsTurn[i] = false;
            playerDieReserve[i] = false;
            playerSeedMoney[i] = 0;

            playerCardDetails[i, 0] = 0;
            playerCardDetails[i, 1] = 0;
            playerCardGO.Add((null, null));
        }
        winnerList.Clear();

        deadPlayerNum = 0;
        _nowPlayerNum = 0;
        isOneLeft = false;
    }

    public void ClearBetSetting()
    {
        for (int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            playerIsBet[i] = false;
            playerIsTurn[i] = false;
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

    public int GetPlayerSeedMoney(int index)
    {
        return playerSeedMoney[index];
    }

    public void UpdatePlayerSeedMoney(int index, int seedMoney)
    {
        playerSeedMoney[index] = seedMoney;
    }

    public int GetPlayerBet(int index)
    {
        return playerBettingMoney[index];
    }

    public void UpdatePlayerBetting(int index, int amount)      // User가 자신의 HoldemPlayer에서 SetBetMoney을 호출할때 마다 동기화
    {
        playerBettingMoney[index] = amount;
        playerIsBet[index] = true;
    }

    public bool GetPlayerIsBet(int index)
    {
        return playerIsBet[index];
    }

    public void UpdatePlayerIsBet(int index, bool val)
    {
        playerIsBet[index] = val;
    }

    public bool GetPlayerState(int index)
    {
        return playerIsAlive[index];
    }

    public void UpdatePlayerState(int index, bool val)
    {
        playerIsAlive[index] = val;
    }

    public bool GetPlayerTurn(int index)
    {
        return playerIsTurn[index];
    }

    public void UpdatePlayerTurn(int index, bool val)
    {
        playerIsTurn[index] = val;
    }

    public bool GetPlayerDieReserve(int index)
    {
        return playerDieReserve[index];
    }

    public void UpdatePlayerDieReserve(int index, bool val)
    {
        playerDieReserve[index] = val;
    }

    public int GetDeadPlayerNum()
    {
        return deadPlayerNum;
    }

    public void SetDeadPlayerNum(int num)
    {
        deadPlayerNum = num;
    }

    public void SetPlayerCardDetails(int index, int card1, int card2)
    {
        playerCardDetails[index, 0] = card1;
        playerCardDetails[index, 1] = card2;
    }

    public void test(string uid, int cardlen, int popedcard)        /////////////////////////////////////////////////////////////////////////////////////////////////
    {
        for (int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            if(uid == GetPlayerUID(i))
            {
                playerCardDetails[i, cardlen] = popedcard;
            }
        }
    }

    public List<string> GetWinnerList()
    {
        return winnerList;
    }

    public void SetWinnerList(string[] wList)
    {
        for (int i = 0; i < wList.Length; i++)
        {
            winnerList.Add(wList[i]);
        }
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
}
