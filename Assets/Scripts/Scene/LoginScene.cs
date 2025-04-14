using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class LoginScene : BaseScene
{
    private Dictionary<string, RoomInfo> availableRooms = new Dictionary<string, RoomInfo>();
    UI_Login _loginUI = null;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;
        _loginUI = Managers.UI.ShowSceneUI<UI_Login>();
    }

    void Start()
    {
        StartCoroutine(Loading(0.01f));
        // 바로 포톤과 접속은 문제 없지만 UI 바인드에 시간이 걸려서
        // text를 바꾸지 못함. 그래서 로딩시간 필요함.
        // (로그를 띄워보니 UI스크립트의 Init을 실행도 하기전에 
        //  텍스트를 접근하려해서 null값을 건드리게됨)
    }

    IEnumerator Loading(float sec)
    {
        yield return new WaitForSeconds(sec);
        ConnectToPhoton();
    }

    void ConnectToPhoton()
    {
        PhotonNetwork.ConnectUsingSettings();
        // 같은 버전만 매칭 시도를 위해 게임 버전 설정        PhotonNetwork.GameVersion = gameVersion;
        // 설정한 정보로 마스터 서버 접속 시도
        // 접속 시도 중 표시
        _loginUI.SetConnectionInfoText("연결 중...");
    }

    public override void Clear()
    {
        Debug.Log("Login Scene Clear");
    }

    public override void OnConnectedToMaster()
    {   // 포톤 마스터 서버에 접속 성공한 경우 자동 실행됨.
        _loginUI.ButtonInteractive(true);
        _loginUI.SetConnectionInfoText("Online : Connected to Master Server");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {   // 마스터 서버 접속 실패 || 서버 접속 상태에서 접속이 끊긴 경우
        if (_loginUI.LobbyButton != null) 
            _loginUI.ButtonInteractive(false);
        Reconnect();
    }

    void Reconnect()
    {
        _loginUI.SetConnectionInfoText("Offline : Disconnected to Master Server\nReconnect..");
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
                Debug.Log("���� ���ŵ�: " + room.Name);
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
        _loginUI.LobbyButton.interactable = false;

        if (PhotonNetwork.IsConnected)
        {
            _loginUI.SetConnectionInfoText("Creating New Room..");
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
    public void JoinRoom()
    {   // 버튼을 클릭시 실행할 메서드. 빈 무작위 룸에 접속 시도함.
        // 중복 접속 막기위해 버튼 비활성화

        if (PhotonNetwork.IsConnected)
        {
            _loginUI.SetConnectionInfoText("Entering Room..");
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            Reconnect();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {   // 랜덤 룸 접속에 실패한 경우 (서버 연결 안끊김)
        _loginUI.SetConnectionInfoText("No Available Room..");
        Debug.Log("No available room");
        // 최대 7명을 수용 가능한 빈 방 생성
        CreateRoom();
    }

    public override void OnJoinedRoom()
    {   // 룸 참가에 성공한 경우
        _loginUI.SetConnectionInfoText("Success to Enter Room");
        // 모든 룸 참가자가 GameRoom 씬을 로드하게함
        Managers.Scene.PhotonLoadScene(Define.Scene.Holdem);
        // 씬메니저로 로드하면 연결 정보가 사라짐.
    }
}
