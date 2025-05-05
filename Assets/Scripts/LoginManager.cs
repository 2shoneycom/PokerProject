using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;
using Google;

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
    private Button loginButton;

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

    //private void OnEnable()
    //{
    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //}

    //private void OnDisable()
    //{
    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //}

    public void OnSceneLoaded()
    {
        Debug.Log("sceneload start");
        Debug.Log("login scene");
        loginButton = GameObject.Find("UI_GoogleLoginButton")?.GetComponent<Button>();
        if (loginButton != null)
        {
            loginButton.onClick.RemoveAllListeners();
            loginButton.onClick.AddListener(() =>
            {
                Debug.Log("button clicked");
                LogIn();
            });
            Debug.Log("Login button connected!");
        }
        else
        {
            Debug.LogError("Login button not found!");
        }

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
                Debug.Log("Firebase login success: " + user.DisplayName);
                DBManager.Instance.GetUserInfo();
                SceneManager.LoadScene("Lobby");
            }
        }
    }

    public void LogIn()
    {

        Debug.Log("Attempting Google Sign-In...");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Google Sign-In was canceled by the user.");
                return;
            }
            if (task.IsFaulted)
            {
                foreach (var e in task.Exception.InnerExceptions)
                {
                    Debug.LogError("Google Sign-In error: " + e.Message);
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

                if (Managers.User == null)
                    Debug.LogError("User.Instance is null after login!!");
            });
        });
    }

    public void LogOut()
    {
        Debug.Log("Logout process started.");

        try
        {
            if (auth != null)
                auth.StateChanged -= AuthStateChanged;

            auth?.SignOut();
            GoogleSignIn.DefaultInstance.SignOut();
            user = null;

            isFirebaseInitialized = false;
            isGoogleSignInInitialized = false;

            Debug.Log("Logout success.");

            SceneManager.LoadScene("Login");
        }
        catch (Exception e)
        {
            Debug.LogError("Unexpected error during logout: " + e.Message);
        }
    }
}
