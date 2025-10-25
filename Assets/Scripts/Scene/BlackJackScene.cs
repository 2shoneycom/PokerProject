using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlackJackScene : GameScene
{
    const int MAX_PLAYER = 5;

    UI_BlackJack _jackUI = null;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.BlackJack;
        _jackUI = Managers.UI.ShowSceneUI<UI_BlackJack>();
        this.GetOrAddComponent<JackGameControl>();
        this.GetOrAddComponent<SyncSystem>();

        User.NowUser.SetJackPlay();

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
        if (nickname == SeatManager.DEFAULT_NULL_SEAT)
        {
            _jackUI.UpdatePlayerName(index + 1, "");
            return;
        }

        _jackUI.UpdatePlayerName(index + 1, nickname);

        if(nickname != SeatManager.DEFAULT_NULL_SEAT)
            _jackUI.UpdatePlayerButton(index + 1);

        if (nickname == User.NowUser.GetNickName())
        {
            for (int i = 0; i < MAX_PLAYER; i++)
                _jackUI.UpdatePlayerButton(i + 1);
        }
    }

    public override void UpdateBetUI(bool isOn)
    {
        _jackUI.BetUISwitch(isOn);
    }

    public override void ReadyForGameStart(bool isOn = true)
    {
        _jackUI.GameStartButtonSetting();
    }

    void Update()
    {

    }

    public override void Clear()
    {
        Debug.Log("Jack Scene Clear");
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
