using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Google;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using System;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    private static LoginManager instance;
    public static LoginManager Instance { get { return instance; } }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public string GoogleAPI = "1022865872304-vpjlvm2modeojucrj1aa7ud7kq301jak.apps.googleusercontent.com"; // Web Client ID를 설정하세요.
    private GoogleSignInConfiguration configuration;

    private FirebaseAuth auth;
    private FirebaseUser user;
    public string userId;

    private bool isGoogleSignInInitialized = false;


    private void Start()
    {
        InitFirebase();
        Debug.Log("Google Sign-In start");

        // Google Sign-In 초기화 (한 번만 실행)
        if (!isGoogleSignInInitialized)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = GoogleAPI,
                RequestEmail = true
            };

            isGoogleSignInInitialized = true;
            Debug.Log("Google Sign-In initialize complete");
        }
    }

    void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
        Debug.Log("Firebase initialize complete");
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            user = auth.CurrentUser;
            userId = user.UserId;
            Debug.Log("Firebase login success: " + user.DisplayName);
            User.Instance.SetUserInfo();
            SceneManager.LoadScene("Lobby Scene");
        }
    }

    public void LogIn()
    {
        try
        {
            // 로그인 요청
            Task<GoogleSignInUser> signInTask = GoogleSignIn.DefaultInstance.SignIn();

            signInTask.ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("Google Sign-In canceled");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("Google Sign-In fault: " + task.Exception);
                    return;
                }

                Debug.Log("Google Sign-In success: " + task.Result.DisplayName);

                // Firebase 인증 처리
                Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
                auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
                {
                    if (authTask.IsCanceled)
                    {
                        Debug.LogError("Firebase auth canceled");
                        return;
                    }
                    if (authTask.IsFaulted)
                    {
                        Debug.LogError("Firebase auth fail: " + authTask.Exception);
                        return;
                    }
                    Debug.Log("firebase auth success");
                    if (User.Instance == null)
                    {
                        Debug.LogError("User.Instance is null!!");
                    }
                    // 로그인 성공 시 사용자 정보 업데이트
                    user = auth.CurrentUser;
                    userId = user.UserId;
                    User.Instance.SetUserInfo();
                    Debug.Log("Firebase login success: " + user.DisplayName);
                    SceneManager.LoadScene("Lobby Scene");
                });
            });
        }
        catch (DllNotFoundException e)
        {
            Debug.LogError("cannot find native library: " + e.Message);
        }
        catch (System.Exception e)
        {
            Debug.LogError("unexpected error occured: " + e.Message);
        }
    }

    public void LogOut()
    {
        try
        {
            auth.SignOut();
            GoogleSignIn.DefaultInstance.SignOut();
            Debug.Log("logout success");
        }
        catch (System.Exception e)
        {
            Debug.LogError("error occured when logout: " + e.Message);
        }
    }
}
