using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldemPlayerManager
{
    Dictionary<string, string> playerNickName;
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
        playerNickName = new Dictionary<string, string>();
        holdemPlayerUID = new string[HoldemGameControl.MAX_PLAYER_NUM];
        playerSeedMoney = new int[HoldemGameControl.MAX_PLAYER_NUM];
        playerBettingMoney = new int[HoldemGameControl.MAX_PLAYER_NUM];
        playerIsBet = new bool[HoldemGameControl.MAX_PLAYER_NUM];
        playerIsAlive = new bool[HoldemGameControl.MAX_PLAYER_NUM];
        playerIsTurn = new bool[HoldemGameControl.MAX_PLAYER_NUM];
        playerDieReserve = new bool[HoldemGameControl.MAX_PLAYER_NUM];
        playerCardDetails = new int[HoldemGameControl.MAX_PLAYER_NUM, HoldemCardManager.PLAYER_CARD_NUM];
        playerCardGO = new List<(GameObject, GameObject)>();

        winnerList = new List<string>();
    }

    public void GameSetting()
    {
        playerNickName.Clear();
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
        else
        {
            playerNickName.Add(UID, Managers.Seat.GetPlayerNickNameByUID(UID));
        }

        int gameIndex = HoldemGameControl.Control.ConvertUItoGame(seatIdx);
        holdemPlayerUID[gameIndex] = UID;
    }

    public string GetPlayerNickNameByUID(string pUID)
    {
        return playerNickName[pUID];
    }

    public int GetPlayerGameIndexByUID(string pUID)
    {
        for(int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            if (pUID == GetPlayerUID(i))
                return i;
        }
        return -1;
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
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemPlayerManager.cs 파일의 UpdatePlayerSeedMoney 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        playerSeedMoney[index] = seedMoney;
        HoldemGameControl.Control.UpdatePlayerSeedMoneyUI();
    }

    public int GetPlayerBet(int index)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemPlayerManager.cs 파일의 GetPlayerBet 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        return playerBettingMoney[index];
    }

    public void UpdatePlayerBetting(int index, int amount)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemPlayerManager.cs 파일의 UpdatePlayerBetting 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        playerBettingMoney[index] += amount;
        playerIsBet[index] = true;
        HoldemGameControl.Control.UpdatePlayerBetMoneyUI();
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
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemPlayerManager.cs 파일의 UpdatePlayerTurn 함수 실행"); // 디버깅 추적용 (25.11.12 승헌

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
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemPlayerManager.cs 파일의 SetDeadPlayerNum 함수 실행"); // 디버깅 추적용 (25.11.12 승헌

        deadPlayerNum = num;
    }

    public void SetPlayerCard(string pUID, int cardViewID, int cardDetail)
    {
        int playerIndex = GetPlayerGameIndexByUID(pUID);

        GameObject cardGO = PhotonView.Find(cardViewID).gameObject;

        var ex = playerCardGO[playerIndex];
        if (HoldemGameControl.Card.CardLen == 0)
            playerCardGO[playerIndex] = (cardGO, ex.Item2);
        else
            playerCardGO[playerIndex] = (ex.Item1, cardGO);

        playerCardDetails[playerIndex, HoldemGameControl.Card.CardLen] = cardDetail;

        if (User.NowUser.GetUid() == pUID)
            cardGO.GetComponent<SpriteRenderer>().sprite = HoldemGameControl.Card.GetRightCardImage(cardDetail);
    }

    public List<string> GetWinnerList()
    {
        return winnerList;
    }

    public void SetWinnerList(string[] wList)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemPlayaerManager.cs 파일의 SetWinnerList 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        for (int i = 0; i < wList.Length; i++)
        {
            winnerList.Add(wList[i]);
        }
    }

    public int GetLowestPlayerSeedMoney()
    {
        int min_bet = int.MaxValue;
        for(int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            if (GetPlayerState(i) == false || GetPlayerUID(i) == "")
                continue;

            if (playerSeedMoney[i] < min_bet)
                min_bet = playerSeedMoney[i];
        }
        return min_bet;
    }

    public int FindHighestBet()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemPlayerManager.cs 파일의 FindHighestBet 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        int max_bet = 0;
        for(int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            if (GetPlayerState(i) == false || GetPlayerUID(i) == "")
                continue;

            if (playerBettingMoney[i] > max_bet)
                max_bet = playerBettingMoney[i];
        }
        return max_bet;
    }

    public void ShowPlayerCard()
    {
        for (int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            (GameObject, GameObject) cards = playerCardGO[i];

            if (GetPlayerUID(i) != "" && playerIsAlive[i])
            {
                cards.Item1.GetComponent<SpriteRenderer>().sprite = HoldemGameControl.Card.GetRightCardImage(playerCardDetails[i, 0]);
                cards.Item2.GetComponent<SpriteRenderer>().sprite = HoldemGameControl.Card.GetRightCardImage(playerCardDetails[i, 1]);
            }
        }
    }

    public void ClearGameSetting()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} HoldemPlayerManager.cs 파일의 ClearGameSetting 함수 실행"); // 디버깅 추적용 (25.11.12 승헌

        for (int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            (GameObject, GameObject) cards = playerCardGO[i];

            if (cards.Item1 == null)
                continue;

            if (cards.Item1.GetPhotonView().IsMine)
            {
                Managers.Resource.PhotonDestroy(cards.Item1);
                Managers.Resource.PhotonDestroy(cards.Item2);
            }
        }
    }

    public void GiveHoldemPlayerManagerSyncData(Player newPlayer)
    {

    }
}
