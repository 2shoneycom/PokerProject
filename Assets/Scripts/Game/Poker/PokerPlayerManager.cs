using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PokerPlayerManager
{
    PokerGameControl _control;

    Dictionary<string, string> playerNickName;
    string[] pokerPlayerUID;
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

    List<GameObject>[] playerCardGO;
    int[,] playerCardDetails;
    int[,] playerCardSels;
    public int[,] PlayerCards { get { return playerCardDetails; } }

    List<string> winnerList;    // UI 용도

    public PokerPlayerManager(PokerGameControl control)
    {
        _control = control;

        playerNickName = new Dictionary<string, string>();
        pokerPlayerUID = new string[PokerGameControl.MAX_PLAYER_NUM];
        playerSeedMoney = new int[PokerGameControl.MAX_PLAYER_NUM];
        playerBettingMoney = new int[PokerGameControl.MAX_PLAYER_NUM];
        playerIsBet = new bool[PokerGameControl.MAX_PLAYER_NUM];
        playerIsAlive = new bool[PokerGameControl.MAX_PLAYER_NUM];
        playerIsTurn = new bool[PokerGameControl.MAX_PLAYER_NUM];
        playerDieReserve = new bool[PokerGameControl.MAX_PLAYER_NUM];

        playerCardDetails = new int[PokerGameControl.MAX_PLAYER_NUM, PokerCardManager.PLAYER_CARD_NUM];
        playerCardSels = new int[PokerGameControl.MAX_PLAYER_NUM, 2];
        playerCardGO = new List<GameObject>[PokerGameControl.MAX_PLAYER_NUM];
        for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
            playerCardGO[i] = new List<GameObject>();

        winnerList = new List<string>();
    }

    public void GameSetting()
    {
        playerNickName.Clear();
        for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
        {
            playerBettingMoney[i] = 0;
            playerIsBet[i] = false;
            playerIsAlive[i] = true;
            playerIsTurn[i] = false;
            playerDieReserve[i] = false;
            playerSeedMoney[i] = 0;

            playerCardGO[i].Clear();
            for (int j = 0; j < PokerCardManager.PLAYER_CARD_NUM; j++)
            {
                playerCardGO[i].Add(null);
                playerCardDetails[i, j] = 0;
            }

            playerCardSels[i, 0] = -1;
            playerCardSels[i, 1] = -1;
        }
        winnerList.Clear();

        deadPlayerNum = 0;
        _nowPlayerNum = 0;
        isOneLeft = false;
    }

    public void ClearBetSetting()
    {
        for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
        {
            playerIsBet[i] = false;
            playerBettingMoney[i] = 0;
            playerIsTurn[i] = false;
        }

        User.NowGamePlayer.SetBetMoney(0);
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

        int gameIndex = _control.ConvertUItoGame(seatIdx);
        pokerPlayerUID[gameIndex] = UID;
    }

    public string GetPlayerNickNameByUID(string pUID)
    {
        return playerNickName[pUID];
    }

    public int GetPlayerGameIndexByUID(string pUID)
    {
        for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
        {
            if (pUID == GetPlayerUID(i))
                return i;
        }
        return -1;
    }

    public string GetPlayerUID(int index)
    {
        return pokerPlayerUID[index];
    }

    public int GetPlayerSeedMoney(int index)
    {
        return playerSeedMoney[index];
    }

    public void UpdatePlayerSeedMoney(int index, int seedMoney)
    {
        playerSeedMoney[index] = seedMoney;
        _control.UpdatePlayerSeedMoneyUI();
    }

    public int GetPlayerBet(int index)
    {
        return playerBettingMoney[index];
    }

    public void UpdatePlayerBetting(int index, int amount, bool isCall = false)
    {
        playerBettingMoney[index] += amount;
        playerIsBet[index] = isCall;
        _control.UpdatePlayerBetMoneyUI();
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

    public void SetPlayerCard(string pUID, int cardViewID, int cardDetail, bool isOpenCard)
    {
        int playerIndex = GetPlayerGameIndexByUID(pUID);
        int cardIndex = _control.CardLen;
        GameObject cardGO = PhotonView.Find(cardViewID).gameObject;

        playerCardGO[playerIndex][cardIndex] = cardGO;
        playerCardDetails[playerIndex, cardIndex] = cardDetail;
        Debug.Log($"PlayerIndex : {playerIndex}, Player CardIndex : {cardIndex}");
        if (User.NowUser.GetUid() == pUID || isOpenCard)
            cardGO.GetComponent<SpriteRenderer>().sprite = _control.Card.GetRightCardImage(cardDetail);
    }

    public void PlayerDelCardSel(int playerIndex, int cardIndex)
    {
        playerCardSels[playerIndex, 0] = cardIndex;
    }

    public void PlayerOpenCardSel(int playerIndex, int cardIndex)
    {
        playerCardSels[playerIndex, 1] = cardIndex;
    }

    public bool IsEveryoneSel()
    {
        for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
        {
            if (GetPlayerState(i) == false || GetPlayerUID(i) == "")
                continue;

            if (playerCardSels[i, 0] == -1 || playerCardSels[i, 1] == -1)
                return false;
        }
        return true;
    }

    public void ArrangeSelectedCard()
    {
        if (!_control.IsPlaying) return;

        for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
        {
            if (pokerPlayerUID[i] == "") continue;

            int[] privateCardIndex = new int[2];
            privateCardIndex[0] = -1;

            int delCardIndex = playerCardSels[i, 0];
            int openCardIndex = playerCardSels[i, 1];

            GameObject[] copyGO = new GameObject[4];
            int[] copyDetail = new int[4];

            for (int j = 0; j < 4; j++)
            {
                copyGO[j] = playerCardGO[i][j];
                copyDetail[j] = playerCardDetails[i, j];
                if (j == delCardIndex || j == openCardIndex) continue;

                if (privateCardIndex[0] == -1) privateCardIndex[0] = j;
                else privateCardIndex[1] = j;
            }
            Debug.Log($"Player {i} private card : {privateCardIndex[0]}, private card : {privateCardIndex[1]}");
            Debug.Log($"Player {i} delete card : {delCardIndex}, open card : {openCardIndex}");
            if (PhotonNetwork.IsMasterClient)
                Managers.Resource.PhotonDestroy(playerCardGO[i][delCardIndex]);

            playerCardGO[i][0] = copyGO[privateCardIndex[0]];
            playerCardDetails[i, 0] = copyDetail[privateCardIndex[0]];
            if (PhotonNetwork.IsMasterClient)
                _control.Card.CardMoveToPos(playerCardGO[i][0], i, 0);

            playerCardGO[i][1] = copyGO[privateCardIndex[1]];
            playerCardDetails[i, 1] = copyDetail[privateCardIndex[1]];
            if (PhotonNetwork.IsMasterClient)
                _control.Card.CardMoveToPos(playerCardGO[i][1], i, 1);

            playerCardGO[i][2] = copyGO[openCardIndex];
            playerCardDetails[i, 2] = copyDetail[openCardIndex];
            if (PhotonNetwork.IsMasterClient)
                _control.Card.CardMoveToPos(playerCardGO[i][2], i, 2);
            SetCardOpen(i, 2);
        }

        _control.NextStage();
    }

    void SetCardOpen(int playerIndex, int cardIndex)
    {
        GameObject cardGO = playerCardGO[playerIndex][cardIndex];
        int cardDetail = playerCardDetails[playerIndex, cardIndex];

        cardGO.GetComponent<SpriteRenderer>().sprite = _control.Card.GetRightCardImage(cardDetail);
    }

    public int GetOriginPlayerMoney(int playerIndex)
    {
        return GetPlayerBet(playerIndex) + GetPlayerSeedMoney(playerIndex);
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

    public int GetLowestPlayerSeedMoney()
    {
        int min_bet = int.MaxValue;
        for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
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
        int max_bet = 0;
        for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
        {
            if (GetPlayerState(i) == false || GetPlayerUID(i) == "")
                continue;

            if (playerBettingMoney[i] > max_bet)
                max_bet = playerBettingMoney[i];
        }
        return max_bet;
    }

    public int GetPlayerCardDetail(int playerIndex, int cardIndex)
    {
        return playerCardDetails[playerIndex, cardIndex];
    }

    public void ShowPlayerCard()
    {
        for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
        {
            if (GetPlayerUID(i) == "" || playerIsAlive[i] == false)
                continue;

            SetCardOpen(i, 0);
            SetCardOpen(i, 1);
            SetCardOpen(i, 7);
        }
    }

    public void ClearGameSetting()
    {
        for (int i = 0; i < PokerGameControl.MAX_PLAYER_NUM; i++)
        {
            if (GetPlayerUID(i) == "")
                continue;

            for (int j = 0; j < PokerCardManager.PLAYER_CARD_NUM; j++)
            {
                if (j == 3) continue;

                GameObject cardGO = playerCardGO[i][j];

                if (cardGO.GetPhotonView().IsMine)
                    Managers.Resource.PhotonDestroy(cardGO);
            }
        }
    }

    //public void GiveHoldemPlayerManagerSyncData(Player newPlayer)
    //{

    //}
}
