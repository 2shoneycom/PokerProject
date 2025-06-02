using System;
using System.Collections.Generic;
using Google.MiniJSON;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

class SyncSystem : MonoBehaviourPun
{
    /* 싱글톤 */
    private static SyncSystem instance;
    public static SyncSystem Instacne
    {
        get
        {
            return instance;
        }
    }

    public Action<string[]> OnSeatsSynced;
    public Action<string, int> OnHaveSeat;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // 씬 안에서 중복 생성 방지
        }
    }

    #region UserData
    public void DecreaseMoneyToTarget(string uid, int amount)
    {
        photonView.RPC("RPC_DecreaseMoneyToTarget", RpcTarget.All, uid, amount);
    }

    [PunRPC]
    private void RPC_DecreaseMoneyToTarget(string uid, int amount)
    {
        User.NowUser.DecreaseMoney(uid, amount);
    }

    public void IncreaseMoneyToTarget(string uid, int amount)
    {
        photonView.RPC("RPC_IncreaseMoneyToTarget", RpcTarget.All, uid, amount);
    }

    [PunRPC]
    private void RPC_IncreaseMoneyToTarget(string uid, int amount)
    {
        User.NowUser.IncreaseMoney(uid, amount);
    }

    public void HoldemBetMoneyToTarget(string uid, int amount)
    {
        photonView.RPC("RPC_HoldemBetMoneyToTarget", RpcTarget.All, uid, amount);
    }

    [PunRPC]
    private void RPC_HoldemBetMoneyToTarget(string uid, int amount)
    {
        User.NowUser.HoldemBettingMoney(uid, amount);
    }

    #endregion

    #region HoldemPlayerManager

    public void SyncHoldemMyBetting(int index, int amount)
    {
        photonView.RPC("RPC_SyncHoldemMyBetting", RpcTarget.All, index, amount);
    }

    [PunRPC]
    private void RPC_SyncHoldemMyBetting(int index, int amount)
    {
        HoldemGameControl.Players.UpdatePlayerBetting(index, amount);
    }

    public void SyncHoldemPlayerIsBet(int index, bool isOn)
    {
        photonView.RPC("RPC_SyncHoldemPlayerIsBet", RpcTarget.All, index, isOn);
    }

    [PunRPC]
    private void RPC_SyncHoldemPlayerIsBet(int index, bool isOn)
    {
        HoldemGameControl.Players.UpdatePlayerIsBet(index, isOn);
    }

    public void SyncHoldemPlayerIsAlive(int index, bool isOn)
    {
        photonView.RPC("RPC_SyncHoldemPlayerIsAlive", RpcTarget.All, index, isOn);
    }

    [PunRPC]
    private void RPC_SyncHoldemPlayerIsAlive(int index, bool isOn)
    {
        HoldemGameControl.Players.UpdatePlayerState(index, isOn);
    }

    public void SyncHoldemIsTurn(int index, bool isOn)
    {
        photonView.RPC("RPC_SyncHoldemIsTurn", RpcTarget.All, index, isOn);
    }

    [PunRPC]
    private void RPC_SyncHoldemIsTurn(int index, bool isOn)
    {
        HoldemGameControl.Players.UpdatePlayerTurn(index, isOn);
    }

    public void SyncHoldemPlayerSeedMoney(int index, int amount)
    {
        photonView.RPC("RPC_SyncHoldemPlayerSeedMoney", RpcTarget.All, index, amount);
    }

    [PunRPC]
    private void RPC_SyncHoldemPlayerSeedMoney(int index, int amount)
    {
        HoldemGameControl.Players.UpdatePlayerSeedMoney(index, amount);
    }

    public void SyncHoldemDieReserve(int index, bool isOn)
    {
        photonView.RPC("RPC_SyncHoldemDieReserve", RpcTarget.All, index, isOn);
    }

    [PunRPC]
    private void RPC_SyncHoldemDieReserve(int index, bool isOn)
    {
        HoldemGameControl.Players.UpdatePlayerDieReserve(index, isOn);
    }

    public void SyncHoldemDeadPlayerNum(int num)
    {
        photonView.RPC("RPC_SyncHoldemDeadPlayerNum", RpcTarget.All, num);
    }

    [PunRPC]
    private void RPC_SyncHoldemDeadPlayerNum(int num)
    {
        HoldemGameControl.Players.SetDeadPlayerNum(num);
    }

    public void SyncHoldemIsOneLeft(bool isOn)
    {
        photonView.RPC("RPC_SyncHoldemIsOneLeft", RpcTarget.All, isOn);
    }

    [PunRPC]
    private void RPC_SyncHoldemIsOneLeft(bool isOn)
    {
        HoldemGameControl.Players.IsOneLeft = isOn;
    }

    public void SyncHoldemPlayerCardDetails(int index, int card1, int card2)
    {
        photonView.RPC("RPC_SyncHoldemPlayerCardDetails", RpcTarget.All, index, card1, card2);
    }

    [PunRPC]
    private void RPC_SyncHoldemPlayerCardDetails(int index, int card1, int card2)
    {
        HoldemGameControl.Players.SetPlayerCardDetails(index, card1, card2);
    }

    public void SyncHoldemWinnerList(string[] wList)
    {
        string json = Json.Serialize(wList);
        photonView.RPC("RPC_SyncHoldemWinnerList", RpcTarget.All, json);
    }

    [PunRPC]
    private void RPC_SyncHoldemWinnerList(string json)
    {
        List<object> wListRaw = Json.Deserialize(json) as List<object>;
        string[] wList = wListRaw.ConvertAll(obj => obj.ToString()).ToArray();
        HoldemGameControl.Players.SetWinnerList(wList);
    }

    #endregion

    #region HoldemGameControl

    public void HoldemStartSync()
    {
        photonView.RPC("RPC_HoldemStartSyncing", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_HoldemStartSyncing()
    {
        HoldemGameControl.Control.StartGame();
    }

    public void SyncHoldemStageCount(int count)
    {
        photonView.RPC("RPC_SyncHoldemStageCount", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_SyncHoldemStageCount(int count)
    {
        HoldemGameControl.Control.StageCount = count;
    }

    public void SyncHoldemPlayerUID()
    {
        photonView.RPC("RPC_SyncHoldemPlayerUID", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_SyncHoldemPlayerUID()
    {
        Managers.Seat.ConverToPlayers();
    }
    public void SyncHoldemPotMoney(int money)
    {
        photonView.RPC("RPC_SyncHoldemPotMoney", RpcTarget.All, money);
    }

    [PunRPC]
    private void RPC_SyncHoldemPotMoney(int money)
    {
        HoldemGameControl.Control.PotMoney = money;
    }

    public void HoldemNextStage(int state = 0)      // 1은 스테이지 세부 사항 카운트 증가
    {
        photonView.RPC("RPC_HoldemNextStage", RpcTarget.All, state);
    }

    [PunRPC]
    private void RPC_HoldemNextStage(int state = 0)
    {
        HoldemGameControl.Control.NextStage(state);
    }

    public void HoldemAutoDieTimerSwitch(bool isOn)
    {
        photonView.RPC("RPC_HoldemAutoDieTimerSwitch", RpcTarget.All, isOn);
    }

    [PunRPC]
    private void RPC_HoldemAutoDieTimerSwitch(bool isOn)
    {
        HoldemGameControl.Control.AutoDieTimerSwitch(isOn);
    }

    public void RequestPlayerCardDetail()
    {
        photonView.RPC("RPC_RequestPlayerCardDetail", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_RequestPlayerCardDetail()
    {
        User.NowHoldemPlayer.GiveMyCardDetailToOthers();
    }

    #endregion

    #region HoldemBetManager

    public void HoldemBetStart(int curplayer)
    {
        photonView.RPC("RPC_HoldemBetStart", RpcTarget.All, curplayer);
    }

    [PunRPC]
    private void RPC_HoldemBetStart(int curplayer)
    {
        HoldemGameControl.Bet.HandleBet(curplayer);
    }

    public void HoldemBetProcess(int curPlayer, string betType)
    {
        photonView.RPC("RPC_HoldemBetProcess", RpcTarget.MasterClient, curPlayer, betType);
    }

    [PunRPC]
    public void RPC_HoldemBetProcess(int curPlayer, string betType)
    {
        HoldemGameControl.Bet.BetProcess(curPlayer, betType);
    }

    public void HoldemBetEnd()
    {
        photonView.RPC("RPC_HoldemBetEnd", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_HoldemBetEnd()
    {
        HoldemGameControl.Bet.CurrentStageBetEnd();
    }

    #endregion

    #region HoldemCardManager

    public void SyncHoldemDeck()
    {
        photonView.RPC("RPC_SyncHoldemDeck", RpcTarget.All, HoldemGameControl.Card.GetCardDeck());
    }

    [PunRPC]
    private void RPC_SyncHoldemDeck(int[] cardDeck)
    {
        HoldemGameControl.Card.SetCardDeck(cardDeck);
    }

    public void SyncHoldemDealerIndex(int index)
    {
        photonView.RPC("RPC_SyncHoldemDealerIndex", RpcTarget.All, index);
    }

    [PunRPC]
    private void RPC_SyncHoldemDealerIndex(int index)
    {
        HoldemGameControl.Control.SetDealer(index);
    }

    public void HoldemAddCard(string toPlayer)
    {
        photonView.RPC("RPC_HoldemAddCard", RpcTarget.All, toPlayer);
    }

    [PunRPC]
    private void RPC_HoldemAddCard(string toPlayer)
    {
        HoldemGameControl.Card.AddCardToPlayerStarter(toPlayer);
    }

    public void HoldemDealerCard()
    {
        photonView.RPC("RPC_HoldemDealerCard", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_HoldemDealerCard()
    {
        HoldemGameControl.Card.AddCardToDealerStarter();
    }

    public void SyncHoldemDealerCard(GameObject go, int index, int cardDetail)
    {
        photonView.RPC("RPC_SyncHoldemDealerCard", RpcTarget.Others, go.GetComponent<PhotonView>().ViewID, index, cardDetail);
    }

    [PunRPC]
    private void RPC_SyncHoldemDealerCard(int viewID, int index, int cardDetail)
    {
        HoldemGameControl.Card.DealerCardSetting(viewID, index, cardDetail);
    }

    #endregion

    #region SeatManager

    public void SyncSeatsToMaster()
    {
        photonView.RPC("GetSeatsDataFromMaster", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
    }

    [PunRPC]
    private void GetSeatsDataFromMaster(Player requester)
    {
        string[] currentSeats = Managers.Seat.Seats.ToArray();
        photonView.RPC("ReceiveSeatsData", requester, currentSeats);
    }

    [PunRPC]
    private void ReceiveSeatsData(string[] seats)
    {
        OnSeatsSynced?.Invoke(seats);
    }

    public void SyncHaveSeat(string uid, int seatIndex)
    {
        photonView.RPC("RPC_HaveSeat", RpcTarget.All, uid, seatIndex);
    }

    [PunRPC]
    private void RPC_HaveSeat(string uid, int seatIndex)
    {
        OnHaveSeat?.Invoke(uid, seatIndex);
    }

    #endregion

    public void SyncHoldemResultUI(bool isOn)
    {
        photonView.RPC("RPC_SyncHoldemResultUI", RpcTarget.All, isOn);
    }

    [PunRPC]
    private void RPC_SyncHoldemResultUI(bool isOn)
    {
        HoldemGameControl.Control.ShowResult(isOn);
    }
}