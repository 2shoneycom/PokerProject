using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;


public class HoldemScene : BaseScene
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

    public void UpdateAllSeatUI()
    {
        for(int i = 0; i < Managers.Seat.Seats.Count; i++)
        {
            _holdemUI.UpdatePlayerName(i + 1, Managers.Seat.Seats[i]);
        }
    }

    public void ReadyForGameStart()
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
}
