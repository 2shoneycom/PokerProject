using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using UnityEngine;
using Firebase.Database;

public class RewardManager : MonoBehaviour
{
    public void DailyGift()
    {
        Debug.Log("daily gift start");

        string userId = LoginManager.Instance.userId;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set");
            return;
        }

        var userRef = DBManager.Instance.dbRef.Child("Users").Child(userId);

        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Data fetch failed: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;
            Debug.Log("data loaded success");

            string jsonData = snapshot.GetRawJsonValue();
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError("jsonData is null or empty!");
                return;
            }

            Debug.Log("jsonData: " + jsonData);

            DataToSave loadedData = JsonUtility.FromJson<DataToSave>(jsonData);
            if (loadedData == null)
            {
                Debug.LogError("Failed to parse data to DataToSave");
                return;
            }

            Debug.Log("Parsed Data - reward: " + loadedData.reward + ", seedMoney: " + loadedData.seedMoney);

            if (loadedData.reward)
            {
                Debug.Log("You already received");
            }
            else
            {
                Dictionary<string, object> updateData = new Dictionary<string, object>()
                {
                    { "nickName", loadedData.nickName },
                    { "reward", true },
                    { "seedMoney", loadedData.seedMoney += 1_000_000 }
                };

                userRef.SetValueAsync(updateData).ContinueWithOnMainThread(saveTask =>
                {
                    if (saveTask.IsFaulted || saveTask.IsCanceled)
                    {
                        Debug.LogError("data save fail: " + saveTask.Exception);
                    }
                    else
                    {
                        Managers.User.seedMoney = loadedData.seedMoney;
                        Debug.Log("data save success");
                    }
                });
            }
        });
    }

}
