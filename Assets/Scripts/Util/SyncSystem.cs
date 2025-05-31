using System;
using System.Collections.Generic;
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

    #endregion

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
        HoldemGameControl.Control.NextStage();
    }

    public void SyncHoldemDeck()
    {
        photonView.RPC("RPC_SyncHoldemDeck", RpcTarget.All, HoldemGameControl.Card.GetCardDeck());
    }

    [PunRPC]
    private void RPC_SyncHoldemDeck(int[] cardDeck)
    {
        HoldemGameControl.Card.SetCardDeck(cardDeck);
        HoldemGameControl.Control.NextStage();
    }

    public void SyncHoldemDealerIndex(int index)
    {
        photonView.RPC("RPC_SyncHoldemDealerIndex", RpcTarget.All, index);
    }

    [PunRPC]
    private void RPC_SyncHoldemDealerIndex(int index)
    {
        HoldemGameControl.Control.SetDealer(index);
        HoldemGameControl.Control.NextStage();
    }

    public void SyncHoldemPotMoney(int money)
    {
        photonView.RPC("RPC_SyncHoldemPotMoney", RpcTarget.All, money);
    }

    [PunRPC]
    private void RPC_SyncHoldemPotMoney(int money)
    {
        HoldemGameControl.Control.PotMoney = money;
        HoldemGameControl.Control.NextStage();
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

    public void HoldemNextStage(int state = 0)      // 1은 스테이지 세부 사항 카운트 증가
    {
        photonView.RPC("RPC_HoldemNextStage", RpcTarget.All, state);
    }

    [PunRPC]
    private void RPC_HoldemNextStage(int state = 0)
    {
        HoldemGameControl.Control.NextStage(state);
    }

    public void SyncCard(GameObject cardObj)
    {
//        photonView.RPC("RPC_SyncCard", RpcTarget.Others, cardObj.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    private void RPC_SyncCard()
    {

    }
}