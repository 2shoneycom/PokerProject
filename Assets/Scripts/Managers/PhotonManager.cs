using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
using UnityEngine;
using static Define;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private Dictionary<string, RoomInfo> availableRooms = new Dictionary<string, RoomInfo>();
    UI_Loading _loadingUI;
    UI_Login _loginUI;
    public string currentRoomName;

    private bool isWaitingForJoinOtherGame = false;
    private int currentBetMoney;
    private Define.GameType currentGameType;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    /* 현재 게임 종류 반환 */
    public Define.GameType GetGameType()
    {
        return currentGameType;
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
        Managers.DB.SetUserStatus(Define.Status.Online);    // 로그인씬 -> 로비씬 (status: online)
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

    public void CreateGame(int betMoney, Define.GameType gameType) 
    {
        _loadingUI = Managers.UI.ShowPopupUI<UI_Loading>();
        StartCoroutine(LoadingCreateGame(0.5f, betMoney, gameType));
    }

    IEnumerator LoadingCreateGame(float sec, int betMoney, Define.GameType gameType)
    {
        yield return new WaitForSeconds(sec);
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InLobby)
        {
            CreateRoom(betMoney, gameType);
        }
        else
        {
            // 연결되어있지 않으면 Reconnect 후 콜백에서 CreateRoom 호출
            StartCoroutine(WaitForLobbyAndCreateRoom(betMoney, gameType));
        }
    }

    IEnumerator WaitForLobbyAndCreateRoom(int betMoney, Define.GameType gameType)
    {
        if (!PhotonNetwork.IsConnected)
        {
            Reconnect();
        }

        while (!PhotonNetwork.IsConnectedAndReady)
            yield return null;

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            while (!PhotonNetwork.InLobby)
                yield return null;
        }

        CreateRoom(betMoney, gameType);
    }

    void CreateRoom(int betMoney, Define.GameType gameType)
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            _loadingUI.SetConnectionInfoText("Creating New Room..");
            string roomName = gameType.ToString() + betMoney + UnityEngine.Random.Range(1000, 9999);
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = 10,
                IsVisible = false, // 방이 리스트에 나타나게 설정
                IsOpen = false,    // 새로운 플레이어가 들어올 수 있도록 설정
                CleanupCacheOnLeave = false,
                CustomRoomProperties = new ExitGames.Client.Photon.Hashtable {
                    { "betMoney", betMoney },
                    { "gameType", gameType.ToString() }
                },
                CustomRoomPropertiesForLobby = new string[] {"betMoney", "gameType"}
            };
            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
        else
        {
            Reconnect();
        }
    }

    public void OpenRoomToPublic()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;
            Debug.Log("방이 공개되었습니다.");
        }
    }

    public void JoinGame(int betMoney, Define.GameType gameType)
    {
        _loadingUI = Managers.UI.ShowPopupUI<UI_Loading>();
        StartCoroutine(LoadingJoinGame(0.5f, betMoney, gameType));
    }

        // roomID로 해당 방 들어가는 함수
    public void JoinRoomByName(string roomName)
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("초대받은 방의 아이디: " + roomName);
            _loadingUI = Managers.UI.ShowPopupUI<UI_Loading>();
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Reconnect();
        }
    }

    IEnumerator LoadingJoinGame(float sec, int betMoney, Define.GameType gameType)
    {
        yield return new WaitForSeconds(sec);
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InLobby)
        {
            JoinOrCreateRoom(betMoney, gameType);
        }
        else
        {
            StartCoroutine(WaitForLobbyAndJoinRoom(betMoney, gameType));
        }
    }

    IEnumerator WaitForLobbyAndJoinRoom(int betMoney, Define.GameType gameType)
    {
        Reconnect();

        while (!PhotonNetwork.IsConnectedAndReady)
        {
            yield return null;
        }

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            while (!PhotonNetwork.InLobby)
                yield return null;
        }

        JoinOrCreateRoom(betMoney, gameType);
    }

    void JoinOrCreateRoom(int betMoney, Define.GameType gameType)
    {
        List<RoomInfo> matchingRooms = new List<RoomInfo>();

        foreach (RoomInfo room in availableRooms.Values)
        {
            if (room.CustomProperties.ContainsKey("betMoney") && room.CustomProperties.ContainsKey("gameType"))
            {
                object betObj = room.CustomProperties["betMoney"];
                object gameTypeObj = room.CustomProperties["gameType"];

                if (betObj == null || gameTypeObj == null)
                {
                    Debug.Log($"Room {room.Name} betMoney or gameType is null.");
                    continue;
                }

                int roomBetMoney = Convert.ToInt32(betObj);
                string roomGameTypeStr = gameTypeObj.ToString();
                Define.GameType roomGameType = (Define.GameType)Enum.Parse(typeof(Define.GameType), roomGameTypeStr);

                Debug.Log($"Room {room.Name} betMoney: {roomBetMoney}, gameType: {roomGameType}");

                if (roomBetMoney == betMoney && roomGameType == gameType && room.PlayerCount < room.MaxPlayers)
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
            Debug.Log($"Entering Room ({betMoney}, {gameType})...");
        }
        else
        {
            Debug.Log($"No available room for betMoney {betMoney}, gameType {gameType}");
            CreateRoom(betMoney, gameType);
        }
    }

    public override void OnJoinedRoom()
    {   
        // 룸 참가에 성공한 경우
        PhotonNetwork.IsMessageQueueRunning = false;
        Debug.Log("Success to Enter Room");
        _loadingUI.SetConnectionInfoText("Success to Enter Room");

        // 방의 gameType 가져오기
        Define.GameType gameType = Define.GameType.Holdem; // 기본값

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gameType"))
        {
            string gameTypeStr = PhotonNetwork.CurrentRoom.CustomProperties["gameType"].ToString();
            gameType = (Define.GameType)Enum.Parse(typeof(Define.GameType), gameTypeStr);
        }

        // gameType에 따라 다른 씬 로드
        switch (gameType)
        {
            case Define.GameType.Holdem:
                Managers.Scene.PhotonLoadScene(Define.Scene.Holdem);
                break;
            case Define.GameType.Seven:
                //Managers.Scene.PhotonLoadScene(Define.Scene.Seven);
                break;
            case Define.GameType.Black:
                //Managers.Scene.PhotonLoadScene(Define.Scene.Black);
                break;
            default:
                Debug.LogError($"Unknown gameType: {gameType}");
                break;
        }

        currentRoomName = PhotonNetwork.CurrentRoom.Name;

        // 방에 들어왔으면 내 포톤 플레이어 정보 설정
        SetMyPhotonPlayerInfo(PhotonNetwork.LocalPlayer);
    }

    public void JoinOtherGame(int betMoney, Define.GameType gameType)
    {
        if (PhotonNetwork.InRoom)
        {
            isWaitingForJoinOtherGame = true;
            currentBetMoney = betMoney;
            currentGameType = gameType;

            PhotonNetwork.LeaveRoom();
            Debug.Log("Leaving current room...");
        }
        else
        {
            JoinOtherRoom(betMoney, gameType);
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Successfully left the room.");

        if (isWaitingForJoinOtherGame)
        {
            isWaitingForJoinOtherGame = false;
            StartCoroutine(LoadingJoinOtherRoom(0.5f, currentBetMoney, currentGameType));
        }
    }

    IEnumerator LoadingJoinOtherRoom(float sec, int betMoney, Define.GameType gameType)
    {
        yield return new WaitForSeconds(sec);

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InLobby)
        {
            JoinOtherRoom(betMoney, gameType);
        }
        else
        {
            StartCoroutine(WaitForLobbyAndJoinOtherRoom(betMoney, gameType));
        }
    }

    IEnumerator WaitForLobbyAndJoinOtherRoom(int betMoney, Define.GameType gameType)
    {
        Reconnect();

        while (!PhotonNetwork.IsConnectedAndReady)
        {
            yield return null;
        }

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            while (!PhotonNetwork.InLobby)
                yield return null;
        }

        JoinOtherRoom(betMoney, gameType);
    }

    void JoinOtherRoom(int betMoney, Define.GameType gameType)
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log($"Searching Other Room ({betMoney}, {gameType})...");

            List<RoomInfo> matchingRooms = new List<RoomInfo>();

            foreach (RoomInfo room in availableRooms.Values)
            {
                if (room.CustomProperties.ContainsKey("betMoney") && room.CustomProperties.ContainsKey("gameType"))
                {
                    object betObj = room.CustomProperties["betMoney"];
                    object gameTypeObj = room.CustomProperties["gameType"];

                    if (betObj == null || gameTypeObj == null)
                    {
                        Debug.Log($"Room {room.Name} betMoney or gameType is null.");
                        continue;
                    }

                    int roomBetMoney = Convert.ToInt32(betObj);
                    string roomGameTypeStr = gameTypeObj.ToString();

                    Define.GameType roomGameType = (Define.GameType)Enum.Parse(typeof(Define.GameType), roomGameTypeStr);

                    if (roomBetMoney == betMoney &&
                        roomGameType == gameType &&
                        room.PlayerCount < room.MaxPlayers &&
                        PhotonNetwork.CurrentRoom != null &&
                        room.Name != PhotonNetwork.CurrentRoom.Name)
                    {
                        matchingRooms.Add(room);
                    }
                }
            }

            if (matchingRooms.Count > 0)
            {
                int rand = UnityEngine.Random.Range(0, matchingRooms.Count);
                PhotonNetwork.JoinRoom(matchingRooms[rand].Name);
                Debug.Log($"Entering Other Room ({betMoney}, {gameType})...");
            }
            else
            {
                Debug.Log($"No other available room for betMoney {betMoney}, gameType {gameType}. Creating new room.");

                CreateRoom(betMoney, gameType);
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

    public Define.GameType GetCurrentRoomGameType()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gameType"))
        {
            string gameTypeStr = PhotonNetwork.CurrentRoom.CustomProperties["gameType"].ToString();
            try
            {
                Define.GameType gameType = (Define.GameType)Enum.Parse(typeof(Define.GameType), gameTypeStr);
                return gameType;
            }
            catch (Exception e)
            {
                Debug.LogError($"gameType 변환 오류: {gameTypeStr}, {e.Message}");
                return Define.GameType.None;
            }
        }
        else
        {
            Debug.LogWarning("현재 방 정보가 없거나 gameType 프로퍼티가 없음");
            return Define.GameType.None;
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
