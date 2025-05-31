using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldemPlayerManager
{
    string[] holdemPlayerUID;
    int _nowPlayerNum;
    public int NowPlayerNum { get { return _nowPlayerNum; } }

    public HoldemPlayerManager(int max_num)
    {
        holdemPlayerUID = new string[max_num];
    }

    public void UpdatePlayerUID(int seatIdx, string UID)
    {
        _nowPlayerNum++;

        if (UID == SeatManager.DEFAULT_NULL_SEAT)
        {
            UID = "";
            _nowPlayerNum--;
        }

        int gameIndex = HoldemGameControl.Control.ConvertUItoGame(seatIdx);
        holdemPlayerUID[gameIndex] = UID;
    }

    public string GetPlayerUID(int index)
    {
        return holdemPlayerUID[index];
    }
}
