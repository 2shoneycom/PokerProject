using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Google;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using System;
using Firebase.Database;

public class DBManager : MonoBehaviour
{
    public DatabaseReference dbRef;
    private static DBManager instance;
    public static DBManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("DBManager");
                instance = obj.AddComponent<DBManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private void Awake()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public Dictionary<string, object> GetUserInfo()
    {
        return new Dictionary<string, object>
        {
            {"uid", LoginManager.Instance.userId},
            {"nickname", User.Instance.nickName},
            {"seedMoney", User.Instance.seedMoney}
        };
    }

}