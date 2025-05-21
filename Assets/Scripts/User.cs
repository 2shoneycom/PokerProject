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

    public string nickName;
    public long seedMoney; // private으로 nickName이랑 seedMoney


    public void SetHoldemPlay()
    {
        nickName = Random.Range(10000, 100000).ToString();
        seedMoney = 1000000;
        holdemPlayer = new HoldemPlayer();
    }

    public int GetHoldemSeat()
    {
        return holdemPlayer.SeatIndex;
    }

    public void SetHoldemSeat(int idx)
    {
        holdemPlayer.SeatIndex = idx;
    }
}
