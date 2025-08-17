using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class JackPlayerManager
{
    Dictionary<string, string> playerNickName;
    string[] jackPlayerUID;
    int _nowPlayerNum;
    public int NowPlayerNum { get { return _nowPlayerNum; } }

    int deadPlayerNum = 0;
    int[] playerSeedMoney;
    int[] playerBettingMoney;
    bool[] playerIsBet;
    bool[] playerIsAlive;
    bool isOneLeft;
    public bool IsOneLeft
    {
        get { return isOneLeft; }
        set { isOneLeft = value; }
    }

    List<GameObject>[] playerCardGO;        ///////////////////// 스플릿 생각
    List<int>[] playerCardDetails;
    Tuple<int, int>[] playerCardScore;
    List<int>[] PlayerCards { get { return playerCardDetails; } }

    public JackPlayerManager()
    {
        playerNickName = new Dictionary<string, string>();
        jackPlayerUID = new string[JackGameControl.MAX_PLAYER_NUM];
        playerSeedMoney = new int[JackGameControl.MAX_PLAYER_NUM];
        playerBettingMoney = new int[JackGameControl.MAX_PLAYER_NUM];
        playerIsBet = new bool[JackGameControl.MAX_PLAYER_NUM];
        playerIsAlive = new bool[JackGameControl.MAX_PLAYER_NUM];

        playerCardGO = new List<GameObject>[JackGameControl.MAX_PLAYER_NUM];
        playerCardDetails = new List<int>[JackGameControl.MAX_PLAYER_NUM];
        playerCardScore = new Tuple<int, int>[JackGameControl.MAX_PLAYER_NUM];
        for (int i = 0; i < JackGameControl.MAX_PLAYER_NUM; i++)
        {
            playerCardGO[i] = new List<GameObject>();
            playerCardDetails[i] = new List<int>();
            playerCardScore[i] = Tuple.Create(-1, -1);
        }
    }

    public void GameSetting()
    {
        playerNickName.Clear();
        for (int i = 0; i < JackGameControl.MAX_PLAYER_NUM; i++)
        {
            playerBettingMoney[i] = 0;
            playerIsBet[i] = false;
            playerIsAlive[i] = true;
            playerSeedMoney[i] = 0;

            playerCardGO[i].Clear();
            playerCardDetails[i].Clear();
            for (int j = 0; j < JackCardManager.PLAYER_CARD_NUM; j++)
            {
                playerCardGO[i].Add(null);
                playerCardDetails[i].Add(-1);
                playerCardScore[i] = Tuple.Create(-1, -1);
            }
        }

        deadPlayerNum = 0;
        _nowPlayerNum = 0;
        isOneLeft = false;
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

        int gameIndex = seatIdx;
        jackPlayerUID[gameIndex] = UID;
    }

    public string GetPlayerNickNameByUID(string pUID)
    {
        return playerNickName[pUID];
    }

    public int GetPlayerGameIndexByUID(string pUID)
    {
        for (int i = 0; i < JackGameControl.MAX_PLAYER_NUM; i++)
        {
            if (pUID == GetPlayerUID(i))
                return i;
        }
        return -1;
    }

    public string GetPlayerUID(int index)
    {
        return jackPlayerUID[index];
    }

    public int GetPlayerSeedMoney(int index)
    {
        return playerSeedMoney[index];
    }

    public void UpdatePlayerSeedMoney(int index, int seedMoney)
    {
        playerSeedMoney[index] = seedMoney;
        JackGameControl.Control.UpdatePlayerSeedMoneyUI();
    }

    public int GetPlayerBet(int index)
    {
        return playerBettingMoney[index];
    }

    public void UpdatePlayerBetting(int index, int amount)
    {
        playerBettingMoney[index] += amount;
        JackGameControl.Control.UpdatePlayerBetMoneyUI();
    }

    public void UpdatePlayerBetReset(int index)
    {
        playerBettingMoney[index] = 0;
        JackGameControl.Control.UpdatePlayerBetMoneyUI();
    }

    public void UpdatePlayerIsBet(int index, bool val)
    {
        playerIsBet[index] = val;
    }

    public bool GetPlayerIsBet(int index)
    {
        return playerIsBet[index];
    }

    public int GetPlayerCardLen(int index)
    {
        for (int i = 0; i < playerCardGO.Length; i++)
        {
            if (playerCardGO[index][i] == null)
                return i;
        }
        return -1;
    }

    public void SetPlayerCard(string pUID, int cardViewID, int cardDetail)
    {
        int playerIndex = GetPlayerGameIndexByUID(pUID);
        int cardIndex = GetPlayerCardLen(playerIndex);
        GameObject cardGO = PhotonView.Find(cardViewID).gameObject;

        playerCardGO[playerIndex][cardIndex] = cardGO;
        playerCardDetails[playerIndex][cardIndex] = cardDetail;
        Debug.Log($"PlayerIndex : {playerIndex}, Player CardIndex : {cardIndex}");

        UI_Card cardUI = cardGO.GetOrAddComponent<UI_Card>();
        cardUI.SetCardImage(cardDetail);
        Debug.Log("c");

        JackGameControl.Control.UpdatePlayerBetScoreUI(playerIndex);

        JackGameControl.Control.NextStage(1);
    }

    public Tuple<int, int> CalculatePlayerBetScore(int playerIndex)
    {
        int[] score = new int[22];
        if (playerCardScore[playerIndex].Item1 == -1)
        {
            score[0] = 1;
        }
        else
        {
            score[playerCardScore[playerIndex].Item1] = 1;

            if (playerCardScore[playerIndex].Item2 != -1)
                score[playerCardScore[playerIndex].Item2] = 1;
        }

        int i = -1;

        for (int ii = 0; ii < playerCardGO[playerIndex].Count; ii++)
        {
            if (playerCardGO[playerIndex][ii] == null)
                break;

            i = ii;
        }

        int cardscore = JackGameControl.Card.GetCardNum(playerCardDetails[playerIndex][i]);
        if (cardscore >= 10)
            cardscore = 10;

        int[] tmp = new int[22];
        for (int j = 0; j < 22; j++)
        {
            if (score[j] == 1)
            {
                int s = j + cardscore;

                if (s <= 21 && tmp[s] == 0)
                    tmp[s] = 1;

                if (cardscore == 1)
                {
                    s = j + 11;

                    if (s <= 21 && tmp[s] == 0)
                        tmp[s] = 1;
                }
            }
        }
        score = tmp;

        int a = -1;
        int b = -1;

        for (int j = 0; j < 22; j++)
        {
            if (score[j] == 1)
            {
                if (a == -1)
                    a = j;
                else
                    b = j;
            }
        }

        playerCardScore[playerIndex] = Tuple.Create(a, b);

        return playerCardScore[playerIndex];
    }

    public void FindPlayerBlackJack()
    {
        for (int i = 0; i < JackGameControl.MAX_PLAYER_NUM; i++)
        {
            if (GetPlayerUID(i) == "")
                continue;

            if (playerCardScore[i].Item1 == 21 || playerCardScore[i].Item2 == 21)
                SyncSystem.Sync.JackNoticeBlackJack(i);
        }
    }
}
