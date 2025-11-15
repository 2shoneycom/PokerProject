using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public static int DEBUG_INDEX = 0;   // 디버깅 용 (25.11.12 승헌)

    public enum Scene
    {
        Unknown,
        Login,
        Holdem,
        Lobby,
        Friend,
        PlayerInfo,
        Poker,
        BlackJack,
    }

    public enum UIEvent
    {
        Click,
        Drag,
    }

    public enum Status
    {
        Offline,
        Online,
        Playing,
    }

    public enum Difficulty
    {
        None,
        Beginner,
        Amateur,
        Pro,
    }

    public enum GameType
    {
        None,
        Holdem,
        Poker,
        BlackJack,
    }

    public enum BGM
    {
        Login,
        Lobby,
        Friend,
        PlayerInfo,
        Game,
    }

    public enum SFX
    {
        Win,
        Lose,
        Button,
        Card,
    }
}
