using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldemPlayerManager
{
    string[] holdemPlayerUID;

    void Init()
    {
        holdemPlayerUID = new string[7];
    }

    public void UpdatePlayerUID(int seatIdx, string UID)
    {
        if (UID == "자리 없음")
            UID = "";

        holdemPlayerUID[seatIdx] = UID;
    }
}
