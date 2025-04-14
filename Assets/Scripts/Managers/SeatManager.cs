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

    public void Init(int seatSize)      // holdemscene���� init����
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
            seats.Add("empty");
        }
        // ui
        _holdem.UpdateAllSeatUI();
    }

    public void HaveSeat(string player_uid, int seatIndex)
    {   
        if (seats[seatIndex] == "empty")
        {
            seats[seatIndex] = player_uid;
            occupiedCount++;
            SyncSystem.Instacne.SyncHaveSeat(player_uid, seatIndex);
            if (occupiedCount >= 2 && PhotonNetwork.IsMasterClient)
            {
                /* 
                ���� ��� 2�� �̻��̰� ���� �����̸�,
                UI�� ���� ��ŸƮ ��ư ���� ��û
                */
            }
        }
        else
        {
            Debug.Log($"{seatIndex}��° �ڸ��� �̹� �����Ǿ��ֽ��ϴ�.");
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
//            MyDebug.Instance.DebugLog($"{seatIndex}��° �ڸ��� �ɾ��ִ� �ڸ��� �ƴϹǷ� ���� �� �����ϴ�.");
//            Debug.Log($"{seatIndex}��° �ڸ��� �ɾ��ִ� �ڸ��� �ƴϹǷ� ���� �� �����ϴ�.");
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