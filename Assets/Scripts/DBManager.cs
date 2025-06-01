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
using System;

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
        // 초기 데이터 설정
        dts = new DataToSave(
            "User" + UnityEngine.Random.Range(10000, 100000),
            1_000_000L,
            false
            );
    }

    public void GetUserInfo()
    {
        if (string.IsNullOrEmpty(AuthManager.Instance.userId))
        {
            Debug.LogError("User ID is not set");
            return;
        }

        // 데이터베이스에서 사용자 정보 조회
        dbRef.Child("Users").Child(AuthManager.Instance.userId)
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("data search fail: " + task.Exception);
                    return;
                }

                DataSnapshot snapshot = task.Result;
                Managers.User.SetUid(AuthManager.Instance.userId);

                // 기존 데이터가 있는 경우
                if (snapshot.Exists)
                {
                    // JSON 데이터 파싱
                    string jsonData = snapshot.GetRawJsonValue();
                    DataToSave loadedData = JsonUtility.FromJson<DataToSave>(jsonData);

                    // 데이터 적용
                    Managers.User.SetNickName(loadedData.nickName);
                    Managers.User.SetSeedMoney(loadedData.seedMoney);
                    Debug.Log("user data load success");
                }
                // 새로운 사용자인 경우
                else
                {
                    DataSetting(); // 초기 데이터 생성
                    SaveNewUserData(); // 데이터베이스에 저장
                    Managers.User.SetNickName(dts.nickName);
                    Managers.User.SetSeedMoney(dts.seedMoney);
                    Debug.Log("new user data create success");
                }
            });
    }

    private void SaveNewUserData()
    {
        // Users 저장 (기존 코드 유지)
        var defaultData = new Dictionary<string, object>
    {
        { "nickName", dts.nickName },
        { "seedMoney", dts.seedMoney },
        { "reward", dts.reward }
    };

        dbRef.Child("Users").Child(AuthManager.Instance.userId)
            .SetValueAsync(defaultData).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError("Users 저장 실패: " + task.Exception);
            });

        // MoneyRank 저장 방식 수정
        string userId = AuthManager.Instance.userId;
        var tasks = new List<Task>();

        string[] gameTypes = { "holdem", "seven", "blackjack" };
        foreach (string gameType in gameTypes)
        {
            tasks.Add(InitializeMoneyRank(userId, gameType));
        }

        Task.WhenAll(tasks).ContinueWithOnMainThread(combinedTask =>
        {
            if (combinedTask.IsFaulted)
                Debug.LogError("MoneyRank 처리 실패: " + combinedTask.Exception);
            else
                Debug.Log("MoneyRank 처리 완료");
        });
    }

    private async Task InitializeMoneyRank(string userId, string gameType)
    {
        DatabaseReference rankRef = dbRef.Child("MoneyRank").Child(gameType).Child(userId);

        try
        {
            // 1. 기존 데이터 존재 여부 확인
            DataSnapshot snapshot = await rankRef.GetValueAsync();

            // 2. 데이터가 없는 경우에만 초기화
            if (!snapshot.Exists)
            {
                await rankRef.SetValueAsync(0L);
                Debug.Log($"[{gameType}] 초기값 0 설정");
            }
            else
            {
                Debug.Log($"[{gameType}] 기존 값 유지: {snapshot.Value}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{gameType}] 처리 오류: {ex.Message}");
        }
    }

    public void DeleteUserData(string userId, Action<bool, string> onComplete = null)
    {
        dbRef.Child("Users").Child(userId).RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log($"DBManager: 사용자 {userId} 데이터 삭제 성공");
                onComplete?.Invoke(true, null);
            }
            else
            {
                string error = task.Exception?.Message ?? "Unknown DB error";
                Debug.LogError($"DBManager: 사용자 {userId} 데이터 삭제 실패 - {error}");
                onComplete?.Invoke(false, error);
            }
        });
    }

    public void DBUpdateMoney(string uid, long amount, string gameType)
    {
        // 1. Users/{uid}/seedMoney 업데이트
        UpdateUserSeedMoney(uid, amount);

        // 2. MoneyRank/{gameType}/{uid} 업데이트
        UpdateMoneyRank(uid, amount, gameType);
    }

    private void UpdateUserSeedMoney(string uid, long amount)
    {
        dbRef.Child("Users").Child(uid).Child("seedMoney").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("seedMoney 조회 실패: " + task.Exception);
                return;
            }

            long currentValue = 0;
            if (task.Result.Exists && long.TryParse(task.Result.Value.ToString(), out long value))
                currentValue = value;

            long newValue = currentValue + amount;

            dbRef.Child("Users").Child(uid).Child("seedMoney").SetValueAsync(newValue).ContinueWithOnMainThread(setTask =>
            {
                if (setTask.IsFaulted)
                    Debug.LogError("seedMoney 저장 실패: " + setTask.Exception);
                else
                    Debug.Log("seedMoney 저장 성공: " + newValue);
            });
        });
    }

    private void UpdateMoneyRank(string uid, long amount, string gameType)
    {
        dbRef.Child("MoneyRank").Child(gameType).Child(uid).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("MoneyRank 조회 실패: " + task.Exception);
                return;
            }

            long currentValue = 0;
            if (task.Result.Exists && long.TryParse(task.Result.Value.ToString(), out long value))
                currentValue = value;

            long newValue = currentValue + amount;

            dbRef.Child("MoneyRank").Child(gameType).Child(uid).SetValueAsync(newValue).ContinueWithOnMainThread(setTask =>
            {
                if (setTask.IsFaulted)
                    Debug.LogError("MoneyRank 저장 실패: " + setTask.Exception);
                else
                    Debug.Log("MoneyRank 저장 성공: " + newValue);
            });
        });
    }

    public async Task<(string, long)> GetPlayerInfo(string uid)
    {
        try
        {
            DataSnapshot snapshot = await dbRef.Child("Users").Child(uid).GetValueAsync();

            if (snapshot.Exists)
            {
                string jsonData = snapshot.GetRawJsonValue();
                DataToSave loadedData = JsonUtility.FromJson<DataToSave>(jsonData);
                Debug.Log("user data load success");
                return (loadedData.nickName, loadedData.seedMoney);
            }
            else
            {
                Debug.Log("데이터가 존재하지 않음");
                return (null, 0); // 기본값 반환
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"데이터 조회 실패: {ex.Message}");
            return (null, 0);
        }
    }
}