using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private Dictionary<string, RoomInfo> availableRooms = new Dictionary<string, RoomInfo>();
    UI_Loading _loadingUI;
    UI_Login _loginUI;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    #region Connect
    public void ConnectToPhoton(UI_Login login)
    {
        _loginUI = login;
        PhotonNetwork.ConnectUsingSettings();
        // 같은 버전만 매칭 시도를 위해 게임 버전 설정        PhotonNetwork.GameVersion = gameVersion;
        // 설정한 정보로 마스터 서버 접속 시도
        // 접속 시도 중 표시
        _loginUI.SetConnectionInfoText("포톤 서버 연결중...");
    }

    public override void OnConnectedToMaster()
    {   // 포톤 마스터 서버에 접속 성공한 경우 자동 실행됨.
        _loginUI.SetConnectionInfoText("연결 성공!");
        PhotonNetwork.JoinLobby();
        Managers.Scene.LoadScene(Define.Scene.Lobby);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {   // 마스터 서버 접속 실패 || 서버 접속 상태에서 접속이 끊긴 경우
        // UI 띄우면서 재접속 창 뜨게 하기
        _loginUI.ShowReconnectButton();
    }

    public void DisconnectPhoton()
    {
        PhotonNetwork.Disconnect();
    }

    public void Reconnect()
    {
        // 연결 재시도
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"OnRoomListUpdate called. Room count: {roomList.Count}");
        foreach (RoomInfo room in roomList)
        {
            Debug.Log($"Room: {room.Name}, PlayerCount: {room.PlayerCount}, Removed: {room.RemovedFromList}");
            if (room.RemovedFromList)
            {
                // 방 목록에서 제거된 방 처리
                availableRooms.Remove(room.Name);
                Debug.Log("방 제거: " + room.Name);
            }
            else
            {
                // 사용 가능한 방 표시 및 업데이트
                if (availableRooms.ContainsKey(room.Name))
                {
                    Debug.Log($"Room {room.Name} betMoney: {room.CustomProperties["betMoney"]}");
                    availableRooms[room.Name] = room;
                }
                else
                {
                    availableRooms.Add(room.Name, room);
                }
                Debug.Log("사용 가능한 방: " + room.Name + ", 플레이어 수: " + room.PlayerCount + "/" + room.MaxPlayers);
            }
        }
    }

    public void CreateHoldem(int betMoney) 
    {
        _loadingUI = Managers.UI.ShowPopupUI<UI_Loading>();
        StartCoroutine(LoadingCreateHoldem(0.5f, betMoney));
    }

    IEnumerator LoadingCreateHoldem(float sec, int betMoney)
    {
        yield return new WaitForSeconds(sec);
        CreateRoom(betMoney);
    }

    void CreateRoom(int betMoney)
    {
        if (PhotonNetwork.IsConnected)
        {
            _loadingUI.SetConnectionInfoText("Creating New Room..");
            string roomName = "Room " + UnityEngine.Random.Range(1000, 9999);
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = 10,
                IsVisible = true, // 방이 리스트에 나타나게 설정
                IsOpen = true,    // 새로운 플레이어가 들어올 수 있도록 설정
                CleanupCacheOnLeave = false,
                CustomRoomProperties = new ExitGames.Client.Photon.Hashtable { { "betMoney", betMoney} },
                CustomRoomPropertiesForLobby = new string[] {"betMoney"}
            };
            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
        else
        {
            Reconnect();
        }
    }

    public void JoinHoldem(int betMoney)
    {
        _loadingUI = Managers.UI.ShowPopupUI<UI_Loading>();
        StartCoroutine(LoadingJoinHoldem(0.5f, betMoney));
    }

    IEnumerator LoadingJoinHoldem(float sec, int betMoney)
    {
        yield return new WaitForSeconds(sec);
        JoinRoom(betMoney);
    }

    void JoinRoom(int betMoney)
    {
        if (PhotonNetwork.IsConnected)
        {
            _loadingUI.SetConnectionInfoText("Searching Room ({betMoney})...");
            JoinOrCreateRoom(betMoney);
        }
        else
        {
            Reconnect();
        }
    }

    void JoinOrCreateRoom(int betMoney)
    {
        List<RoomInfo> matchingRooms = new List<RoomInfo>();

        foreach (RoomInfo room in availableRooms.Values)
        {
            if (room.CustomProperties.ContainsKey("betMoney"))
            {
                object betObj = room.CustomProperties["betMoney"];
                if (betObj == null)
                {
                    Debug.Log($"Room {room.Name} betMoney is null.");
                    continue;
                }

                int roomBetMoney = Convert.ToInt32(betObj);
                Debug.Log($"Room {room.Name} betMoney: {roomBetMoney}");

                if (roomBetMoney == betMoney && room.PlayerCount < room.MaxPlayers)
                {
                    matchingRooms.Add(room);
                }
            }
        }

        Debug.Log($"matchingRooms.Count = {matchingRooms.Count}");

        if (matchingRooms.Count > 0)
        {
            int rand = UnityEngine.Random.Range(0, matchingRooms.Count);
            PhotonNetwork.JoinRoom(matchingRooms[rand].Name);
            _loadingUI.SetConnectionInfoText($"Entering Room ({betMoney})...");
        }
        else
        {
            _loadingUI.SetConnectionInfoText("No Available Room.. Creating New Room...");
            Debug.Log("No available room for betMoney " + betMoney);
            CreateRoom(betMoney);
        }
    }

    public override void OnJoinedRoom()
    {   // 룸 참가에 성공한 경우
        _loadingUI.SetConnectionInfoText("Success to Enter Room");
        // 모든 룸 참가자가 GameRoom 씬을 로드하게함
        Managers.Scene.PhotonLoadScene(Define.Scene.Holdem);
        // 씬메니저로 로드하면 연결 정보가 사라짐.

        // 방에 들어왔으면 내 포톤 플레이어 정보 설정
        SetMyPhotonPlayerInfo(PhotonNetwork.LocalPlayer);
    }

    public void JoinOtherHoldemRoom(int betMoney)
    {
        _loadingUI = Managers.UI.ShowPopupUI<UI_Loading>();
        StartCoroutine(LoadingJoinOtherHoldem(0.5f, betMoney));
    }

    IEnumerator LoadingJoinOtherHoldem(float sec, int betMoney)
    {
        yield return new WaitForSeconds(sec);
        JoinOtherRoom(betMoney);
    }

    void JoinOtherRoom(int betMoney)
    {
        if (PhotonNetwork.IsConnected)
        {
            _loadingUI.SetConnectionInfoText($"Searching Other Room ({betMoney})...");

            List<RoomInfo> matchingRooms = new List<RoomInfo>();

            foreach (RoomInfo room in availableRooms.Values)
            {
                if (room.CustomProperties.ContainsKey("betMoney"))
                {
                    object betObj = room.CustomProperties["betMoney"];
                    int roomBetMoney = Convert.ToInt32(betObj);

                    if (roomBetMoney == betMoney &&
                        room.PlayerCount < room.MaxPlayers &&
                        PhotonNetwork.CurrentRoom != null &&
                        room.Name != PhotonNetwork.CurrentRoom.Name) // 현재 방 제외
                    {
                        matchingRooms.Add(room);
                    }
                }
            }

            if (matchingRooms.Count > 0)
            {
                int rand = UnityEngine.Random.Range(0, matchingRooms.Count);
                PhotonNetwork.JoinRoom(matchingRooms[rand].Name);
                _loadingUI.SetConnectionInfoText($"Entering Other Room ({betMoney})...");
            }
            else
            {
                _loadingUI.SetConnectionInfoText("No Other Room.. Creating New Room...");
                Debug.Log($"No other available room for betMoney {betMoney}. Creating new room.");

                CreateRoom(betMoney);
            }
        }
        else
        {
            Reconnect();
        }
    }

    public int GetCurrentRoomBetMoney()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("betMoney"))
        {
            return (int)PhotonNetwork.CurrentRoom.CustomProperties["betMoney"];
        }
        else
        {
            Debug.LogWarning("현재 방 정보가 없거나 betMoney 프로퍼티가 없음");
            return -1;
        }
    }

    #endregion

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            Debug.LogError("PhotonManger.cs -> LeaveRoom(), 현재 방에 들어와있는 상태가 아닙니다.");
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            TakeOwnerShip();
            HoldemGameControl.Control.ProcessStage();
        }
    }

    private void TakeOwnerShip()
    {
        foreach (var view in FindObjectsOfType<PhotonView>())
        {
            if (view.Owner == null || view.OwnerActorNr == 0)
            {
                view.TransferOwnership(PhotonNetwork.LocalPlayer);
            }
        }
    }

    private void SetMyPhotonPlayerInfo(Player newPlayer)
    {
        Hashtable props = new Hashtable
        {
            { "uid", User.NowUser.GetUid() }
        };
        newPlayer.SetCustomProperties(props);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"나간 사람 ActorNumber: {otherPlayer.ActorNumber}");
        Debug.Log($"나간 사람 CustomProperties: {otherPlayer.CustomProperties["uid"]}");
        
        if (otherPlayer.CustomProperties.ContainsKey("uid"))
        {
            string uid = otherPlayer.CustomProperties["uid"].ToString();
            Managers.Seat.LeaveSeat(uid);
        }
        else
        {
            Debug.LogWarning("나간 사람의 CustomProperties에 uid가 없습니다.");
        }
    }
}
