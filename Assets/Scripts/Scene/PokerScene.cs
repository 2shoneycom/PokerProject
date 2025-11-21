using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PokerScene : GameScene
{
    const int MAX_PLAYER = 5;

    UI_Poker _pokerUI = null;
    PokerGameControl _control;
    SyncSystem _syncSystem;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Poker;
        _syncSystem = this.GetOrAddComponent<SyncSystem>();
        _control = this.GetOrAddComponent<PokerGameControl>();

        _syncSystem.SetPokerControl(_control);
        _control.SetSyncSystem(_syncSystem);

        _pokerUI = Managers.UI.ShowSceneUI<UI_Poker>();
        User.NowUser.SetPokerPlay();

        StartCoroutine(Loading(0.01f));
    }

    IEnumerator Loading(float sec)
    {
        yield return new WaitForSeconds(sec);
        SeatInit();
    }

    void SeatInit()
    {
        Managers.Seat.Init(MAX_PLAYER, _syncSystem);
    }

    public PokerGameControl GetControl()
    {
        return _control;
    }

    public override void UpdateSeatUI(int index, string nickname)
    {
        _pokerUI.UpdatePlayerName(index + 1, nickname);

        if (nickname != SeatManager.DEFAULT_NULL_SEAT)
            _pokerUI.UpdatePlayerIcon(index + 1, nickname);
        else
            _pokerUI.UpdatePlayerIcon(index + 1, nickname, true);
    }

    public override void UpdateBetUI(bool isOn)
    {
        _pokerUI.BetUISwitch(isOn);
    }

    public override void ReadyForGameStart(bool isOn = true)
    {
        _pokerUI.GameStartButtonOn();
    }

    void Update()
    {

    }

    public override void Clear()
    {
        Debug.Log("Poker Scene Clear");
    }

    public void RequestLeaveRoom()
    {
        // 방을 나가고 로비씬으로 이동
        Managers.Photon.LeaveRoom();
        Managers.Scene.LoadScene(Define.Scene.Lobby);
    }

    public override void OnMasterChanged()
    {

    }

    public override void OnPlayerLeft(string uid)
    {

    }
}
