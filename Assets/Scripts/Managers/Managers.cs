using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp.Server;

public class Managers : MonoBehaviour
{
    static Managers m_instance;
    static Managers Instance { get { Init(); return m_instance; } }

    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();
    SeatManager _seat = new SeatManager();
    UIManager _ui = new UIManager();
    LoginManager _login = new LoginManager();
    DBManager _db = new DBManager();
    AuthManager _auth = new AuthManager();
    RewardManager _reward = new RewardManager();
    NickNameManager _nickname = new NickNameManager();
    PhotonManager _photon;
    WebManager _web = new WebManager();

    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SeatManager Seat { get { return Instance._seat; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static LoginManager Login { get { return Instance._login; } }
    public static DBManager DB { get { return Instance._db; } }
    public static AuthManager Auth { get { return Instance._auth; } }
    public static RewardManager Reward { get { return Instance._reward; } }
    public static PhotonManager Photon { get { return Instance._photon; } }
    public static WebManager Web { get { return Instance._web;  }}
    public static NickNameManager NickName { get { return Instance._nickname; } }

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
                    //return BlackJackGameControl.Control.IsPlaying;
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
                //return BlackJackGameControl.Control.IsPlaying;
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
        }
    }

    static public void Clear()
    {
        Scene.Clear();
        UI.Clear();
    }
}
