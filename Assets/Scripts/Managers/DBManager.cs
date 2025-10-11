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
using System.Runtime.Serialization;

public class DataToSave
{
    public string nickName;
    public long seedMoney;
    public bool reward;
    public string nickNameUpdatedDate;
    public Dictionary<string, object> invitation;

    public DataToSave() { }
    public DataToSave(string nickName, long seedMoney, bool reward, string nickNameUpdatedDate = "20000101")
    {
        this.nickName = nickName;
        this.seedMoney = seedMoney;
        this.reward = reward;
        this.nickNameUpdatedDate = nickNameUpdatedDate;
        this.invitation = new Dictionary<string, object>();
    }
}

public class DBManager
{
    public DatabaseReference dbRef;
    private DatabaseReference inviteRef;
    public DataToSave dts;

    public void Init()
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
        if (string.IsNullOrEmpty(Managers.Auth.userId))
        {
            Debug.LogError("User ID is not set");
            return;
        }

        // 데이터베이스에서 사용자 정보 조회
        dbRef.Child("Users").Child(Managers.Auth.userId)
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("data search fail: " + task.Exception);
                    return;
                }

                DataSnapshot snapshot = task.Result;
                User.NowUser.SetUid(Managers.Auth.userId);

                // 기존 데이터가 있는 경우
                if (snapshot.Exists)
                {
                    // JSON 데이터 파싱
                    string jsonData = snapshot.GetRawJsonValue();
                    DataToSave loadedData = JsonUtility.FromJson<DataToSave>(jsonData);

                    // 데이터 적용
                    User.NowUser.SetNickName(loadedData.nickName);
                    User.NowUser.SetSeedMoney(loadedData.seedMoney);
                    Debug.Log("user data load success");
                }
                // 새로운 사용자인 경우
                else
                {
                    DataSetting(); // 초기 데이터 생성
                    SaveNewUserData(); // 데이터베이스에 저장
                    User.NowUser.SetNickName(dts.nickName);
                    User.NowUser.SetSeedMoney(dts.seedMoney);
                    Debug.Log("new user data create success");
                }

                // 게임 초대 관련 DB에 콜백함수 연결
                inviteRef = dbRef.Child("Users").Child(User.NowUser.GetUid()).Child("invitation");
                inviteRef.ValueChanged += HandleInviteAdded;
            });
    }

    private void SaveNewUserData()
    {
        // Users 저장 (기존 코드 유지)
        var defaultData = new Dictionary<string, object>
    {
        { "nickName", dts.nickName },
        { "seedMoney", dts.seedMoney },
        { "reward", dts.reward },
        { "nickNameUpdatedDate", dts.nickNameUpdatedDate }
    };

        dbRef.Child("Users").Child(Managers.Auth.userId)
            .SetValueAsync(defaultData).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError("Users 저장 실패: " + task.Exception);
            });

        // MoneyRank 저장 방식 수정
        string userId = Managers.Auth.userId;
        var tasks = new List<Task>();

        string[] gameTypes = { "holdem", "poker", "blackjack" };
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
        // 삭제할 경로들
        List<Task> deleteTasks = new List<Task>();

        // 1. Users/{userId} 삭제
        deleteTasks.Add(dbRef.Child("Users").Child(userId).RemoveValueAsync());

        // 2. MoneyRank/{gameType}/{userId} 삭제
        string[] gameTypes = { "holdem", "poker", "blackjack" };
        foreach (string gameType in gameTypes)
        {
            deleteTasks.Add(dbRef.Child("MoneyRank").Child(gameType).Child(userId).RemoveValueAsync());
        }

        // 모든 삭제 작업 완료 후 콜백 처리
        Task.WhenAll(deleteTasks).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log($"DBManager: 사용자 {userId} 데이터(Users 및 MoneyRank) 삭제 성공");
                onComplete?.Invoke(true, null);
            }
            else
            {
                string error = task.Exception?.Message ?? "Unknown DB error";
                Debug.LogError($"DBManager: 사용자 {userId} 데이터(Users 및 MoneyRank) 삭제 실패 - {error}");
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

        // 업데이트
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

    public void ChangeNickName(string newNickName)
    {
        DateTime utcNow = DateTime.UtcNow;
        string date = utcNow.ToString("yyyyMMdd");

        var updates = new Dictionary<string, object>
        {
            { "nickName", newNickName },
            { "nickNameUpdatedDate", date }
        };

        dbRef.Child("Users").Child(Managers.Auth.userId).UpdateChildrenAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("닉네임 변경 실패: " + task.Exception);
            }
            else
            {
                Debug.Log("닉네임 및 업데이트 날짜 변경 성공: " + newNickName);
            }
        });
    }

    public void IsNickNameChangeAvailable(Action<bool> callback)
    {
        dbRef.Child("Users").Child(Managers.Auth.userId).Child("nickNameUpdatedDate").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Users 조회 실패: " + task.Exception);
                callback(false);
                return;
            }

            DataSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Debug.Log("snapshot exitsts");
                string date = snapshot.Value.ToString();
                DateTime parsedDate = DateTime.ParseExact(
                    date,
                    "yyyyMMdd",
                    null,
                    System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal
                );

                DateTime plus30Days = parsedDate.AddDays(30);

                if (plus30Days <= DateTime.UtcNow.Date)
                {
                    Debug.Log("if plus");
                    callback(true);
                    return;
                }
            }

            callback(false);
        });
    }

    public void IsOverlapped(string newNickName, Action<NickNameCheckResult> callback)
    {
        string currentUID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        dbRef.Child("Users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Users 조회 실패: " + task.Exception);
                callback(NickNameCheckResult.Error);
                return;
            }

            foreach (var userSnapshot in task.Result.Children)
            {
                string uid = userSnapshot.Key;
                string nick = userSnapshot.Child("nickName").Value?.ToString();

                // 자기 자신의 UID는 검사에서 제외
                if (uid != currentUID && nick == newNickName)
                {
                    callback(NickNameCheckResult.Duplicated);
                    return;
                }
            }

            callback(NickNameCheckResult.Available);
        });
    }

    /*
        친구들 정보 불러오는 함수
    */
    public void GetFriendsData(Action<List<string>> onFriendsLoaded)
    {
        string myUID = User.NowUser.GetUid();

        dbRef.Child("Users").Child(myUID).Child("friends").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("친구 목록 불러오기 실패: " + task.Exception);
                onFriendsLoaded?.Invoke(null);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                List<string> friendUIDs = new List<string>();

                foreach (var child in snapshot.Children)
                {
                    string friendUid = child.Key.ToString();
                    friendUIDs.Add(friendUid);
                }

                // 콜백으로 전달
                onFriendsLoaded?.Invoke(friendUIDs);
            }
        });
    }

    /*
        친구 요청 목록 불러오는 함수
    */
    public void GetRequests(Action<List<string>> onRequestsLoaded)
    {
        string currentUID = User.NowUser.GetUid();

        dbRef.Child("Users").Child(currentUID).Child("requests").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("친구 요청 목록 불러오기 실패: " + task.Exception);
                onRequestsLoaded?.Invoke(null);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                List<string> requestUIDs = new List<string>();

                foreach (var child in snapshot.Children)
                {
                    string requestUid = child.Key.ToString();
                    requestUIDs.Add(requestUid);
                }

                // 콜백으로 전달
                onRequestsLoaded?.Invoke(requestUIDs);
            }
        });
    }

    /*
        uid를 통해, 닉네임을 가져오는 함수
    */
    public void GetNicknameByUID(string uid, Action<string> onNicknameLoaded)
    {
        dbRef.Child("Users").Child(uid).Child("nickName").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("닉네임 가져오기 실패: " + task.Exception);
                onNicknameLoaded?.Invoke(null);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                onNicknameLoaded?.Invoke(snapshot.Value.ToString());
            }
        });
    }

    /*
        uid를 통해, 접속 상태를 가져오는 함수
    */
    public void GetStatusByUID(string uid, Action<Define.Status> onStatusLoaded)
    {
        dbRef.Child("Users").Child(uid).Child("status").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("닉네임 가져오기 실패: " + task.Exception);
                onStatusLoaded?.Invoke(Enum.Parse<Define.Status>(null));
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                onStatusLoaded?.Invoke(Enum.Parse<Define.Status>(snapshot.Value.ToString()));
            }
        });
    }

    /*
        닉네임을 통해, 해당 닉네임을 가진 uid 목록을 가져오는 함수
    */
    public void GetUIDsByNickname(string nickname, Action<List<string>> onUIDsFound)
    {
        var query = dbRef.Child("Users")
                         .OrderByChild("nickName")
                         .EqualTo(nickname);

        query.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("닉네임 검색 실패: " + task.Exception);
                onUIDsFound?.Invoke(null);
                return;
            }

            var uids = new List<string>();
            var snap = task.Result;

            if (snap.Exists && snap.HasChildren)
            {
                foreach (var child in snap.Children)
                {
                    // EqualTo 일치한 유저들의 UID
                    uids.Add(child.Key);
                }
            }

            onUIDsFound?.Invoke(uids);
        });
    }


    /*
        특정 플레이어에게 친구 요청을 보내놓는 함수
    */
    public void RequestAddFriend(string targetUID)
    {
        string myUID = User.NowUser.GetUid();

        dbRef.Child("Users").Child(targetUID).Child("requests").Child(myUID).SetValueAsync(true)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"친구 요청 보냄: {myUID} -> {targetUID}");
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError("친구 요청 실패: " + task.Exception);
                }
            });
    }

    /*
        친구 수락 함수 (비로소 친구 목록에 서로를 추가) 
    */
    public void AcceptAndAddFriend(string targetUID)
    {
        string myUID = User.NowUser.GetUid();

        // 1. 내 친구 목록에 상대방 추가
        dbRef.Child("Users").Child(myUID).Child("friends").Child(targetUID).SetValueAsync(true)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"친구 추가 됨: {myUID} -> {targetUID}");
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError("친구 추가 실패: " + task.Exception);
                }
            });

        // 2. 상대방 목록에 나 추가
        dbRef.Child("Users").Child(targetUID).Child("friends").Child(myUID).SetValueAsync(true)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"친구 추가 됨: {targetUID} -> {myUID}");
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError("친구 추가 실패: " + task.Exception);
                }
            });

        // 3. 내 요청 목록에서 해당 유저 제거
        dbRef.Child("Users").Child(myUID).Child("requests").Child(targetUID).RemoveValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"요청 제거 완료: {targetUID}");
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError("요청 제거 실패: " + task.Exception);
                }
            });
    }

    /*
        친구 요청 거절 함수 
    */
    public void RejectRequest(string targetUID)
    {
        string myUID = User.NowUser.GetUid();

        // 그냥 내 요청 목록에서 상대방 제거
        dbRef.Child("Users").Child(myUID).Child("requests").Child(targetUID).RemoveValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"요청 제거 완료: {targetUID}");
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError("요청 제거 실패: " + task.Exception);
                }
            });
    }

    /*
        친구 제거 함수
    */
    public void RemoveFriend(string targetUID)
    {
        string myUID = User.NowUser.GetUid();

        // 1. 내 친구 목록에서 상대방 제거
        dbRef.Child("Users").Child(myUID).Child("friends").Child(targetUID).RemoveValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"친구 제거 완료: {targetUID}");
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError("친구 제거 실패: " + task.Exception);
                }
            });

        // 2. 상대방 친구 목록에서 나 제거
        dbRef.Child("Users").Child(targetUID).Child("friends").Child(myUID).RemoveValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("상대방 친구 목록에서 나 제거 완료");
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError("상대방 친구 목록에서 나 제거 실패: " + task.Exception);
                }
            });
    }

    /*
        둘이 친구인지 (친구면 true, 아니면 false)
    */
    public async Task<bool> IsFriendAsync(string uid1, string uid2)
    {
        var snapshot = await dbRef.Child("Users").Child(uid1).Child("friends").Child(uid2).GetValueAsync();
        return snapshot.Exists;
    }

    /*
        유저의 상태를 설정
    */
    public void SetUserStatus(Define.Status value)
    {
        string myUID = User.NowUser.GetUid();

        DatabaseReference statusRef = dbRef.Child("Users").Child(myUID).Child("status");

        // 현재 상태 기록
        statusRef.SetValueAsync(value.ToString()).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("상태 변경 완료: " + value);
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("상태 변경 실패: " + value);
            }
        });

        // 연결 끊기면 offline으로 자동 변경
        if (value != Define.Status.Offline)
        {
            statusRef.OnDisconnect().SetValue(Define.Status.Offline.ToString());
        }
    }

    /*
        상대방에게 게임 초대 보내놓는 함수
    */
    public void SendInvitation(string targetUID)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long ttl = 30 * 1000; // 30초
        long expiresAt = now + ttl;

        string myNickName = User.NowUser.GetNickName();

        // 초대 신청 데이터 포맷
        var inviteData = new Dictionary<string, object>
        {
            {"from", myNickName},
            {"roomID", Managers.Photon.currentRoomName},
            {"gameType", Managers.Photon.GetGameType().ToString()},
            {"createdAt", now},
            {"expiresAt", expiresAt}
        };

        // 상대가 온라인인지부터 확인 (온라인일 때만 초대 보내기)
        GetStatusByUID(targetUID, (status) =>
        {
            if (status == Define.Status.Online)
            {
                DatabaseReference targetInvRef = dbRef.Child("Users").Child(targetUID).Child("invitation");
                var snapshotTask = targetInvRef.GetValueAsync();

                snapshotTask.ContinueWithOnMainThread(snapshotTaskResult =>
                {
                    if (snapshotTaskResult.IsCompleted)
                    {
                        DataSnapshot snapshot = snapshotTaskResult.Result;

                        bool canSend = true;

                        if (snapshot.Exists)
                        {
                            // 기존 초대 데이터가 있다면 만료 여부 확인
                            var data = snapshot.Value as Dictionary<string, object>;
                            if (data != null && data.ContainsKey("expiresAt"))
                            {
                                long existingExpiresAt = Convert.ToInt64(data["expiresAt"]);
                                if (now < existingExpiresAt)
                                {
                                    // 아직 만료 안 된 초대가 있음
                                    canSend = false;
                                }
                            }
                        }

                        if (canSend)
                        {
                            targetInvRef.SetValueAsync(inviteData).ContinueWithOnMainThread(task =>
                            {
                                if (task.IsCompleted)
                                {
                                    Debug.Log($"{targetUID}에게 초대 보내기 성공");
                                }
                                else if (task.IsFaulted)
                                {
                                    Debug.LogError($"{targetUID}에게 초대 보내기 실패");
                                }
                            });
                        }
                        else
                        {
                            Debug.Log("이미 유효한 초대가 존재합니다.");
                        }
                    }
                });
            }
            else
            {
                Debug.Log("상대가 온라인 상태가 아닙니다.");
            }
        });
    }

    private void HandleInviteAdded(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("DB Error: " + args.DatabaseError.Message);
            return;
        }

        var data = args.Snapshot.Value as Dictionary<string, object>;
        if (data == null) return;

        long expiresAt = Convert.ToInt64(data["expiresAt"]);
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        string myUID = User.NowUser.GetUid();

        if (now > expiresAt)
        {
            Debug.Log("만료된 초대, 무시");
            // DB에서 삭제 처리
            args.Snapshot.Reference.RemoveValueAsync();
            return;
        }

        string roomID = Convert.ToString(data["roomID"]);
        string senderName = Convert.ToString(data["from"]);
        string gameType = Convert.ToString(data["gameType"]);

        // 내가 온라인일 때만
        GetStatusByUID(myUID, (status) =>
        {
            if (status == Define.Status.Online)
            {
                Debug.Log("새로운 초대 도착!");
                // 게임 초대 팝업 띄우기
                GameObject inviteGO = Managers.UI.ShowPopupUI<UI_GameInvitePopup>().gameObject;
                UI_GameInvitePopup invitePopup = inviteGO.GetOrAddComponent<UI_GameInvitePopup>();
                invitePopup.Init();
                invitePopup.SetRoomID(roomID);
                invitePopup.SetSenderNickName(senderName);
                invitePopup.SetGameType(gameType);
            }
        });
    }

    /* 
        받은 초대 삭제하는 함수
    */
    public void RemoveInvitation()
    {
        string myUID = User.NowUser.GetUid();

        // invitation 밑의 모든 자식 삭제
        dbRef.Child("Users").Child(myUID).Child("invitation").SetValueAsync(null).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("invitation 전체 삭제 성공");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("invitation 전체 삭제 실패");
            }
        });
    }
}