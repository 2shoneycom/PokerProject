using System;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

public class SeatManager
{
    private List<string> seats;
    public List<string> Seats { get { return seats; } }
    private int occupiedCount;

    public const string DEFAULT_NULL_SEAT = "자리 선택";

    HoldemScene _holdem = null;

    public void Init(int seatSize)      // holdemscene에서 init해줌
    {
        _holdem = (HoldemScene)Managers.Scene.CurrentScene;

        occupiedCount = 0;
        SyncSystem.Sync.OnSeatsSynced -= ApplySeatsData;
        SyncSystem.Sync.OnSeatsSynced += ApplySeatsData;
        SyncSystem.Sync.OnHaveSeat -= TakeSeat;
        SyncSystem.Sync.OnHaveSeat += TakeSeat;

        SetSeats(seatSize);

        if (PhotonNetwork.IsMasterClient == false)
        {
            RequestSyncSeats();
        }
    }

    private void SetSeats(int seatSize)
    {
        seats = new List<string>();
        for (int i = 0; i < seatSize; i++)
        {
            seats.Add(DEFAULT_NULL_SEAT);
            seats.Add(DEFAULT_NULL_SEAT);

            // ui
            _holdem.UpdateSeatUI(i, DEFAULT_NULL_SEAT);
        }
    }

    public void HaveSeat(string playerUID, string playerNickName, int seatIndex)
    {   
        if(seats[seatIndex * 2] != DEFAULT_NULL_SEAT)
        {
            Debug.Log($"{seatIndex}번째 자리는 이미 차지되어있습니다.");
            return;
        }

        if(User.NowHoldemPlayer.SeatIndex != -1)
        {
            Debug.Log($"이미 {User.NowHoldemPlayer.SeatIndex}번째 자리에 앉으셨습니다.");
            return;
        }

        SyncSystem.Sync.SyncHaveSeat(playerUID, playerNickName, seatIndex);
    }

    private void TakeSeat(string playerUID, string playerNickName, int seatIndex)
    {
        seats[seatIndex * 2] = playerUID;
        seats[seatIndex * 2 + 1] = playerNickName;

        if (playerUID == User.NowUser.GetUid())
            User.NowHoldemPlayer.SetSeatIndex(seatIndex);

        // occupiedCount 변수 동기화 위해 옮김
        occupiedCount++;
        if (occupiedCount >= 2 && PhotonNetwork.IsMasterClient && HoldemGameControl.Control.IsPlaying == false)
        {
            /* 
            앉은 사람 2명 이상이고 내가 방장이면,
            UI에 게임 스타트 버튼 띄우기 요청
            */
            _holdem.ReadyForGameStart();
        }

        // ui
        _holdem.UpdateSeatUI(seatIndex, playerNickName);
    }

    public void LeaveSeat(string player_uid, int seatIndex)
    {
        if (seats[seatIndex * 2] == player_uid)
        {
            seats[seatIndex * 2] = DEFAULT_NULL_SEAT;
            seats[seatIndex * 2 + 1] = DEFAULT_NULL_SEAT;
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
        SyncSystem.Sync.SyncSeatsToMaster();
    }

    public string[] SendSeatsData()
    {
        return seats.ToArray();
    }

    public void ApplySeatsData(string[] syncedSeats)
    {
        for (int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            int index = i * 2;
            if (syncedSeats[index] != DEFAULT_NULL_SEAT)
            {
                seats[index] = syncedSeats[index];
                seats[index + 1] = syncedSeats[index + 1];

                occupiedCount++;

                _holdem.UpdateSeatUI(i, seats[index + 1]);
            }
        }
    }

    public void ConverToPlayers()
    {
        for(int i = 0; i < HoldemGameControl.MAX_PLAYER_NUM; i++)
        {
            HoldemGameControl.Players.UpdatePlayerUID(i, seats[i * 2]);
        }

        HoldemGameControl.Control.NextStage();
    }

    public int GetOccupiedCount()
    {
        return occupiedCount;
    }

    public string GetPlayerNickNameByUID(string pUID)
    {
        for (int i = 0; i < seats.Count; i++) 
        {
            if (seats[i] == pUID)
                return seats[i + 1];
        }
        return "";
    }
}