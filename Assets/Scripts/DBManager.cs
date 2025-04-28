using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Google;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Database;
using System.Net;

public class DataToSave
{
    public string nickName;
    public long seedMoney;
    public bool reward;

    public DataToSave() { }
    public DataToSave(string nickName, long seedMoney, bool reward)
    {
        this.nickName = nickName;
        this.seedMoney = seedMoney;
        this.reward = reward;
    }
}

public class DBManager : MonoBehaviour
{
    public DatabaseReference dbRef;
    public DataToSave dts;
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

    private void DataSetting()
    {
        // �ʱ� ������ ����
        dts = new DataToSave(
            "User" + Random.Range(10000, 100000),
            1_000_000L,
            false
            );
    }

    public void GetUserInfo()
    {
        if (string.IsNullOrEmpty(LoginManager.Instance.userId))
        {
            Debug.LogError("User ID is not set");
            return;
        }

        // �����ͺ��̽����� ����� ���� ��ȸ
        DBManager.Instance.dbRef.Child("Users").Child(LoginManager.Instance.userId)
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("data search fail: " + task.Exception);
                    return;
                }

                DataSnapshot snapshot = task.Result;

                // ���� �����Ͱ� �ִ� ���
                if (snapshot.Exists)
                {
                    // JSON ������ �Ľ�
                    string jsonData = snapshot.GetRawJsonValue();
                    DataToSave loadedData = JsonUtility.FromJson<DataToSave>(jsonData);

                    // ������ ����
                    User.Instance.nickName = loadedData.nickName;
                    User.Instance.seedMoney = loadedData.seedMoney;
                    Debug.Log("user data load success");
                }
                // ���ο� ������� ���
                else
                {
                    DataSetting(); // �ʱ� ������ ����
                    SaveNewUserData(); // �����ͺ��̽��� ����
                    User.Instance.nickName = dts.nickName;
                    User.Instance.seedMoney = dts.seedMoney;
                    Debug.Log("new user data create success");
                }
            });
    }

    private void SaveNewUserData()
    {
        var defaultData = new Dictionary<string, object>
        {
            { "nickName", dts.nickName },
            { "seedMoney", dts.seedMoney },
            { "reward", dts.reward }
        };

        DBManager.Instance.dbRef.Child("Users").Child(LoginManager.Instance.userId)
            .SetValueAsync(defaultData).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("data save fail: " + task.Exception);
                }
            });
    }
}