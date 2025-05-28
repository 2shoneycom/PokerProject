using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

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
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }
    }

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
        photonView.RPC("HoldemStartSyncing", RpcTarget.All);
    }

    [PunRPC]
    private void HoldemStartSyncing()
    {
        Managers.Seat.HoldemSeatSetting();
        UI_Holdem ui = (UI_Holdem)Managers.UI.SceneUI;
        ui.UISwitch(true);
        HoldemGameControl.Instance.StartGame();
    }
}