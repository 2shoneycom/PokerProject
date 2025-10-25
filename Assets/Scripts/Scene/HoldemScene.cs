using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;


public class HoldemScene : GameScene
{
    const int MAX_PLAYER = 7;

    UI_Holdem _holdemUI = null;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Holdem;
        _holdemUI = Managers.UI.ShowSceneUI<UI_Holdem>();
        this.GetOrAddComponent<HoldemGameControl>();
        this.GetOrAddComponent<SyncSystem>();

        User.NowUser.SetHoldemPlay();

        StartCoroutine(Loading(0.01f));
    }

    IEnumerator Loading(float sec)
    {
        yield return new WaitForSeconds(sec);
        SeatInit();
    }

    void SeatInit()
    {
        Managers.Seat.Init(MAX_PLAYER);
    }

    public override void UpdateSeatUI(int index, string nickname)
    {
        _holdemUI.UpdatePlayerName(index + 1, nickname);

        if (nickname != SeatManager.DEFAULT_NULL_SEAT)
            _holdemUI.UpdatePlayerIcon(index + 1, nickname);
        else
            _holdemUI.UpdatePlayerIcon(index + 1, nickname, true);
    }

    public override void UpdateBetUI(bool isOn)
    {
        _holdemUI.BetUISwitch(isOn);
    }

    public override void ReadyForGameStart()
    {
        _holdemUI.GameStartButtonOn();
    }

    void Update()
    {

    }

    public override void Clear()
    {
        Debug.Log("Holdem Scene Clear");
    }

    public void RequestLeaveRoom()
    {
        // 방을 나가고 로비씬으로 이동
        Managers.Photon.LeaveRoom();
        Managers.Scene.LoadScene(Define.Scene.Lobby);
        Managers.DB.SetUserStatus(Define.Status.Online);    // 홀덤씬 -> 로비씬 (status: online)
    }

    public override void OnMasterChanged()
    {
        // 마스터가 나간 경우 처리
    }

    public override void OnPlayerLeft(string uid)
    {
        // 1. 카드 받기도 전에 나간 경우 -> 그냥 카드까지 받게 하고 die 처리
        // 2. 내 차례가 아닌 딜링 하는 경우 -> die 처리
        // 3. 내 차례 였던 경우 -> die 처리
        if (HoldemGameControl.Control.IsPlaying == false) return;
        if (PhotonNetwork.IsMasterClient == false) return;

        int gameIndex = HoldemGameControl.Players.GetPlayerGameIndexByUID(uid);
        if (HoldemGameControl.Bet.CurBetPlayer == gameIndex)
            HoldemGameControl.Bet.PlayerBetSelected("Die");
        else
            SyncSystem.Sync.SyncHoldemDieReserve(User.NowGamePlayer.GameIndex, true);
    }
}
