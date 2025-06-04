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

    private string uid;
    private string nickName;
    private long seedMoney;

    public void SetUid(string value) => uid = value;
    public void SetNickName(string value) => nickName = value;
    public void SetSeedMoney(long value) => seedMoney = value;

    // 값 읽기용 getter도 필요하다면 추가
    public string GetUid() => uid;
    public string GetNickName() => nickName;
    public long GetSeedMoney() => seedMoney;

    public void UpdateMoney(long value)
    {
        seedMoney += value;
    }

    public void SetHoldemPlay()
    {
        SetNickName(Random.Range(10000, 100000).ToString());
        SetSeedMoney(100000);
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
        if (HoldemGameControl.Control.IsPlaying)
            SyncSystem.Sync.SyncHoldemPlayerSeedMoney(NowHoldemPlayer.GameIndex, (int)seedMoney);
    }

}
