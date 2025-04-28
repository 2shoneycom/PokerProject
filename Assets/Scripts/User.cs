using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class User : MonoBehaviour
{
    private static User instance;
    public static User Instance { get { return instance; } }
    public string nickName;
    public long seedMoney; // private으로 nickName이랑 seedMoney

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject); // 씬이 넘어가도 유지하려면 필요
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}