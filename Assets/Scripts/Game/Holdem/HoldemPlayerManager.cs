using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldemPlayerManager
{
    string[] holdemPlayerUID;
    HoldemPlayer[] players;

    void Init()
    {
        holdemPlayerUID = new string[7];
        players = new HoldemPlayer[7];
    }
}
