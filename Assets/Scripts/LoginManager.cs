using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using UnityEngine.EventSystems;

public class LoginManager : MonoBehaviour
{
    private static LoginManager instance;
    public static LoginManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LoginManager>();
                if (instance == null)
                {
                    GameObject singletonObj = new GameObject(nameof(LoginManager));
                    instance = singletonObj.AddComponent<LoginManager>();
                    DontDestroyOnLoad(singletonObj);
                }
            }
            return instance;
        }
    }

    public string GoogleAPI = "1022865872304-vpjlvm2modeojucrj1aa7ud7kq301jak.apps.googleusercontent.com";
    private FirebaseAuth auth;
    private FirebaseUser user;
    public string userId;

    private bool isGoogleSignInInitialized = false;
    private bool isFirebaseInitialized = false;
    private UI_Login _loginUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void LoginSceneLoaded(UI_Login login)
    {
        _loginUI = login;
        if (!isGoogleSignInInitialized) InitGoogleSignIn();
        if (!isFirebaseInitialized) InitFirebase();
    }

    private void InitGoogleSignIn()
    {
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = GoogleAPI,
            RequestEmail = true
        };
        isGoogleSignInInitialized = true;
        Debug.Log("Google Sign-In initialized.");
    }

    private void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        isFirebaseInitialized = true;
        Debug.Log("Firebase initialized.");
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            user = auth.CurrentUser;
            if (user != null)
            {
                userId = user.UserId;
                _loginUI.SetConnectionInfoText("자동 로그인 성공!");
                DBManager.Instance.GetUserInfo();
                Managers.Photon.ConnectToPhoton(_loginUI);
            }
        }
    }

    public void LogIn()
    {
        _loginUI.SetConnectionInfoText("로그인 중...");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                _loginUI.SetConnectionInfoText("로그인 실패...");
                return;
            }
            if (task.IsFaulted)
            {
                foreach (var e in task.Exception.InnerExceptions)
                {
                    _loginUI.SetConnectionInfoText("Google Sign-In error: " + e.Message);
                }
                return;
            }

            Debug.Log("Google Sign-In success: " + task.Result.DisplayName);

            Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
            {
                if (authTask.IsCanceled || authTask.IsFaulted)
                {
                    Debug.LogError($"Firebase Auth failed: {authTask.Exception}");
                    return;
                }

                Debug.Log("Firebase auth success");

                if (User.NowUser == null)
                    Debug.LogError("User.Instance is null after login!!");
            });
        });
    }

    public void LogOut(PointerEventData data)
    {
        Debug.Log("Logout process started.");

        try
        {
            if (auth != null)
                auth.StateChanged -= AuthStateChanged;

            auth?.SignOut();
            GoogleSignIn.DefaultInstance.SignOut();
            GoogleSignIn.DefaultInstance.Disconnect();
            user = null;

            isGoogleSignInInitialized = false;
            isFirebaseInitialized = false;

            Debug.Log("Logout success.");

            Managers.Photon.DisconnectPhoton();
            Managers.Scene.LoadScene(Define.Scene.Login);
        }
        catch (Exception e)
        {
            Debug.LogError("Unexpected error during logout: " + e.Message);
        }
    }
}
