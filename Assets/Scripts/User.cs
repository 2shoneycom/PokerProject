using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User
{
    private static User user = new User();
    public static User NowUser
    {
        get
        {
            return user;
        }
    }

    HoldemPlayer holdemPlayer;
    public static HoldemPlayer NowHoldemPlayer { get { return NowUser.holdemPlayer; } }

    public string nickName;
    public long seedMoney; // private으로 nickName이랑 seedMoney


    public void SetHoldemPlay()
    {
        nickName = Random.Range(10000, 100000).ToString();
        seedMoney = 1000000;
        holdemPlayer = new HoldemPlayer();
    }

    public void DecreaseMoney(string targetUID, int amount)
    {
        if (targetUID != nickName)
            return;

        //////////////////////////////// DB와 소통
        seedMoney -= amount;
        HoldemSyncSeedMoney();
    }

    public void IncreaseMoney(string targetUID, int amount)
    {
        if (targetUID != nickName)
            return;

        //////////////////////////////// DB와 소통
        seedMoney += amount;
        HoldemSyncSeedMoney();
    }

    public void HoldemBettingMoney(string targetUID, int amount)
    {
        if (targetUID != nickName)
            return;

        //////////////////////////////// DB와 소통
        seedMoney -= amount;
        HoldemSyncSeedMoney();
        NowHoldemPlayer.SetBetMoney(NowHoldemPlayer.BetMoney + amount);
    }

    public void HoldemSyncSeedMoney()      // seedmoney 수정시 항상 호출
    {
        if(HoldemGameControl.Control.IsPlaying)
            SyncSystem.Instacne.SyncHoldemPlayerSeedMoney(NowHoldemPlayer.GameIndex, (int)seedMoney);
    }
}
