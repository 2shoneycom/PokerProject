using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Google.MiniJSON;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;

class SyncSystem : MonoBehaviourPun
{
    /* 싱글톤 */
    private static SyncSystem instance;
    public static SyncSystem Sync
    {
        get
        {
            return instance;
        }
    }

    public Action<string[]> OnSeatsSynced;
    public Action<string, string, int> OnHaveSeat;

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

    public void HoldemBetMoneyToTarget(string uid, int amount)
    {
        photonView.RPC("RPC_HoldemBetMoneyToTarget", RpcTarget.All, uid, amount);
    }

    [PunRPC]
    private void RPC_HoldemBetMoneyToTarget(string uid, int amount)
    {
        User.NowUser.HoldemBettingMoney(uid, amount);
    }

    public void PokerBetMoneyToTarget(string uid, int amount)
    {
        photonView.RPC("RPC_PokerBetMoneyToTarget", RpcTarget.All, uid, amount);
    }

    [PunRPC]
    private void RPC_PokerBetMoneyToTarget(string uid, int amount)
    {
        User.NowUser.PokerBettingMoney(uid, amount);
    }

    public void JackBetMoneyToTarget(string uid, int amount)
    {
        photonView.RPC("RPC_JackBetMoneyToTarget", RpcTarget.All, uid, amount);
    }

