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
    public long seedMoney; // private���� nickName�̶� seedMoney

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject); // ���� �Ѿ�� �����Ϸ��� �ʿ�
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}