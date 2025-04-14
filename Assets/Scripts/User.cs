using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

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
public class User : MonoBehaviour
{
    private static User instance;
    public static User Instance { get { return instance; } }

    public DataToSave dts;
    public string nickName;
    public long seedMoney;
    private bool connectionStatus;

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

    private void DataSetting()
    {
        // �ʱ� ������ ����
        dts = new DataToSave(
            "User"+ Random.Range(10000, 100000),
            1_000_000L,
            false
            );
    }

    // User.cs
    public void SetUserInfo()
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
                    nickName = loadedData.nickName;
                    seedMoney = loadedData.seedMoney;
                    Debug.Log("user data load success");
                }
                // ���ο� ������� ���
                else
                {
                    DataSetting(); // �ʱ� ������ ����
                    SaveNewUserData(); // �����ͺ��̽��� ����
                    nickName = dts.nickName;
                    seedMoney = dts.seedMoney;
                    Debug.Log("new user data create success");
                }
            });
    }

    // �� ����� ������ ���� �޼���
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
