using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WebSocketSharp.Server;
using static Define;

public class Managers : MonoBehaviour
{
    static Managers m_instance;
    static public Managers Instance { get { Init(); return m_instance; } }

    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();
    SeatManager _seat = new SeatManager();
    UIManager _ui = new UIManager();
    LoginManager _login = new LoginManager();
    DBManager _db = new DBManager();
    AuthManager _auth = new AuthManager();
    NickNameManager _nickname = new NickNameManager();
    PhotonManager _photon;
    WebManager _web = new WebManager();
    AudioManager _audio = new AudioManager();

    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SeatManager Seat { get { return Instance._seat; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static LoginManager Login { get { return Instance._login; } }
    public static DBManager DB { get { return Instance._db; } }
    public static AuthManager Auth { get { return Instance._auth; } }
    public static PhotonManager Photon { get { return Instance._photon; } }
    public static WebManager Web { get { return Instance._web; } }
    public static NickNameManager NickName { get { return Instance._nickname; } }
    public static AudioManager Audio { get { return Instance._audio; } }

    Define.GameType gameType = Define.GameType.None;
    public static Define.GameType CurrentGameType
    {
        get { return m_instance.gameType; }
        set { m_instance.gameType = value; }
    }

    Define.Difficulty difficulty = Define.Difficulty.None;
    public static Define.Difficulty CurrentDifficulty
    {
        get { return m_instance.difficulty; }
        set { m_instance.difficulty = value; }
    }

    public static Dictionary<Tuple<Define.GameType, Define.Difficulty>, int> GameBaseBet;

    public static bool IsNowPlayingGame
    {
        get {
            switch (CurrentGameType)
            {
                case Define.GameType.Holdem:
                    return HoldemGameControl.Control.IsPlaying;
                case Define.GameType.Poker:
                    return PokerGameControl.Control.IsPlaying;
                case Define.GameType.BlackJack:
                    return JackGameControl.Control.IsPlaying;
                default:
                    return false;
            }
        }
    }

    public static int GetCurGameMaxPlayer
    {
        get
        {
            switch (CurrentGameType)
            {
                case Define.GameType.Holdem:
                    return HoldemGameControl.MAX_PLAYER_NUM;
                case Define.GameType.Poker:
                    return PokerGameControl.MAX_PLAYER_NUM;
                case Define.GameType.BlackJack:
                    return JackGameControl.MAX_PLAYER_NUM;
                default:
                    return 0;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
        _photon = FindAnyObjectByType<PhotonManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Audio.PlaySFX(Define.SFX.Button);
    }

    void OnDestroy()
    {
        Scene.OnDestroy();
    }

    static void Init()
    {
        if (m_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            m_instance = go.GetComponent<Managers>();

            DB.Init();
            Scene.Init();
            Audio.Init();

            GameBaseBet = new Dictionary<Tuple<Define.GameType, Define.Difficulty>, int>();
            InitGameBaseBet();
        }
    }

    static void InitGameBaseBet()
    {
        foreach (GameType type in Enum.GetValues(typeof(GameType)))
        {
            if(type == GameType.None) continue;

            foreach (Difficulty diff in Enum.GetValues(typeof(Difficulty)))
            {
                if (diff == Difficulty.None) continue;

                switch (type)
                {
                    case Define.GameType.Holdem:
                    case Define.GameType.Poker:
                        {
                            switch (diff)
                            {
                                case Define.Difficulty.Beginner:
                                    GameBaseBet[Tuple.Create(type, diff)] = 1000;
                                    break;
                                case Define.Difficulty.Amateur:
                                    GameBaseBet[Tuple.Create(type, diff)] = 10000;
                                    break;
                                case Define.Difficulty.Pro:
                                    GameBaseBet[Tuple.Create(type, diff)] = 100000;
                                    break;
                            }
                            break;
                        }
                    case Define.GameType.BlackJack:
                        {
                            switch (diff)
                            {
                                case Define.Difficulty.Beginner:
                                    GameBaseBet[Tuple.Create(type, diff)] = 500;
                                    break;
                                case Define.Difficulty.Amateur:
                                    GameBaseBet[Tuple.Create(type, diff)] = 5000;
                                    break;
                                case Define.Difficulty.Pro:
                                    GameBaseBet[Tuple.Create(type, diff)] = 50000;
                                    break;
                            }
                            break;
                        }
                }
            }
        }
    }

    public static int GetCurGameBaseBet()
    {
        return GameBaseBet[Tuple.Create(CurrentGameType, CurrentDifficulty)];
    }

    static public void Clear()
    {
        Scene.Clear();
        UI.Clear();
        Audio.Claer();
    }
}
