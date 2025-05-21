using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SeatManager
{
    private List<string> seats;
    public List<string> Seats { get { return seats; } }
    private int occupiedCount;

    HoldemScene _holdem = null;

    public void Init(int seatSize)      // holdemscene에서 init해줌
    {
        _holdem = (HoldemScene)Managers.Scene.CurrentScene;

        occupiedCount = 0;
        SyncSystem.Instacne.OnSeatsSynced += ApplySeatsData;
        SyncSystem.Instacne.OnHaveSeat += TakeSeat;

        SetSeats(seatSize);

        if (PhotonNetwork.IsMasterClient == false)
        {
            RequestSyncSeats();
        }
    }

    private void SetSeats(int seatSize)
    {
        seats = new List<string>(seatSize);
        for (int i = 0; i < seatSize; i++)
        {
            seats.Add("자리 선택");
        }
        // ui
        _holdem.UpdateAllSeatUI();
    }

    public void HaveSeat(string player_uid, int seatIndex)
    {   
        if (seats[seatIndex] == "자리 선택" && User.NowUser.GetHoldemSeat() == -1)
        {
            seats[seatIndex] = player_uid;
            occupiedCount++;
            SyncSystem.Instacne.SyncHaveSeat(player_uid, seatIndex);
            if (occupiedCount >= 2 && PhotonNetwork.IsMasterClient)
            {
                /* 
                앉은 사람 2명 이상이고 내가 방장이면,
                UI에 게임 스타트 버튼 띄우기 요청
                */
                _holdem.ReadyForGameStart();
            }
        }
        else
        {
            Debug.Log($"{seatIndex}번째 자리는 이미 차지되어있습니다.");
        }
    }

    private void TakeSeat(string player_uid, int seatIndex)
    {
        seats[seatIndex] = player_uid;
        // ui
        _holdem.UpdateAllSeatUI();
    }

    public void LeaveSeat(string player_uid, int seatIndex)
    {
        if (seats[seatIndex] == player_uid)
        {
            seats[seatIndex] = "empty";
            occupiedCount--;
        }
        else
        {
//            MyDebug.Instance.DebugLog($"{seatIndex}번째 자리는 앉아있던 자리가 아니므로 떠날 수 없습니다.");
//            Debug.Log($"{seatIndex}번째 자리는 앉아있던 자리가 아니므로 떠날 수 없습니다.");
        }

//        MyDebug.Instance.DebugLog($"{seats[0]}, {seats[1]}, {seats[2]}, {seats[3]}, {seats[4]}, {seats[5]}, {seats[6]}");
    }

    public void RequestSyncSeats()
    {
        SyncSystem.Instacne.SyncSeatsToMaster();
    }

    public string[] SendSeatsData()
    {
        return seats.ToArray();
    }

    public void ApplySeatsData(string[] syncedSeats)
    {
        int seatsLength = syncedSeats.Length;
        for (int i = 0; i < seatsLength; i++)
        {
            seats[i] = syncedSeats[i];
        }
        // ui
        _holdem.UpdateAllSeatUI();
    }
}