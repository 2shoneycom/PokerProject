using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers m_instance;
    static Managers Instance { get { Init(); return m_instance; } }

    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();
    SeatManager _seat = new SeatManager();
    UIManager _ui = new UIManager();
    PhotonManager _photon;

    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SeatManager Seat { get { return Instance._seat; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static PhotonManager Photon { get { return Instance._photon; } }

    User _user = new User();
    public static User User { get { return Instance._user; } }


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
        }
    }

    static public void Clear()
    {
        Scene.Clear();
        UI.Clear();
    }
}
