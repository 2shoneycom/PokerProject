using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        Managers.Audio.PlayBGM(Define.BGM.Game);
    }

    public override void Clear()
    {

    }

    public abstract void UpdateSeatUI(int index, string nickname);
    public abstract void ReadyForGameStart();
    public abstract void UpdateBetUI(bool isOn);
    public abstract void OnMasterChanged();
    public abstract void OnPlayerLeft(string uid);
}
