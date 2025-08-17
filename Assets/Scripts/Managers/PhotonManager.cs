using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
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
        foreach (RoomInfo room in roomList)
        {
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

    public void CreateRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            _loadingUI.SetConnectionInfoText("Creating New Room..");
            string roomName = "Room " + Random.Range(1000, 9999);
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = 10,
                IsVisible = true, // 방이 리스트에 나타나게 설정
                IsOpen = true,    // 새로운 플레이어가 들어올 수 있도록 설정
                CleanupCacheOnLeave = false
            };
            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
        else
        {
            Reconnect();
        }
    }

    public void JoinHoldem()
    {
        _loadingUI = Managers.UI.ShowPopupUI<UI_Loading>();
        StartCoroutine(Loading(0.5f));
    }

    public void JoinPoker()
    {
        _loadingUI = Managers.UI.ShowPopupUI<UI_Loading>();
        StartCoroutine(Loading(0.5f));
    }

    public void JoinJack()
    {
        _loadingUI = Managers.UI.ShowPopupUI<UI_Loading>();
        StartCoroutine(Loading(0.5f));
    }

    IEnumerator Loading(float sec)
    {
        yield return new WaitForSeconds(sec);
        JoinRoom();
    }

    void JoinRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            _loadingUI.SetConnectionInfoText("Entering Room..");
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            Reconnect();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {   // 랜덤 룸 접속에 실패한 경우 (서버 연결 안끊김)
        _loadingUI.SetConnectionInfoText("No Available Room..");
        Debug.Log("No available room");
        // 최대 7명을 수용 가능한 빈 방 생성
        CreateRoom();
    }

    public override void OnJoinedRoom()
    {   // 룸 참가에 성공한 경우
        _loadingUI.SetConnectionInfoText("Success to Enter Room");
        // 모든 룸 참가자가 GameRoom 씬을 로드하게함
        Define.GameType gameType = Managers.CurrentGameType;

        switch (gameType)
        {
            case Define.GameType.Holdem:
                Managers.Scene.PhotonLoadScene(Define.Scene.Holdem);
                break;
            case Define.GameType.Poker:
                Managers.Scene.PhotonLoadScene(Define.Scene.Poker);
                break;
            case Define.GameType.BlackJack:
                Managers.Scene.PhotonLoadScene(Define.Scene.BlackJack);
                break;
            default:
                break;
        }
        
        // 방에 들어왔으면 내 포톤 플레이어 정보 설정
        SetMyPhotonPlayerInfo(PhotonNetwork.LocalPlayer);
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
    
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(PhotonNetwork.IsMasterClient)                            // Room 입장과 Scene 입장은 별개이므로 N초의 로딩 시간 적용
            HoldemGameControl.Control.PlayerEnterHoldemRoom(1f, newPlayer);
    }

    /*
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)       // 일반 클라이언트 나간 경우
    {
        foreach (var view in PhotonNetwork.PhotonViewCollection)
        {
            // if(view.Owner == otherPlayer) 라고 하는 경우 소유권이 나가자마자 이전되서 항상 false가 됨.
            if (view.CreatorActorNr == otherPlayer.ActorNumber)     // 현재 오브젝트의 창조자가 나간 클라이언트인 경우
            {
                view.TransferOwnership(PhotonNetwork.MasterClient);     // 굳이 할 필요가 있을까???

                Player leavePlayer = view.gameObject.GetComponent<Player>();
                if (leavePlayer != null)
                {
                    PlayerManager.Inst.leavePlayerList.Add(leavePlayer.pIdx);

                    if (TurnManager.Inst.isNowGameStarted)  // 현재 게임중일때
                    {
                        if (leavePlayer.isActive)           // 살아있다면
                        {
                            if (leavePlayer.myTurn) PlayerManager.Inst.BetProcess("Die");       // 현재 나간 플레이어 턴인 경우
                            else leavePlayer.getOutReserve = true;                             // 다른 플레이어 턴인 경우
                        }
                    }
                    else
                    {                                       // 게임 시작 안했을땐 바로 나가는 처리
                        PlayerManager.Inst.LeavePlayerProcess();
                    }
                }
            }
        }
    }


    public override void OnMasterClientSwitched(Photon.Realtime.Player newMaster)   // 마스터 클라이언트 나간 경우
    {
        foreach (PhotonView view in PhotonNetwork.PhotonViewCollection)
        {
            if (view.IsMine)  // 이전 마스터가 소유한 오브젝트라면
            {
                view.TransferOwnership(newMaster);
            }
        }
    }
     */
}
