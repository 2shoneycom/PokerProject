using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class HoldemScene : BaseScene
{
    UI_Holdem _holdemUI = null;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Holdem;
        _holdemUI = Managers.UI.ShowSceneUI<UI_Holdem>();

        StartCoroutine(Loading(0.01f));
    }

    IEnumerator Loading(float sec)
    {
        yield return new WaitForSeconds(sec);
        SeatInit();
    }

    void SeatInit()
    {
        Managers.Seat.Init(7);
    }

    public void UpdateAllSeatUI()
    {
        for(int i = 0; i < Managers.Seat.Seats.Count; i++)
        {
            _holdemUI.UpdatePlayerName(i + 1, Managers.Seat.Seats[i]);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && PhotonNetwork.IsMasterClient)
            Managers.Resource.PhotonInstantiate("GO", transform);
    }

    public override void Clear()
    {
        Debug.Log("Holdem Scene Clear");
    }
}
