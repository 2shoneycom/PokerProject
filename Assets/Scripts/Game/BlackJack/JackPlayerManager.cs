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

    int[] playerSeedMoney;
    int[,] playerBettingMoney;
    bool[] playerIsBet;
    bool[,] playerIsGameEnd;
    int[] playerIsInsurance;

    List<GameObject>[,] playerCardGO;        ///////////////////// 스플릿 생각
    List<int>[,] playerCardDetails;

    Tuple<int, int>[,] playerCardScore;

    public JackPlayerManager()
    {
        playerNickName = new Dictionary<string, string>();
        jackPlayerUID = new string[JackGameControl.MAX_PLAYER_NUM];
        playerSeedMoney = new int[JackGameControl.MAX_PLAYER_NUM];
        playerBettingMoney = new int[JackGameControl.MAX_PLAYER_NUM, JackGameControl.MAX_SPLIT_NUM];
        playerIsBet = new bool[JackGameControl.MAX_PLAYER_NUM];
        playerIsGameEnd = new bool[JackGameControl.MAX_PLAYER_NUM, JackGameControl.MAX_SPLIT_NUM];
        playerIsInsurance = new int[JackGameControl.MAX_PLAYER_NUM];

        playerCardGO = new List<GameObject>[JackGameControl.MAX_PLAYER_NUM, JackGameControl.MAX_SPLIT_NUM];
        playerCardDetails = new List<int>[JackGameControl.MAX_PLAYER_NUM, JackGameControl.MAX_SPLIT_NUM];
        playerCardScore = new Tuple<int, int>[JackGameControl.MAX_PLAYER_NUM, JackGameControl.MAX_SPLIT_NUM];
        for (int i = 0; i < JackGameControl.MAX_PLAYER_NUM; i++)
        {
            for (int j = 0; j < JackGameControl.MAX_SPLIT_NUM; j++) 
            {
                playerCardGO[i, j] = new List<GameObject>();
                playerCardDetails[i, j] = new List<int>();
                playerCardScore[i, j] = Tuple.Create(-1, -1);
            }
        }
    }

    public void GameSetting()
    {
        playerNickName.Clear();
        for (int i = 0; i < JackGameControl.MAX_PLAYER_NUM; i++)
        {
            playerIsBet[i] = false;
            playerIsInsurance[i] = 0;
            playerSeedMoney[i] = 0;

            for(int j = 0; j < JackGameControl.MAX_SPLIT_NUM; j++)
            {
                playerBettingMoney[i, j] = 0;

                if (j == 0)
                    playerIsGameEnd[i, j] = false;
                else
                    playerIsGameEnd[i, j] = true;

                playerCardGO[i, j].Clear();
                playerCardDetails[i, j].Clear();
                playerCardScore[i, j] = Tuple.Create(-1, -1);
            }
        }

        _nowPlayerNum = 0;
    }

    public void ClearIsBet()
    {
        for (int i = 0; i < JackGameControl.MAX_PLAYER_NUM; i++)
            playerIsBet[i] = false;
    }

    public void ClearGameSetting()
    {
        for (int i = 0; i < JackGameControl.MAX_PLAYER_NUM; i++)
        {
            if (GetPlayerUID(i) == "")
                continue;

            for (int j = 0; j < JackGameControl.MAX_SPLIT_NUM; j++)
            {
                for (int k = 0; k < playerCardGO[i,j].Count; k++)
                {
                    GameObject cardGO = playerCardGO[i, j][k];

                    if (cardGO.GetPhotonView().IsMine)
                        Managers.Resource.PhotonDestroy(cardGO);
                }
            }
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

    public int GetPlayerBet(int playerIndex, int splitNum)
    {
        return playerBettingMoney[playerIndex, splitNum];
    }

    public void UpdatePlayerIsInsurance(int index, int value)
    {
        playerIsInsurance[index] = value;
    }

    public int GetPlayerIsInsurance(int index)
    {
        return playerIsInsurance[index];
    }

    public void UpdatePlayerIsGameEnd(int playerIndex, int splitNum, bool value)
    {
        playerIsGameEnd[playerIndex, splitNum] = value;

        if (value == true)
            PlayerGameEndSetting(playerIndex, splitNum);

        Debug.Log("UpdatePlayerIsGameEnd multi call?");
        if (PhotonNetwork.IsMasterClient)
            JackGameControl.Control.DetectGameEndAllPass();
    }

    public bool GetPlayerIsGameEnd(int playerIndex, int splitNum)
    {
        return playerIsGameEnd[playerIndex, splitNum];
    }

    public void UpdatePlayerBetting(int playerIndex, int splitNum, int amount)
    {
        playerBettingMoney[playerIndex, splitNum] += amount;
        JackGameControl.Control.UpdatePlayerBetMoneyUI();
    }

    public void UpdatePlayerBetReset(int playerIndex, int splitNum)
    {
        playerBettingMoney[playerIndex, splitNum] = 0;
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

    public int GetPlayerCardLen(int playerIndex, int splitNum)
    {
        return playerCardGO[playerIndex, splitNum].Count;
    }

    public void SetPlayerCard(string pUID, int splitNum, int cardViewID, int cardDetail)
    {
        int playerIndex = GetPlayerGameIndexByUID(pUID);
        int cardIndex = GetPlayerCardLen(playerIndex, splitNum);
        GameObject cardGO = PhotonView.Find(cardViewID).gameObject;

        playerCardGO[playerIndex, splitNum].Add(cardGO);
        playerCardDetails[playerIndex, splitNum].Add(cardDetail);
        Debug.Log($"PlayerIndex : {playerIndex}, Player CardIndex : {cardIndex}");

        UI_Card cardUI = cardGO.GetOrAddComponent<UI_Card>();
        cardUI.SetCardImage(cardDetail);
        Debug.Log("c");

        JackGameControl.Control.UpdatePlayerBetScoreUI(playerIndex, splitNum);
        Debug.Log("d");

        if (JackGameControl.Control.StageCount <= 10)
            JackGameControl.Control.NextStage(1);
    }

    public Tuple<int, int> CalculatePlayerBetScore(int playerIndex, int splitNum)
    {
        int[] score = new int[22];
        if (playerCardScore[playerIndex, splitNum].Item1 == -1)
        {
            score[0] = 1;
        }
        else
        {
            score[playerCardScore[playerIndex, splitNum].Item1] = 1;

            if (playerCardScore[playerIndex, splitNum].Item2 != -1)
                score[playerCardScore[playerIndex, splitNum].Item2] = 1;
        }

        int i = GetPlayerCardLen(playerIndex, splitNum) - 1;

        int cardscore = JackGameControl.Card.GetCardNum(playerCardDetails[playerIndex, splitNum][i]);
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

        playerCardScore[playerIndex, splitNum] = Tuple.Create(a, b);

        return playerCardScore[playerIndex, splitNum];
    }

    public void FindPlayerBlackJack()
    {
        if (!JackGameControl.Control.IsPlaying) return;

        for (int i = 0; i < JackGameControl.MAX_PLAYER_NUM; i++)
        {
            if (GetPlayerUID(i) == "")
                continue;

            if (playerCardScore[i, 0].Item1 == 21 || playerCardScore[i, 0].Item2 == 21)
                JackGameControl.Control.UpdatePlayerBetStatusUI(i, "BlackJack!!!");
        }

        JackGameControl.Control.NextStage();
    }

    public Tuple<int, int> GetPlayerCardScore(int playerIndex, int splitNum)
    {
        return playerCardScore[playerIndex, splitNum];
    }

    public void PlayerGameEndSetting(int playerIndex, int splitNum)
    {
        var score = GetPlayerCardScore(playerIndex, splitNum);
        bool isBlackJack = score.Item1 == 21 || score.Item2 == 21;

        int cardLen = GetPlayerCardLen(playerIndex, splitNum);
        if (cardLen > 2) isBlackJack = false;

        foreach (GameObject cardGO in playerCardGO[playerIndex, splitNum]) 
        {
            if (cardGO != null)
            {
                UI_Card card = cardGO.GetOrAddComponent<UI_Card>();

                if(isBlackJack)
                {
                    if (card.GetComponent<PhotonView>().IsMine)
                        JackGameControl.Card.CardScaleBigger(cardGO);
                }
                else
                {
                    card.UIBlockSwitch(true);
                }
            }
        }
    }

    public bool IsPlayerCanSplit(int playerIndex)
    {
        int card1 = playerCardDetails[playerIndex, JackGameControl.Control.PlayerSplit][0];
        int card2 = playerCardDetails[playerIndex, JackGameControl.Control.PlayerSplit][1];

        if (card1 >= 10)
            card1 = 10;
        if (card2 >= 10)
            card2 = 10;

        return card1 == card2;
    }

    public bool IsPlayerSplit(int playerIndex, int splitNum)
    {
        int len = playerCardGO[playerIndex, splitNum].Count;

        return len != 0;
    }
}
