using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Google.MiniJSON;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SyncSystem : MonoBehaviourPun
{
    public void SetHoldemControl(HoldemGameControl holdemControl)
    {
        _holdemControl = holdemControl;
    }

    public void SetPokerControl(PokerGameControl pokerControl)
    {
        _pokerControl = pokerControl;
    }

    public void SetJackControl(JackGameControl jackControl)
    {
        _jackControl = jackControl;
    }

    HoldemGameControl _holdemControl;
    PokerGameControl _pokerControl;
    JackGameControl _jackControl;

    public Action<string[]> OnSeatsSynced;
    public Action<string, string, int> OnHaveSeat;

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
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 IncreaseMoneyToTarget 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        photonView.RPC("RPC_IncreaseMoneyToTarget", RpcTarget.All, uid, amount);
    }

    [PunRPC]
    private void RPC_IncreaseMoneyToTarget(string uid, int amount)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_IncreaseMoneyToTarget 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        User.NowUser.IncreaseMoney(uid, amount);
    }

    public void HoldemBetMoneyToTarget(string uid, int amount)
    {
        Debug.Log("HoldemBetManager.cs 파일의 BaseBetting 함수로부터"); // 디버깅 추적용 (25.11.12 승헌)
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 HoldemBetMoneyToTarget 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        photonView.RPC("RPC_HoldemBetMoneyToTarget", RpcTarget.All, uid, amount);
    }

    [PunRPC]
    private void RPC_HoldemBetMoneyToTarget(string uid, int amount)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_HoldemBetMoneyToTarget 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

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
        Debug.Log("HoldemBetManager.cs 파일의 SyncHoldemMyBetting 함수로부터"); // 디버깅 추적용 (25.11.12 승헌)
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 SyncHoldemMyBetting 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)
        
        photonView.RPC("RPC_SyncHoldemMyBetting", RpcTarget.All, index, amount);
    }

    [PunRPC]
    private void RPC_SyncHoldemMyBetting(int index, int amount)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_SyncHoldemMyBetting 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.Players.UpdatePlayerBetting(index, amount);
    }

    public void SyncHoldemPlayerIsBet(int index, bool isOn)
    {
        photonView.RPC("RPC_SyncHoldemPlayerIsBet", RpcTarget.All, index, isOn);
    }

    [PunRPC]
    private void RPC_SyncHoldemPlayerIsBet(int index, bool isOn)
    {
        _holdemControl.Players.UpdatePlayerIsBet(index, isOn);
    }

    public void SyncHoldemPlayerIsAlive(int index, bool isOn)
    {
        photonView.RPC("RPC_SyncHoldemPlayerIsAlive", RpcTarget.All, index, isOn);
    }

    [PunRPC]
    private void RPC_SyncHoldemPlayerIsAlive(int index, bool isOn)
    {
        _holdemControl.Players.UpdatePlayerState(index, isOn);
    }

    public void SyncHoldemIsTurn(int index, bool isOn)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 SyncHoldemIsTurn 함수 실행"); // 디버깅 추적용 (25.11.12 승헌

        photonView.RPC("RPC_SyncHoldemIsTurn", RpcTarget.All, index, isOn);
    }

    [PunRPC]
    private void RPC_SyncHoldemIsTurn(int index, bool isOn)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_SyncHoldemIsTurn 함수 실행"); // 디버깅 추적용 (25.11.12 승헌

        _holdemControl.Players.UpdatePlayerTurn(index, isOn);
    }

    public void SyncHoldemPlayerSeedMoney(int index, int amount)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 SyncHoldemPlayerSeedMoney 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        photonView.RPC("RPC_SyncHoldemPlayerSeedMoney", RpcTarget.All, index, amount);
    }

    [PunRPC]
    private void RPC_SyncHoldemPlayerSeedMoney(int index, int amount)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_SyncHoldemPlayerSeedMoney 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.Players.UpdatePlayerSeedMoney(index, amount);
    }

    public void SyncHoldemDieReserve(int index, bool isOn)
    {
        photonView.RPC("RPC_SyncHoldemDieReserve", RpcTarget.All, index, isOn);
    }

    [PunRPC]
    private void RPC_SyncHoldemDieReserve(int index, bool isOn)
    {
        _holdemControl.Players.UpdatePlayerDieReserve(index, isOn);
    }

    public void SyncHoldemDeadPlayerNum(int num)
    {
        photonView.RPC("RPC_SyncHoldemDeadPlayerNum", RpcTarget.All, num);
    }

    [PunRPC]
    private void RPC_SyncHoldemDeadPlayerNum(int num)
    {
        _holdemControl.Players.SetDeadPlayerNum(num);
    }

    public void SyncHoldemIsOneLeft(bool isOn)
    {
        photonView.RPC("RPC_SyncHoldemIsOneLeft", RpcTarget.All, isOn);
    }

    [PunRPC]
    private void RPC_SyncHoldemIsOneLeft(bool isOn)
    {
        _holdemControl.Players.IsOneLeft = isOn;
    }

    public void SyncHoldemWinnerList(string[] wList)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 SyncHoldemWinnerList 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        string json = Json.Serialize(wList);
        photonView.RPC("RPC_SyncHoldemWinnerList", RpcTarget.All, json);
    }

    [PunRPC]
    private void RPC_SyncHoldemWinnerList(string json)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_SyncHoldemWinnerList 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        List<object> wListRaw = Json.Deserialize(json) as List<object>;
        string[] wList = wListRaw.ConvertAll(obj => obj.ToString()).ToArray();
        _holdemControl.Players.SetWinnerList(wList);
    }

    public void SyncHoldemPlayerCard(string pUID, int cardViewID, int cardDetail)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 SyncHoldemPlayerCard 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)
        
        photonView.RPC("RPC_SyncHoldemPlayerCard", RpcTarget.All, pUID, cardViewID, cardDetail);
    }

    [PunRPC]
    private void RPC_SyncHoldemPlayerCard(string pUID, int cardViewID, int cardDetail)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_SyncHoldemPlayerCard 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.Players.SetPlayerCard(pUID, cardViewID, cardDetail);
    }

    #endregion

    #region HoldemGameControl

    public void HoldemStartSync()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 HoldemStartSync 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        photonView.RPC("RPC_HoldemStartSyncing", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_HoldemStartSyncing()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_HoldemStartSyncing 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.StartGame();
    }

    public IEnumerator SyncHoldemPlayerUID()
    {
        Debug.Log("HoldemGameControl.cs 파일의 ProcessStage 함수로부터"); // 디버깅 추적용 (25.11.12 승헌)
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 SyncHoldemPlayerUID 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        yield return null;
        photonView.RPC("RPC_SyncHoldemPlayerUID", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_SyncHoldemPlayerUID()
    {
        // Debug.Log("SyncSystem.cs 파일의 SyncHoldemPlayerUID 함수로부터"); // 디버깅 추적용 (25.11.12 승헌)
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_SyncHoldemPlayerUID 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        Managers.Seat.HoldemConvertToPlayers();
    }

    public IEnumerator SyncHoldemDealerIndex(int index)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 SyncHoldemDealerIndex 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        yield return null;
        photonView.RPC("RPC_SyncHoldemDealerIndex", RpcTarget.All, index);
    }

    [PunRPC]
    private void RPC_SyncHoldemDealerIndex(int index)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_SyncHoldemDealerIndex 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.SetDealer(index);
    }

    public IEnumerator SyncHoldemPotMoney(int money, bool isNextStage = false)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 SyncHoldemPotMoney 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        yield return null;
        photonView.RPC("RPC_SyncHoldemPotMoney", RpcTarget.All, money, isNextStage);
    }

    [PunRPC]
    private void RPC_SyncHoldemPotMoney(int money, bool isNextStage)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_SyncHoldemPotMoney 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.PotMoney = money;

        if (isNextStage)
        {
            // Debug.Log($"case {HoldemGameControl.Control.StageCount} 종료, nextStage");
            _holdemControl.NextStage();
        }
    }

    public IEnumerator HoldemNextStage(int state = 0)      // 1은 스테이지 세부 사항 카운트 증가
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 HoldemNextStage 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        yield return null;
        photonView.RPC("RPC_HoldemNextStage", RpcTarget.All, state);
    }

    [PunRPC]
    private void RPC_HoldemNextStage(int state = 0)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_HoldemNextStage 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.NextStage(state);
    }

    public void HoldemNextStage_V2(int state = 0)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 HoldemNextStage_V2 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        photonView.RPC("RPC_HoldemNextStage", RpcTarget.All, state);
    }

    public IEnumerator HoldemAutoDieTimerSwitch(bool isOn)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 HoldemAutoDieTimerSwitch 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        yield return null;
        photonView.RPC("RPC_HoldemAutoDieTimerSwitch", RpcTarget.All, isOn);
    }

    [PunRPC]
    private void RPC_HoldemAutoDieTimerSwitch(bool isOn)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_HoldemAutoDieTimerSwitch 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.AutoDieTimerSwitch(isOn);
    }

    public IEnumerator SyncHoldemResultUI()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 SyncHoldemResultUI 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        yield return null;
        photonView.RPC("RPC_SyncHoldemResultUI", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_SyncHoldemResultUI()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_SyncHoldemResultUI 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.ShowResult();
    }

    public IEnumerator HoldemClearGame()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 HoldemClearGame 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        yield return null;
        photonView.RPC("RPC_HoldemClearGame", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_HoldemClearGame()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_HoldemClearGame 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.ClearGame();
    }

    #endregion

    #region HoldemBetManager

    public IEnumerator HoldemBetStart(int curplayer)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 HoldemBetStart 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        yield return null;
        photonView.RPC("RPC_HoldemBetStart", RpcTarget.All, curplayer);
    }

    [PunRPC]
    private void RPC_HoldemBetStart(int curplayer)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_HoldemBetStart 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.Bet.HandleBet(curplayer);
    }

    public void HoldemBetProcess(int curPlayer, string betType, int betAmount = 0)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 HoldemBetProcess 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        photonView.RPC("RPC_HoldemBetProcess", RpcTarget.All, curPlayer, betType, betAmount);
    }

    [PunRPC]
    public void RPC_HoldemBetProcess(int curPlayer, string betType, int betAmount = 0)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_HoldemBetProcess 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.Bet.BetProcess(curPlayer, betType, betAmount);
    }

    public void HoldemBetEnd()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 HoldemBetEnd 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        photonView.RPC("RPC_HoldemBetEnd", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_HoldemBetEnd()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_HoldemBetEnd 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.Bet.CurrentStageBetEnd();
    }

    #endregion

    #region HoldemCardManager

    public IEnumerator SyncHoldemDeck()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 SyncHoldemDeck 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        yield return null;
        photonView.RPC("RPC_SyncHoldemDeck", RpcTarget.All, _holdemControl.Card.GetCardDeck());
    }

    [PunRPC]
    private void RPC_SyncHoldemDeck(int[] cardDeck)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_SyncHoldemDeck 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.Card.SetCardDeck(cardDeck);
    }

    public void HoldemAddCard(string toPlayer)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 HoldemAddCard 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        photonView.RPC("RPC_HoldemAddCard", RpcTarget.All, toPlayer);
    }

    [PunRPC]
    private void RPC_HoldemAddCard(string toPlayer)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_HoldemAddCard 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.Card.AddCardToPlayerStarter(toPlayer);
    }

    public void HoldemDealerCard()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 HoldemDealerCard 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        photonView.RPC("RPC_HoldemDealerCard", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_HoldemDealerCard()
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} SyncSystem.cs 파일의 RPC_HoldemDealerCard 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        _holdemControl.Card.AddCardToDealerStarter();
    }

    public void SyncHoldemDealerCard(GameObject go, int index, int cardDetail)
    {
        photonView.RPC("RPC_SyncHoldemDealerCard", RpcTarget.All, go.GetComponent<PhotonView>().ViewID, index, cardDetail);
    }

    [PunRPC]
    private void RPC_SyncHoldemDealerCard(int viewID, int index, int cardDetail)
    {
        _holdemControl.Card.DealerCardSetting(viewID, index, cardDetail);
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
        _pokerControl.StartGame();
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
        _pokerControl.SetFirstPlayer(index);
    }

    public IEnumerator SyncPokerCurrentPlayer(int index)
    {
        yield return null;
        photonView.RPC("RPC_SyncPokerCurrentPlayer", RpcTarget.All, index);

    }

    [PunRPC]
    private void RPC_SyncPokerCurrentPlayer(int index)
    {
        _pokerControl.SetCurrentPlayer(index);
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
        _pokerControl.NextStage(state);
    }

    public IEnumerator PokerMakeCardSelPopup()
    {
        yield return null;
        photonView.RPC("RPC_PokerMakeCardSelPopup", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PokerMakeCardSelPopup()
    {
        _pokerControl.CardSelPopupOn();
    }

    public IEnumerator SyncPokerPotMoney(int money, int isNextStage = 0)
    {
        yield return null;
        photonView.RPC("RPC_SyncPokerPotMoney", RpcTarget.All, money, isNextStage);
    }

    [PunRPC]
    private void RPC_SyncPokerPotMoney(int money, int isNextStage = 0)
    {
        _pokerControl.PotMoney = money;
        Debug.Log($"case {_pokerControl.StageCount} 종료, nextStage");
        _pokerControl.NextStage(isNextStage);
    }

    public IEnumerator PokerAutoDieTimerSwitch(bool isOn)
    {
        yield return null;
        photonView.RPC("RPC_PokerAutoDieTimerSwitch", RpcTarget.All, isOn);
    }

    [PunRPC]
    private void RPC_PokerAutoDieTimerSwitch(bool isOn)
    {
        _pokerControl.AutoDieTimerSwitch(isOn);
    }

    public IEnumerator SyncPokerResultUI()
    {
        yield return null;
        photonView.RPC("RPC_SyncPokerResultUI", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_SyncPokerResultUI()
    {
        _pokerControl.ShowResult();
    }

    public IEnumerator PokerClearGame()
    {
        yield return null;
        photonView.RPC("RPC_PokerClearGame", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PokerClearGame()
    {
        _pokerControl.ClearGame();
    }

    #endregion

    #region PokerCardManager

    public IEnumerator SyncPokerDeck()
    {
        yield return null;
        photonView.RPC("RPC_SyncPokerDeck", RpcTarget.All, _pokerControl.Card.GetCardDeck());
    }

    [PunRPC]
    private void RPC_SyncPokerDeck(int[] cardDeck)
    {
        _pokerControl.Card.SetCardDeck(cardDeck);
    }

    public void PokerAddCard(string toPlayer)
    {
        photonView.RPC("RPC_PokerAddCard", RpcTarget.All, toPlayer);
    }

    [PunRPC]
    private void RPC_PokerAddCard(string toPlayer)
    {
        _pokerControl.Card.AddCardToPlayerStarter(toPlayer);
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
        _pokerControl.Players.UpdatePlayerBetting(index, amount);
    }

    public void SyncPokerPlayerCard(string pUID, GameObject cardGO, int cardDetail, bool isOpenCard)
    {
        int cardViewID = cardGO.GetComponent<PhotonView>().ViewID;
        photonView.RPC("RPC_SyncPokerPlayerCard", RpcTarget.All, pUID, cardViewID, cardDetail, isOpenCard);
    }

    [PunRPC]
    private void RPC_SyncPokerPlayerCard(string pUID, int cardViewID, int cardDetail, bool isOpenCard)
    {
        _pokerControl.Players.SetPlayerCard(pUID, cardViewID, cardDetail, isOpenCard);
    }

    public void SyncPokerPlayerCardSel(int playerIndex, int delcardIndex, int opencardIndex)
    {
        photonView.RPC("RPC_SyncPokerPlayerCardSel", RpcTarget.All, playerIndex, delcardIndex, opencardIndex);
    }

    [PunRPC]
    private void RPC_SyncPokerPlayerCardSel(int playerIndex, int delcardIndex, int opencardIndex)
    {
        Debug.Log($"Player {playerIndex}, del : {delcardIndex}, open : {opencardIndex}");
        _pokerControl.Players.PlayerDelCardSel(playerIndex, delcardIndex);
        _pokerControl.Players.PlayerOpenCardSel(playerIndex, opencardIndex);
    }

    public IEnumerator PokerArrangeSelectedCard()
    {
        yield return null;
        photonView.RPC("RPC_PokerArrangeSelectedCard", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PokerArrangeSelectedCard()
    {
        _pokerControl.Players.ArrangeSelectedCard();
    }

    public void SyncPokerIsTurn(int index, bool isOn)
    {
        photonView.RPC("RPC_SyncPokerIsTurn", RpcTarget.All, index, isOn);
    }

    [PunRPC]
    private void RPC_SyncPokerIsTurn(int index, bool isOn)
    {
        _pokerControl.Players.UpdatePlayerTurn(index, isOn);
    }

    public void SyncPokerDieReserve(int index, bool isOn)
    {
        photonView.RPC("RPC_SyncPokerDieReserve", RpcTarget.All, index, isOn);
    }

    [PunRPC]
    private void RPC_SyncPokerDieReserve(int index, bool isOn)
    {
        _pokerControl.Players.UpdatePlayerDieReserve(index, isOn);
    }

    public void SyncPokerIsOneLeft(bool isOn)
    {
        photonView.RPC("RPC_SyncPokerIsOneLeft", RpcTarget.All, isOn);
    }

    [PunRPC]
    private void RPC_SyncPokerIsOneLeft(bool isOn)
    {
        _pokerControl.Players.IsOneLeft = isOn;
    }

    public void SyncPokerPlayerSeedMoney(int index, int amount)
    {
        photonView.RPC("RPC_SyncPokerPlayerSeedMoney", RpcTarget.All, index, amount);
    }

    [PunRPC]
    private void RPC_SyncPokerPlayerSeedMoney(int index, int amount)
    {
        _pokerControl.Players.UpdatePlayerSeedMoney(index, amount);
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
        _pokerControl.Players.SetWinnerList(wList);
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
        _pokerControl.Bet.HandleBet(curplayer);
    }

    public void PokerBetProcess(int curPlayer, string betType, int betAmount = 0)
    {
        photonView.RPC("RPC_PokerBetProcess", RpcTarget.All, curPlayer, betType, betAmount);
    }

    [PunRPC]
    public void RPC_PokerBetProcess(int curPlayer, string betType, int betAmount = 0)
    {
        _pokerControl.Bet.BetProcess(curPlayer, betType, betAmount);
    }

    public void PokerBetEnd()
    {
        photonView.RPC("RPC_PokerBetEnd", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PokerBetEnd()
    {
        _pokerControl.Bet.CurrentStageBetEnd();
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
        _jackControl.StartGame();
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
        _jackControl.SetFirstPlayer(index);
    }

    public IEnumerator StartFirstBetting()
    {
        yield return null;
        photonView.RPC("RPC_StartFirstBetting", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartFirstBetting()
    {
        _jackControl.StartFirstBet();
    }

    public void FirstBettingAllPass()
    {
        photonView.RPC("RPC_FirstBettingAllPass", RpcTarget.All);

    }

    [PunRPC]
    private void RPC_FirstBettingAllPass()
    {
        _jackControl.FirstBetAllPass();
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
        _jackControl.NextStage(state);
    }

    public IEnumerator JackNoticeBlackJack()
    {
        yield return null;
        photonView.RPC("RPC_JackNoticeBlackJack", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackNoticeBlackJack()
    {
        _jackControl.Players.FindPlayerBlackJack();
    }

    public IEnumerator JackIsDealerIsA()
    {
        yield return null;
        photonView.RPC("RPC_JackIsDealerIsA", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackIsDealerIsA()
    {
        _jackControl.JudgeDealerIsAOrAbove10();
    }

    public void JackInsuranceAllPass()
    {
        photonView.RPC("RPC_JackInsuranceAllPass", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackInsuranceAllPass()
    {
        _jackControl.InsuranceAllPass();
    }

    public void JackGameEnd()
    {
        photonView.RPC("RPC_JackGameEnd", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackGameEnd()
    {
        _jackControl.ClearGame();
    }

    public IEnumerator JackNormalBetting(int playerIndex)
    {
        yield return new WaitForSeconds(1f);
        photonView.RPC("RPC_JackNormalBetting", RpcTarget.All, playerIndex);
    }

    [PunRPC]
    private void RPC_JackNormalBetting(int playerIndex)
    {
        _jackControl.PlayerNormalBetSetting(playerIndex);
    }

    public IEnumerator JackBlackJackPlayerWin(int playerIndex)
    {
        yield return null;
        photonView.RPC("RPC_JackBlackJackPlayerWin", RpcTarget.All, playerIndex);
    }

    [PunRPC]
    private void RPC_JackBlackJackPlayerWin(int playerIndex)
    {
        _jackControl.BlackJackPlayerWin(playerIndex);
    }

    public void JackNormalBetEnd()
    {
        photonView.RPC("RPC_JackNormalBetEnd", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackNormalBetEnd()
    {
        _jackControl.PlayerNormalBetEnd();
    }

    public void JackRestartBetTimer()
    {
        photonView.RPC("RPC_JackRestartBetTimer", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackRestartBetTimer()
    {
        _jackControl.RestartBetTimer();
    }

    public void JackStopBetTimer()
    {
        photonView.RPC("RPC_JackStopBetTimer", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackStopBetTimer()
    {
        _jackControl.BetTimerStop();
    }

    public IEnumerator SyncJacksplitAnd21()
    {
        yield return null;
        photonView.RPC("RPC_SyncJacksplitAnd21", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_SyncJacksplitAnd21()
    {
        _jackControl.ResetSplitAnd21();
    }

    public IEnumerator SyncJackDecideWinner(int playerIndex, int splitNum)
    {
        yield return null;
        photonView.RPC("RPC_SyncJackDecideWinner", RpcTarget.All, playerIndex, splitNum);
    }

    [PunRPC]
    private void RPC_SyncJackDecideWinner(int playerIndex, int splitNum)
    {
        _jackControl.PlayerWinOrLose(playerIndex, splitNum);
    }

    public IEnumerator JackBeforeProcess()
    {
        yield return null;
        photonView.RPC("RPC_JackBeforeProcess", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackBeforeProcess()
    {
        _jackControl.BeforeProcess();
    }


    #endregion

    #region JackCardManager

    public IEnumerator SyncJackDeck()
    {
        yield return null;
        photonView.RPC("RPC_SyncJackDeck", RpcTarget.All, _jackControl.Card.GetCardDeck());
    }

    [PunRPC]
    private void RPC_SyncJackDeck(int[] cardDeck)
    {
        _jackControl.Card.SetCardDeck(cardDeck);
    }

    public void JackAddCard(string toPlayer, int splitNum)
    {
        photonView.RPC("RPC_JackAddCard", RpcTarget.All, toPlayer, splitNum);
    }

    [PunRPC]
    private void RPC_JackAddCard(string toPlayer, int splitNum)
    {
        _jackControl.Card.AddCardToPlayerStarter(toPlayer, splitNum);
    }

    public void JackDealerCard()
    {
        photonView.RPC("RPC_JackDealerCard", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_JackDealerCard()
    {
        _jackControl.Card.AddCardToDealerStarter();
    }

    public void SyncJackDealerCard(GameObject cardGO, int index, int cardDetail)
    {
        int cardViewID = cardGO.GetComponent<PhotonView>().ViewID;
        photonView.RPC("RPC_SyncJackDealerCard", RpcTarget.All, cardViewID, index, cardDetail);
    }

    [PunRPC]
    private void RPC_SyncJackDealerCard(int viewID, int index, int cardDetail)
    {
        _jackControl.Card.SetDealerCard(viewID, index, cardDetail);
    }

    public void JackPlayerCardOrigin(int playerIndex, int splitNum)
    {
        photonView.RPC("RPC_JackPlayerCardOrigin", RpcTarget.All, playerIndex, splitNum);
    }

    [PunRPC]
    private void RPC_JackPlayerCardOrigin(int playerIndex, int splitNum)
    {
        _jackControl.Card.CurTurnPlayerCardOrigin(playerIndex, splitNum);
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
        _jackControl.Players.UpdatePlayerSeedMoney(index, amount);
    }

    public void SyncJackMyBetting(int playerIndex, int splitNum, int amount)
    {
        photonView.RPC("RPC_SyncJackMyBetting", RpcTarget.All, playerIndex, splitNum, amount);
    }

    [PunRPC]
    private void RPC_SyncJackMyBetting(int playerIndex, int splitNum, int amount)
    {
        _jackControl.Players.UpdatePlayerBetting(playerIndex, splitNum, amount);
    }

    public void SyncJackMyBettingReset(int playerIndex, int splitNum)
    {
        photonView.RPC("RPC_SyncJackMyBettingReset", RpcTarget.All, playerIndex, splitNum);
    }

    [PunRPC]
    private void RPC_SyncJackMyBettingReset(int playerIndex, int splitNum)
    {
        _jackControl.Players.UpdatePlayerBetReset(playerIndex, splitNum);
    }

    public void SyncJackIsBet(int index, bool val)
    {
        photonView.RPC("RPC_SyncJackIsBet", RpcTarget.All, index, val);
    }

    [PunRPC]
    private void RPC_SyncJackIsBet(int index, bool val)
    {
        _jackControl.Players.UpdatePlayerIsBet(index, val);
    }

    public void SyncJackIsGameEnd(int playerIndex, int splitNum, int val)
    {
        photonView.RPC("RPC_SyncJackIsGameEnd", RpcTarget.All, playerIndex, splitNum, val);
    }

    [PunRPC]
    private void RPC_SyncJackIsGameEnd(int playerIndex, int splitNum, int val)
    {
        _jackControl.Players.UpdatePlayerIsGameEnd(playerIndex, splitNum, val);
    }

    public void SyncJackPlayerCard(string pUID, GameObject cardGO, int cardDetail)
    {
        int cardViewID = cardGO.GetComponent<PhotonView>().ViewID;
        photonView.RPC("RPC_SyncJackPlayerCard", RpcTarget.All, pUID, cardViewID, cardDetail);
    }

    [PunRPC]
    private void RPC_SyncJackPlayerCard(string pUID, int cardViewID, int cardDetail)
    {
        _jackControl.Card.SetPlayerCard(pUID, cardViewID, cardDetail);
    }

    public void SyncJackIsInsurance(int index, int val)
    {
        photonView.RPC("RPC_SyncJackIsInsurance", RpcTarget.All, index, val);
    }

    [PunRPC]
    private void RPC_SyncJackIsInsurance(int index, int val)
    {
        _jackControl.Players.UpdatePlayerIsInsurance(index, val);
    }

    public void JackPlayerSplitSetting(int playerIndex, int nowSplitNum)
    {
        photonView.RPC("RPC_JackPlayerSplitSetting", RpcTarget.All, playerIndex, nowSplitNum);
    }

    [PunRPC]
    private void RPC_JackPlayerSplitSetting(int playerIndex, int nowSplitNum)
    {
        _jackControl.PlayerSplitSetting(playerIndex, nowSplitNum);
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

}