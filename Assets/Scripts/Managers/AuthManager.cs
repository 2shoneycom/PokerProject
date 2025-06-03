using UnityEngine;
using Firebase.Auth;
using Firebase.Extensions;
using System;

public class AuthManager 
{    
    private FirebaseAuth auth;
    private FirebaseUser user;
    public string userId;

    private bool isFirebaseInitialized = false;

    UI_Login loginUI;

    private void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        isFirebaseInitialized = true;
        Debug.Log("Firebase initialized.");
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            user = auth.CurrentUser;
            if (user != null)
            {
                userId = user.UserId;
                Debug.Log("Firebase Auth Changed: User logged in.");

                Managers.DB.GetUserInfo();
                if (loginUI == null)
                    loginUI = (UI_Login)Managers.UI.SceneUI;

                loginUI.SetConnectionInfoText("자동 로그인 성공!");

                Managers.Photon.ConnectToPhoton(loginUI);
            }
        }
    }

    public void SignInWithGoogle(string idToken, UI_Login uiLogin)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
        {
            Debug.Log("login start");
            if (authTask.IsCanceled || authTask.IsFaulted)
            {
                Debug.LogError($"Firebase Auth failed: {authTask.Exception}");
                uiLogin.SetConnectionInfoText("Firebase 인증 실패");
                return;
            }

            Debug.Log("Firebase auth success");

            if (User.NowUser == null)
                Debug.LogError("User.Instance is null after login!!");
        });
    }

    public void SignOutFirebase()
    {
        auth?.SignOut();
        user = null;
        userId = null;
    }

    public void DeleteAccount(string idToken, Action<bool, string> onComplete)
    {
        if (auth.CurrentUser == null)
        {
            onComplete?.Invoke(false, "No user is signed in.");
            return;
        }

        var credential = GoogleAuthProvider.GetCredential(idToken, null);

        auth.CurrentUser.ReauthenticateAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                string err = task.Exception?.Message ?? "Reauthentication failed.";
                onComplete?.Invoke(false, err);
                return;
            }

            string targetUserId = userId;

            auth.CurrentUser.DeleteAsync().ContinueWithOnMainThread(deleteTask =>
            {
                if (deleteTask.IsCompleted && !deleteTask.IsFaulted && !deleteTask.IsCanceled)
                {
                    Managers.DB.DeleteUserData(targetUserId, (dbSuccess, dbError) =>
                    {
                        if (!dbSuccess)
                            Debug.LogWarning("사용자 DB 데이터 삭제 실패: " + dbError);

                        onComplete?.Invoke(true, null);  // 계정 삭제는 성공했음
                    });
                }
                else
                {
                    string errorMsg = deleteTask.Exception?.ToString() ?? "Unknown error";
                    onComplete?.Invoke(false, errorMsg);
                }
            });
        });
    }
}
