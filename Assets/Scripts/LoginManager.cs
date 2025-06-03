using UnityEngine;
using UnityEngine.EventSystems;
using Google;
using Firebase.Auth;

public class LoginManager
{
    public string GoogleAPI = "1022865872304-vpjlvm2modeojucrj1aa7ud7kq301jak.apps.googleusercontent.com";

    private UI_Login _loginUI;
    private bool isGoogleInitialized = false;

    public void LoginSceneLoaded(UI_Login login)
    {
        _loginUI = login;
        if (!isGoogleInitialized)
            InitGoogleSignIn();
    }

    private void InitGoogleSignIn()
    {
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = GoogleAPI,
            RequestEmail = true,
            ForceTokenRefresh = true
        };
        isGoogleInitialized = true;
        Debug.Log("Google Sign-In initialized.");
    }

    public void LogIn()
    {
        if (_loginUI == null)
            _loginUI = (UI_Login)Managers.UI.SceneUI;

        _loginUI.SetConnectionInfoText("로그인 중...");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                _loginUI.SetConnectionInfoText("로그인 취소됨.");
                return;
            }
            if (task.IsFaulted)
            {
                foreach (var e in task.Exception.InnerExceptions)
                    _loginUI.SetConnectionInfoText("Google Sign-In error: " + e.Message);
                return;
            }

            Debug.Log("Google Sign-In success: " + task.Result.DisplayName);

            // Firebase Auth로 전달
            Managers.Auth.SignInWithGoogle(task.Result.IdToken, _loginUI);
        });
    }

    public void LogOut(PointerEventData data)
    {
        Debug.Log("Logout process started.");

        try
        {
            GoogleSignIn.DefaultInstance.SignOut();
            GoogleSignIn.DefaultInstance.Disconnect();

            isGoogleInitialized = false;

            Managers.Auth.SignOutFirebase();

            Debug.Log("Logout success.");
            Managers.Photon.DisconnectPhoton();
            Managers.Scene.LoadScene(Define.Scene.Login);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Unexpected error during logout: " + e.Message);
        }
    }

    public void DeleteAccount()
    {
        // 1. Google 재로그인
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Google 재로그인 실패: 계정 삭제 중단");
                _loginUI?.SetConnectionInfoText("Google 재인증 실패. 다시 시도해주세요.");
                return;
            }

            string idToken = task.Result.IdToken;

            // 2. Firebase 계정 삭제 요청
            Managers.Auth.DeleteAccount(idToken, (success, error) =>
            {
                if (success)
                {
                    Debug.Log("계정 삭제 완료");
                    GoogleSignIn.DefaultInstance.SignOut();
                    GoogleSignIn.DefaultInstance.Disconnect();
                    _loginUI?.SetConnectionInfoText("회원 탈퇴 완료");

                    Managers.Photon.DisconnectPhoton();
                    Managers.Scene.LoadScene(Define.Scene.Login);
                }
                else
                {
                    Debug.LogError("계정 삭제 실패: " + error);
                    _loginUI?.SetConnectionInfoText("회원 탈퇴 실패: " + error);
                }
            });
        });
    }

}
