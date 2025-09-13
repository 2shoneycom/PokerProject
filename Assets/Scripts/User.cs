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

    GamePlayer gamemPlayer;
    public static GamePlayer NowGamePlayer { get { return NowUser.gamemPlayer; } }

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
        //SetUid(Random.Range(100000, 1000000).ToString());
        //SetNickName(Random.Range(10000, 100000).ToString());
        //SetSeedMoney(100000);
        gamemPlayer = new GamePlayer();
        Managers.CurrentGameType = Define.GameType.Holdem;
    }

    public void SetPokerPlay()
    {
        gamemPlayer = new GamePlayer();
        Managers.CurrentGameType = Define.GameType.Poker;
    }

    public void SetJackPlay()
    {
        gamemPlayer = new GamePlayer();
        Managers.CurrentGameType = Define.GameType.BlackJack;
    }

    public void DecreaseMoney(string targetUID, int amount)
    {
        if (targetUID != uid)
            return;

        //////////////////////////////// DB와 소통
        seedMoney -= amount;

        switch (Managers.CurrentGameType)
        {
            case Define.GameType.Holdem:
                Managers.DB.DBUpdateMoney(uid, -amount, "holdem");
                HoldemSyncSeedMoney();
                break;
            case Define.GameType.Poker:
                Managers.DB.DBUpdateMoney(uid, -amount, "poker");
                PokerSyncSeedMoney();
                break;
            case Define.GameType.BlackJack:
                Managers.DB.DBUpdateMoney(uid, -amount, "black");
                JackSyncSeedMoney();
                break;
        }
    }

    public void IncreaseMoney(string targetUID, int amount)
    {
        if (targetUID != uid)
            return;

        //////////////////////////////// DB와 소통
        seedMoney += amount;

        switch (Managers.CurrentGameType)
        {
            case Define.GameType.Holdem:
                Managers.DB.DBUpdateMoney(uid, amount, "holdem");
                HoldemSyncSeedMoney();
                break;
            case Define.GameType.Poker:
                Managers.DB.DBUpdateMoney(uid, amount, "poker");
                PokerSyncSeedMoney();
                break;
            case Define.GameType.BlackJack:
                Managers.DB.DBUpdateMoney(uid, amount, "black");
                JackSyncSeedMoney();
                break;
        }
    }

    public void HoldemBettingMoney(string targetUID, int amount)
    {
        if (targetUID != uid)
            return;

        //////////////////////////////// DB와 소통
        seedMoney -= amount;
        Managers.DB.DBUpdateMoney(uid, -amount, "holdem");
        NowGamePlayer.SetBetMoney(NowGamePlayer.BetMoney + amount);
        HoldemSyncSeedMoney();
    }

    public void HoldemSyncSeedMoney()      // seedmoney 수정시 항상 호출
    {
        if (HoldemGameControl.Control.IsPlaying)
            SyncSystem.Sync.SyncHoldemPlayerSeedMoney(NowGamePlayer.GameIndex, (int)seedMoney);
    }

    public void PokerBettingMoney(string targetUID, int amount)
    {
        if (targetUID != uid)
            return;

        //////////////////////////////// DB와 소통
        seedMoney -= amount;
        Managers.DB.DBUpdateMoney(uid, -amount, "seven");
        NowGamePlayer.SetBetMoney(NowGamePlayer.BetMoney + amount);
        PokerSyncSeedMoney();
    }

    public void PokerSyncSeedMoney()
    {
        if (PokerGameControl.Control.IsPlaying)
            SyncSystem.Sync.SyncPokerPlayerSeedMoney(NowGamePlayer.GameIndex, (int)seedMoney);
    }

    public void JackBettingMoney(string targetUID, int amount)
    {
        if (targetUID != uid)
            return;

        //////////////////////////////// DB와 소통
        seedMoney -= amount;
        Managers.DB.DBUpdateMoney(uid, -amount, "blackjack");
        NowGamePlayer.SetBetMoney(NowGamePlayer.BetMoney + amount);
        JackSyncSeedMoney();
    }

    public void JackResetBetting()
    {
        int betAmount = NowGamePlayer.BetMoney;

        seedMoney += betAmount;
        Managers.DB.DBUpdateMoney(uid, betAmount, "blackjack");
        NowGamePlayer.SetBetMoney(0);
        JackSyncSeedMoney();
    }

    public void JackSyncSeedMoney()
    {
        if (JackGameControl.Control.IsPlaying)
            SyncSystem.Sync.SyncJackPlayerSeedMoney(NowGamePlayer.GameIndex, (int)seedMoney);
    }

}
