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

    HoldemGameControl holdemControl;
    PokerGameControl pokerControl;
    JackGameControl jackControl;

    private string uid;
    private string nickName;
    private long seedMoney;
    private bool isDailyClaimed;
    private int streak;
    private bool isNotEnoughMoney;

    public void SetUid(string value) => uid = value;
    public void SetNickName(string value) => nickName = value;
    public void SetSeedMoney(long value) => seedMoney = value;
    public void SetisDailyClaimed(bool value) => isDailyClaimed = value;
    public void Setstreak(int value) => streak = value;

    // 값 읽기용 getter도 필요하다면 추가
    public string GetUid() => uid;
    public string GetNickName() => nickName;
    public long GetSeedMoney() => seedMoney;
    public bool GetisDailyClaimed() => isDailyClaimed;
    public int Getstreak() => streak;

    public void UpdateMoney(long value)
    {
        seedMoney += value;
    }

    public void SetIsNotEnough(bool value)
    {
        isNotEnoughMoney = value;
    }

    public bool GetIsNotEnough()
    {
        if (IsEnoughMoney())
        {
            SetIsNotEnough(true);
        }

        return isNotEnoughMoney;
    }

    public bool IsEnoughMoney()
    {
        if (GetSeedMoney() < Managers.GetCurGameBaseBet())
            return false;

        return true;
    }

    public void SetHoldemPlay()
    {
        HoldemScene holdemScene = (HoldemScene)Managers.Scene.CurrentScene;
        holdemControl = holdemScene.GetControl();

        gamemPlayer = new GamePlayer(holdemControl);
        Managers.CurrentGameType = Define.GameType.Holdem;
    }

    public void SetPokerPlay()
    {
        PokerScene pokerScene = (PokerScene)Managers.Scene.CurrentScene;
        pokerControl = pokerScene.GetControl();

        gamemPlayer = new GamePlayer(pokerControl);
        Managers.CurrentGameType = Define.GameType.Poker;
    }

    public void SetJackPlay()
    {
        BlackJackScene jackScene = (BlackJackScene)Managers.Scene.CurrentScene;
        jackControl = jackScene.GetControl();
         
        gamemPlayer = new GamePlayer(jackControl);
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
                Managers.DB.DBUpdateMoney(uid, -amount, "blackjack");
                JackSyncSeedMoney();
                break;
        }
    }

    public void IncreaseMoney(string targetUID, int amount)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} User.cs 파일의 IncreaseMoney 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

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
                Managers.DB.DBUpdateMoney(uid, amount, "blackjack");
                JackSyncSeedMoney();
                break;
        }
    }

    public void HoldemBettingMoney(string targetUID, int amount)
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} User.cs 파일의 HoldemBettingMoney 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        if (targetUID != uid)
            return;

        //////////////////////////////// DB와 소통
        seedMoney -= amount;
        Managers.DB.DBUpdateMoney(uid, -amount, "holdem");
        NowGamePlayer.SetBetMoney(amount);
        HoldemSyncSeedMoney();
    }

    public void HoldemSyncSeedMoney()      // seedmoney 수정시 항상 호출
    {
        Debug.Log($"#{++Define.DEBUG_INDEX} User.cs 파일의 HoldemSyncSeedMoney 함수 실행"); // 디버깅 추적용 (25.11.12 승헌)

        if (holdemControl == null)
            return;

        if (holdemControl.IsPlaying)
            holdemControl.Sync.SyncHoldemPlayerSeedMoney(NowGamePlayer.GameIndex, (int)seedMoney);
    }

    public void PokerBettingMoney(string targetUID, int amount)
    {
        if (targetUID != uid)
            return;

        //////////////////////////////// DB와 소통
        seedMoney -= amount;
        Managers.DB.DBUpdateMoney(uid, -amount, "poker");
        NowGamePlayer.SetBetMoney(amount);
        PokerSyncSeedMoney();
    }

    public void PokerSyncSeedMoney()
    {
        if (pokerControl.IsPlaying)
            pokerControl.Sync.SyncPokerPlayerSeedMoney(NowGamePlayer.GameIndex, (int)seedMoney);
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
        if (jackControl.IsPlaying)
            jackControl.Sync.SyncJackPlayerSeedMoney(NowGamePlayer.GameIndex, (int)seedMoney);
    }
}
