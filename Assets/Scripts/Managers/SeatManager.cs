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

    GameScene curGameScene = null;


    public void Init(int seatSize)      // holdemscene에서 init해줌
    {
        curGameScene = (GameScene)Managers.Scene.CurrentScene;

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
            curGameScene.UpdateSeatUI(i, DEFAULT_NULL_SEAT);
        }
    }

    public void HaveSeat(string playerUID, string playerNickName, int seatIndex)
    {
        if (seats[seatIndex * 2] != DEFAULT_NULL_SEAT)
        {
            Debug.Log($"{seatIndex}번째 자리는 이미 차지되어있습니다.");
            return;
        }

        if (User.NowGamePlayer.SeatIndex != -1)
        {
            Debug.Log($"이미 {User.NowGamePlayer.SeatIndex}번째 자리에 앉으셨습니다.");
            return;
        }

        SyncSystem.Sync.SyncHaveSeat(playerUID, playerNickName, seatIndex);
    }

    private void TakeSeat(string playerUID, string playerNickName, int seatIndex)
    {
        seats[seatIndex * 2] = playerUID;
        seats[seatIndex * 2 + 1] = playerNickName;

        if (playerUID == User.NowUser.GetUid())
            User.NowGamePlayer.SetSeatIndex(seatIndex);

        // occupiedCount 변수 동기화 위해 옮김
        occupiedCount++;
        if (occupiedCount >= 2 && PhotonNetwork.IsMasterClient && Managers.IsNowPlayingGame == false)
        {
            /* 
            앉은 사람 2명 이상이고 내가 방장이면,
            UI에 게임 스타트 버튼 띄우기 요청
            */
            curGameScene.ReadyForGameStart();
        }

        // ui
        curGameScene.UpdateSeatUI(seatIndex, playerNickName);
    }

    public void LeaveSeat(string player_uid)
    {
        int targetIndex = GetSeatIndex(player_uid);

        if (targetIndex == -1)
        {
            Debug.LogError("SeatManager.cs -> LeaveSeat()에서 해당 uid를 찾을 수 없습니다.");
        }
        else
        {   // 글고 지금은 포톤에서 이 함수를 부르고 있는데, 포톤에서 어떤 게임에서 나갓는지 파악하고 호출도 해야할듯, 게임 중인지 아닌지도 판단하고
            seats[targetIndex] = DEFAULT_NULL_SEAT;    // 기존 i 자리에 있던 플레이어의 uid (i) 제거
            seats[targetIndex + 1] = DEFAULT_NULL_SEAT;    // 기존 i 자리에 있던 플레이어의 닉네임 (i + 1) 제거

            // UI 업데이트
            curGameScene.UpdateSeatUI(targetIndex / 2, "자리 선택");
        }
    }

    private int GetSeatIndex(string uid)
    {
        int idx = seats.FindIndex(seat => seat == uid);

        if (idx == -1)
        {
            Debug.LogError("SeatManager.cs -> GetSeatIndex()에서 해당 인덱스를 찾을 수 없습니다.");
        }

        return idx;
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
        for (int i = 0; i < Managers.GetCurGameMaxPlayer; i++)
        {
            int index = i * 2;
            if (syncedSeats[index] != DEFAULT_NULL_SEAT)
            {
                seats[index] = syncedSeats[index];
                seats[index + 1] = syncedSeats[index + 1];

                occupiedCount++;

                curGameScene.UpdateSeatUI(i, seats[index + 1]);
            }
        }
    }

    public void ConverToPlayers()
    {
        for (int i = 0; i < Managers.GetCurGameMaxPlayer; i++)
        {
            HoldemGameControl.Players.UpdatePlayerUID(i, seats[i * 2]);
        }
        User.NowUser.HoldemSyncSeedMoney();
        curGameScene.UpdateBetUI(true);
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