    [PunRPC]
    private void RPC_JackBetMoneyToTarget(string uid, int amount)
    {
        User.NowUser.JackBettingMoney(uid, amount);
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

    public void SyncHoldemPlayerCard(string pUID, GameObject cardGO, int cardDetail)
    {
        int cardViewID = cardGO.GetComponent<PhotonView>().ViewID;
        photonView.RPC("RPC_SyncHoldemPlayerCard", RpcTarget.All, pUID, cardViewID, cardDetail);
    }

    [PunRPC]
    private void RPC_SyncHoldemPlayerCard(string pUID, int cardViewID, int cardDetail)
    {
        HoldemGameControl.Players.SetPlayerCard(pUID, cardViewID, cardDetail);
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

    public IEnumerator SyncHoldemPlayerUID()
    {
        yield return null;
        photonView.RPC("RPC_SyncHoldemPlayerUID", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_SyncHoldemPlayerUID()
    {
        Managers.Seat.HoldemConvertToPlayers();
    }

    public IEnumerator SyncHoldemDealerIndex(int index)
    {
        yield return null;
        photonView.RPC("RPC_SyncHoldemDealerIndex", RpcTarget.All, index);
    }

    [PunRPC]
    private void RPC_SyncHoldemDealerIndex(int index)
    {
        HoldemGameControl.Control.SetDealer(index);
    }

    public IEnumerator SyncHoldemPotMoney(int money, bool isNextStage = false)
    {
        yield return null;
        photonView.RPC("RPC_SyncHoldemPotMoney", RpcTarget.All, money, isNextStage);
    }

    [PunRPC]
    private void RPC_SyncHoldemPotMoney(int money, bool isNextStage)
    {
        HoldemGameControl.Control.PotMoney = money;

        if (isNextStage)
        {
            Debug.Log($"case {HoldemGameControl.Control.StageCount} 종료, nextStage");
            HoldemGameControl.Control.NextStage();
        }
    }

    public IEnumerator HoldemNextStage(int state = 0)      // 1은 스테이지 세부 사항 카운트 증가
    {
        yield return null;
        photonView.RPC("RPC_HoldemNextStage", RpcTarget.All, state);
    }

    [PunRPC]
    private void RPC_HoldemNextStage(int state = 0)
    {
        HoldemGameControl.Control.NextStage(state);
    }

    public void HoldemNextStage_V2(int state = 0)
    {
        photonView.RPC("RPC_HoldemNextStage", RpcTarget.All, state);
    }

    public IEnumerator HoldemAutoDieTimerSwitch(bool isOn)
    {
        yield return null;
        photonView.RPC("RPC_HoldemAutoDieTimerSwitch", RpcTarget.All, isOn);
    }

    [PunRPC]
    private void RPC_HoldemAutoDieTimerSwitch(bool isOn)
    {
        HoldemGameControl.Control.AutoDieTimerSwitch(isOn);
    }

    public IEnumerator HoldemClearGame()
    {
        yield return null;
        photonView.RPC("RPC_HoldemClearGame", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_HoldemClearGame()
    {
        HoldemGameControl.Control.ClearGame();
    }

    #endregion

    #region HoldemBetManager

    public IEnumerator HoldemBetStart(int curplayer)
    {
        yield return null;
        photonView.RPC("RPC_HoldemBetStart", RpcTarget.All, curplayer);
    }

    [PunRPC]
    private void RPC_HoldemBetStart(int curplayer)
    {
        HoldemGameControl.Bet.HandleBet(curplayer);
    }

    public void HoldemBetProcess(int curPlayer, string betType, int betAmount = 0)
    {
        photonView.RPC("RPC_HoldemBetProcess", RpcTarget.All, curPlayer, betType, betAmount);
    }

    [PunRPC]
    public void RPC_HoldemBetProcess(int curPlayer, string betType, int betAmount = 0)
    {
        HoldemGameControl.Bet.BetProcess(curPlayer, betType, betAmount);
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

    public IEnumerator SyncHoldemDeck()
    {
        yield return null;
        photonView.RPC("RPC_SyncHoldemDeck", RpcTarget.All, HoldemGameControl.Card.GetCardDeck());
    }

    [PunRPC]
    private void RPC_SyncHoldemDeck(int[] cardDeck)
    {
        HoldemGameControl.Card.SetCardDeck(cardDeck);
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
        photonView.RPC("RPC_SyncHoldemDealerCard", RpcTarget.All, go.GetComponent<PhotonView>().ViewID, index, cardDetail);
    }

    [PunRPC]
    private void RPC_SyncHoldemDealerCard(int viewID, int index, int cardDetail)
    {
        HoldemGameControl.Card.DealerCardSetting(viewID, index, cardDetail);
    }

    #endregion

    //////////////////////////////////////////////////////

    #region PokerGameControl

    public void PokerStartSync()
    {
        photonView.RPC("RPC_PokerStartSyncing", RpcTarget.All);
    }
    
    [PunRPC]
    private void RPC_PokerStartSyncing()
    {
        PokerGameControl.Control.StartGame();
    }

    public IEnumerator SyncPokerPlayerUID()
    {
        yield return null;
        photonView.RPC("RPC_SyncPokerPlayerUID", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_SyncPokerPlayerUID()
    {
        Managers.Seat.PokerConvertToPlayers();
    }

    public IEnumerator SyncPokerFirstPlayerIndex(int index)
    {
        yield return null;
        photonView.RPC("RPC_SyncPokerFirstPlayerIndex", RpcTarget.All, index);
    }

    [PunRPC]
    private void RPC_SyncPokerFirstPlayerIndex(int index)
    {
        PokerGameControl.Control.SetFirstPlayer(index);
    }

    public IEnumerator SyncPokerCurrentPlayer(int index)
    {
        yield return null;
        photonView.RPC("RPC_SyncPokerCurrentPlayer", RpcTarget.All, index);

    }

    [PunRPC]
    private void RPC_SyncPokerCurrentPlayer(int index)
    {
        PokerGameControl.Control.SetCurrentPlayer(index);
    }

    public IEnumerator PokerNextStage(int state = 0)      // 1은 스테이지 세부 사항 카운트 증가
    {
        yield return null;
        photonView.RPC("RPC_PokerNextStage", RpcTarget.All, state);
    }

    public void PokerNextStage_V2(int state = 0)
    {
        photonView.RPC("RPC_PokerNextStage", RpcTarget.All, state);
    }

    [PunRPC]
    private void RPC_PokerNextStage(int state = 0)
    {
        PokerGameControl.Control.NextStage(state);
    }

    public IEnumerator PokerMakeCardSelPopup()
    {
        yield return null;
        photonView.RPC("RPC_PokerMakeCardSelPopup", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PokerMakeCardSelPopup()
    {
        PokerGameControl.Control.CardSelPopupOn();
    }

    public IEnumerator SyncPokerPotMoney(int money, int isNextStage = 0)
    {
        yield return null;
        photonView.RPC("RPC_SyncPokerPotMoney", RpcTarget.All, money, isNextStage);
    }

    [PunRPC]
    private void RPC_SyncPokerPotMoney(int money, int isNextStage = 0)
    {
        PokerGameControl.Control.PotMoney = money;

        if (isNextStage == 0)
        {
            Debug.Log($"case {PokerGameControl.Control.StageCount} 종료, nextStage");
            PokerGameControl.Control.NextStage();
        }
        else if (isNextStage == 1)
        {
            Debug.Log($"case {PokerGameControl.Control.StageCount} 종료, nextStage");
            PokerGameControl.Control.NextStage(1);
        }
    }

    public IEnumerator PokerAutoDieTimerSwitch(bool isOn)
    {
        yield return null;
        photonView.RPC("RPC_PokerAutoDieTimerSwitch", RpcTarget.All, isOn);
    }

    [PunRPC]
    private void RPC_PokerAutoDieTimerSwitch(bool isOn)
    {
        PokerGameControl.Control.AutoDieTimerSwitch(isOn);
    }

    public IEnumerator PokerClearGame()
    {
        yield return null;
        photonView.RPC("RPC_PokerClearGame", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PokerClearGame()
    {
        PokerGameControl.Control.ClearGame();
    }

    #endregion

    #region PokerCardManager

    public IEnumerator SyncPokerDeck()
    {
        yield return null;
        photonView.RPC("RPC_SyncPokerDeck", RpcTarget.All, PokerGameControl.Card.GetCardDeck());
    }

    [PunRPC]
    private void RPC_SyncPokerDeck(int[] cardDeck)
    {
        PokerGameControl.Card.SetCardDeck(cardDeck);
    }

    public void PokerAddCard(string toPlayer)
    {
        photonView.RPC("RPC_PokerAddCard", RpcTarget.All, toPlayer);
    }

    [PunRPC]
    private void RPC_PokerAddCard(string toPlayer)
    {
        PokerGameControl.Card.AddCardToPlayerStarter(toPlayer);
    }


    #endregion

    #region PokerPlayerManager

    public void SyncPokerMyBetting(int index, int amount)
    {
        photonView.RPC("RPC_SyncPokerMyBetting", RpcTarget.All, index, amount);
    }

    [PunRPC]
    private void RPC_SyncPokerMyBetting(int index, int amount)
    {
        PokerGameControl.Players.UpdatePlayerBetting(index, amount);
    }

    public void SyncPokerPlayerCard(string pUID, GameObject cardGO, int cardDetail, bool isOpenCard)
    {
        int cardViewID = cardGO.GetComponent<PhotonView>().ViewID;
        photonView.RPC("RPC_SyncPokerPlayerCard", RpcTarget.All, pUID, cardViewID, cardDetail, isOpenCard);
    }

    [PunRPC]
    private void RPC_SyncPokerPlayerCard(string pUID, int cardViewID, int cardDetail, bool isOpenCard)
    {
        PokerGameControl.Players.SetPlayerCard(pUID, cardViewID, cardDetail, isOpenCard);
    }

    public void SyncPokerPlayerCardSel(int playerIndex, int delcardIndex, int opencardIndex)
    {
        photonView.RPC("RPC_SyncPokerPlayerCardSel", RpcTarget.All, playerIndex, delcardIndex, opencardIndex);
    }

    [PunRPC]
    private void RPC_SyncPokerPlayerCardSel(int playerIndex, int delcardIndex, int opencardIndex)
    {
        Debug.Log($"Player {playerIndex}, del : {delcardIndex}, open : {opencardIndex}");
        PokerGameControl.Players.PlayerDelCardSel(playerIndex, delcardIndex);
        PokerGameControl.Players.PlayerOpenCardSel(playerIndex, opencardIndex);
    }

    public IEnumerator PokerArrangeSelectedCard()
    {
        yield return null;
        photonView.RPC("RPC_PokerArrangeSelectedCard", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PokerArrangeSelectedCard()
    {
        PokerGameControl.Players.ArrangeSelectedCard();
    }

    public void SyncPokerIsTurn(int index, bool isOn)
    {
        photonView.RPC("RPC_SyncPokerIsTurn", RpcTarget.All, index, isOn);
    }

    [PunRPC]
    private void RPC_SyncPokerIsTurn(int index, bool isOn)
    {
        PokerGameControl.Players.UpdatePlayerTurn(index, isOn);
    }

    public void SyncPokerDieReserve(int index, bool isOn)
    {
        photonView.RPC("RPC_SyncPokerDieReserve", RpcTarget.All, index, isOn);
    }

    [PunRPC]
    private void RPC_SyncPokerDieReserve(int index, bool isOn)
    {
        PokerGameControl.Players.UpdatePlayerDieReserve(index, isOn);
    }

    public void SyncPokerIsOneLeft(bool isOn)
    {
        photonView.RPC("RPC_SyncPokerIsOneLeft", RpcTarget.All, isOn);
    }

    [PunRPC]
    private void RPC_SyncPokerIsOneLeft(bool isOn)
    {
        PokerGameControl.Players.IsOneLeft = isOn;
    }

    public void SyncPokerPlayerSeedMoney(int index, int amount)
    {
        photonView.RPC("RPC_SyncPokerPlayerSeedMoney", RpcTarget.All, index, amount);
    }

    [PunRPC]
    private void RPC_SyncPokerPlayerSeedMoney(int index, int amount)
    {
        PokerGameControl.Players.UpdatePlayerSeedMoney(index, amount);
    }

    public void SyncPokerWinnerList(string[] wList)
    {
        string json = Json.Serialize(wList);
        photonView.RPC("RPC_SyncPokerWinnerList", RpcTarget.All, json);
    }

    [PunRPC]
    private void RPC_SyncPokerWinnerList(string json)
    {
        List<object> wListRaw = Json.Deserialize(json) as List<object>;
        string[] wList = wListRaw.ConvertAll(obj => obj.ToString()).ToArray();
        PokerGameControl.Players.SetWinnerList(wList);
    }


    #endregion

    #region PokerBetManager

    public IEnumerator PokerBetStart(int curplayer)
    {
        yield return null;
        photonView.RPC("RPC_PokerBetStart", RpcTarget.All, curplayer);
    }

    [PunRPC]
    private void RPC_PokerBetStart(int curplayer)
    {
        PokerGameControl.Bet.HandleBet(curplayer);
    }

    public void PokerBetProcess(int curPlayer, string betType, int betAmount = 0)
    {
        photonView.RPC("RPC_PokerBetProcess", RpcTarget.All, curPlayer, betType, betAmount);
    }

    [PunRPC]
    public void RPC_PokerBetProcess(int curPlayer, string betType, int betAmount = 0)
    {
        PokerGameControl.Bet.BetProcess(curPlayer, betType, betAmount);
    }

    public void PokerBetEnd()
    {
        photonView.RPC("RPC_PokerBetEnd", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PokerBetEnd()
    {
        PokerGameControl.Bet.CurrentStageBetEnd();
    }


    #endregion

    //////////////////////////////////////////////////////

    #region JackGameControl

    public void JackStartSync()
    {
        photonView.RPC("RPC_JackStartSync", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackStartSync()
    {
        JackGameControl.Control.StartGame();
    }

    public IEnumerator SyncJackPlayerUID()
    {
        yield return null;
        photonView.RPC("RPC_SyncJackPlayerUID", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_SyncJackPlayerUID()
    {
        Managers.Seat.JackConvertToPlayers();
    }

    public IEnumerator SyncJackFirstPlayerIndex(int index)
    {
        yield return null;
        photonView.RPC("RPC_SyncJackFirstPlayerIndex", RpcTarget.All, index);
    }

    [PunRPC]
    private void RPC_SyncJackFirstPlayerIndex(int index)
    {
        JackGameControl.Control.SetFirstPlayer(index);
    }

    public IEnumerator StartFirstBetting()
    {
        yield return null;
        photonView.RPC("RPC_StartFirstBetting", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartFirstBetting()
    {
        JackGameControl.Control.StartFirstBet();
    }

    public void FirstBettingAllPass()
    {
        photonView.RPC("RPC_FirstBettingAllPass", RpcTarget.All);

    }

    [PunRPC]
    private void RPC_FirstBettingAllPass()
    {
        JackGameControl.Control.FirstBetAllPass();
    }

    public IEnumerator JackNextStage(int state = 0)      // 1은 스테이지 세부 사항 카운트 증가
    {
        yield return null;
        photonView.RPC("RPC_JackNextStage", RpcTarget.All, state);
    }

    public void JackNextStage_V2(int state = 0)
    {
        photonView.RPC("RPC_JackNextStage", RpcTarget.All, state);
    }

    [PunRPC]
    private void RPC_JackNextStage(int state = 0)
    {
        JackGameControl.Control.NextStage(state);
    }

    public IEnumerator JackNoticeBlackJack()
    {
        yield return null;
        photonView.RPC("RPC_JackNoticeBlackJack", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackNoticeBlackJack()
    {
        JackGameControl.Players.FindPlayerBlackJack();
    }

    public IEnumerator JackIsDealerIsA()
    {
        yield return null;
        photonView.RPC("RPC_JackIsDealerIsA", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackIsDealerIsA()
    {
        JackGameControl.Control.JudgeDealerIsAOrAbove10();
    }

    public void JackInsuranceAllPass()
    {
        photonView.RPC("RPC_JackInsuranceAllPass", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackInsuranceAllPass()
    {
        JackGameControl.Control.InsuranceAllPass();
    }

    public void JackGameEnd()
    {
        photonView.RPC("RPC_JackGameEnd", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackGameEnd()
    {
        JackGameControl.Control.ClearGame();
    }

    public IEnumerator JackNormalBetting(int playerIndex)
    {
        yield return new WaitForSeconds(1f);
        photonView.RPC("RPC_JackNormalBetting", RpcTarget.All, playerIndex);
    }

    [PunRPC]
    private void RPC_JackNormalBetting(int playerIndex)
    {
        JackGameControl.Control.PlayerNormalBetSetting(playerIndex);
    }

    public IEnumerator JackBlackJackPlayerWin(int playerIndex)
    {
        yield return null;
        photonView.RPC("RPC_JackBlackJackPlayerWin", RpcTarget.All, playerIndex);
    }

    [PunRPC]
    private void RPC_JackBlackJackPlayerWin(int playerIndex)
    {
        JackGameControl.Control.BlackJackPlayerWin(playerIndex);
    }

    public void JackNormalBetEnd()
    {
        photonView.RPC("RPC_JackNormalBetEnd", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackNormalBetEnd()
    {
        JackGameControl.Control.PlayerNormalBetEnd();
    }

    public void JackRestartBetTimer()
    {
        photonView.RPC("RPC_JackRestartBetTimer", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackRestartBetTimer()
    {
        JackGameControl.Control.RestartBetTimer();
    }


    #endregion

    #region JackCardManager

    public IEnumerator SyncJackDeck()
    {
        yield return null;
        photonView.RPC("RPC_SyncJackDeck", RpcTarget.All, JackGameControl.Card.GetCardDeck());
    }

    [PunRPC]
    private void RPC_SyncJackDeck(int[] cardDeck)
    {
        JackGameControl.Card.SetCardDeck(cardDeck);
    }

    public void JackAddCard(string toPlayer, int splitNum)
    {
        photonView.RPC("RPC_JackAddCard", RpcTarget.All, toPlayer, splitNum);
    }

    [PunRPC]
    private void RPC_JackAddCard(string toPlayer, int splitNum)
    {
        JackGameControl.Card.AddCardToPlayerStarter(toPlayer, splitNum);
    }

    public void JackDealerCard()
    {
        photonView.RPC("RPC_JackDealerCard", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackDealerCard()
    {
        JackGameControl.Card.AddCardToDealerStarter();
    }

    public void SyncJackDealerCard(GameObject cardGO, int index, int cardDetail)
    {
        int cardViewID = cardGO.GetComponent<PhotonView>().ViewID;
        photonView.RPC("RPC_SyncJackDealerCard", RpcTarget.All, cardViewID, index, cardDetail);
    }

    [PunRPC]
    private void RPC_SyncJackDealerCard(int viewID, int index, int cardDetail)
    {
        JackGameControl.Card.SetDealerCard(viewID, index, cardDetail);
    }

    #endregion

    #region JackPlayerManager

    public void SyncJackPlayerSeedMoney(int index, int amount)
    {
        photonView.RPC("RPC_SyncJackPlayerSeedMoney", RpcTarget.All, index, amount);
    }

    [PunRPC]
    private void RPC_SyncJackPlayerSeedMoney(int index, int amount)
    {
        JackGameControl.Players.UpdatePlayerSeedMoney(index, amount);
    }

    public void SyncJackMyBetting(int playerIndex, int splitNum, int amount)
    {
        photonView.RPC("RPC_SyncJackMyBetting", RpcTarget.All, playerIndex, splitNum, amount);
    }

    [PunRPC]
    private void RPC_SyncJackMyBetting(int playerIndex, int splitNum, int amount)
    {
        JackGameControl.Players.UpdatePlayerBetting(playerIndex, splitNum, amount);
    }

    public void SyncJackMyBettingReset(int playerIndex, int splitNum)
    {
        photonView.RPC("RPC_SyncJackMyBettingReset", RpcTarget.All, playerIndex, splitNum);
    }

    [PunRPC]
    private void RPC_SyncJackMyBettingReset(int playerIndex, int splitNum)
    {
        JackGameControl.Players.UpdatePlayerBetReset(playerIndex, splitNum);
    }

    public void SyncJackIsBet(int index, bool val)
    {
        photonView.RPC("RPC_SyncJackIsBet", RpcTarget.All, index, val);
    }

    [PunRPC]
    private void RPC_SyncJackIsBet(int index, bool val)
    {
        JackGameControl.Players.UpdatePlayerIsBet(index, val);
    }

    public void SyncJackIsGameEnd(int playerIndex, int splitNum, bool val)
    {
        photonView.RPC("RPC_SyncJackIsGameEnd", RpcTarget.All, playerIndex, splitNum, val);
    }

    [PunRPC]
    private void RPC_SyncJackIsGameEnd(int playerIndex, int splitNum, bool val)
    {
        JackGameControl.Players.UpdatePlayerIsGameEnd(playerIndex, splitNum, val);
    }

    public void SyncJackPlayerCard(string pUID, GameObject cardGO, int cardDetail)
    {
        int cardViewID = cardGO.GetComponent<PhotonView>().ViewID;
        photonView.RPC("RPC_SyncJackPlayerCard", RpcTarget.All, pUID, cardViewID, cardDetail);
    }

    [PunRPC]
    private void RPC_SyncJackPlayerCard(string pUID, int cardViewID, int cardDetail)
    {
        JackGameControl.Card.SetPlayerCard(pUID, cardViewID, cardDetail);
    }

    public void SyncJackIsInsurance(int index, int val)
    {
        photonView.RPC("RPC_SyncJackIsInsurance", RpcTarget.All, index, val);
    }

    [PunRPC]
    private void RPC_SyncJackIsInsurance(int index, int val)
    {
        JackGameControl.Players.UpdatePlayerIsInsurance(index, val);
    }


    #endregion

    #region JackBetManager


    #endregion


    //////////////////////////////////////////////////////

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

    public void SyncHaveSeat(string uid, string nickname, int seatIndex)
    {
        photonView.RPC("RPC_HaveSeat", RpcTarget.All, uid, nickname, seatIndex);
    }

    [PunRPC]
    private void RPC_HaveSeat(string uid, string nickname, int seatIndex)
    {
        OnHaveSeat?.Invoke(uid, nickname, seatIndex);
    }

    #endregion



    public IEnumerator SyncHoldemResultUI()
    {
        yield return null;
        photonView.RPC("RPC_SyncHoldemResultUI", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_SyncHoldemResultUI()
    {
        HoldemGameControl.Control.ShowResult();
    }

    public IEnumerator SyncPokerResultUI()
    {
        yield return null;
        photonView.RPC("RPC_SyncPokerResultUI", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_SyncPokerResultUI()
    {
        PokerGameControl.Control.ShowResult();
    }

}