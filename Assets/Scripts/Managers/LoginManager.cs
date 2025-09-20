using UnityEngine;
using UnityEngine.EventSystems;
using Google;
using Firebase.Auth;
using System;
using System.Collections;
using UnityEngine.Networking;



public class LoginManager
{
    public string GoogleAPI = "1022865872304-vpjlvm2modeojucrj1aa7ud7kq301jak.apps.googleusercontent.com";
    public string KakaoNativeAppKey = "a7cb0e8d7a09e235219b60e5eefb3ad6";
    public string FirebaseCustomTokenEndpoint = "https://kakaocustomtoken-qynjwajc4q-du.a.run.app";

    private UI_Login _loginUI;
    private bool isGoogleInitialized = false;
    public bool KakaoForceAccountLogin = false;

    public void LoginSceneLoaded(UI_Login login)
    {
        _loginUI = login;
        if (!isGoogleInitialized)
            InitGoogleSignIn();

        // (선택) 시작 시 1회 초기화
#if UNITY_ANDROID && !UNITY_EDITOR
        try { KakaoBridge.Init(KakaoNativeAppKey); } catch { }
#endif
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
                _loginUI.SetConnectionInfoText("로그인 취소됨");
                _loginUI.ButtonInteractive(true);
                _loginUI.ShowLoginButtons();
                return;
            }
            if (task.IsFaulted)
            {
                foreach (var e in task.Exception.InnerExceptions)
                    _loginUI.SetConnectionInfoText("Google Sign-In error: " + e.Message);
                _loginUI.ButtonInteractive(true);
                _loginUI.ShowLoginButtons();
                return;
            }

            Debug.Log("Google Sign-In success: " + task.Result.DisplayName);

            // Firebase Auth로 전달
            Managers.Auth.SignInWithGoogle(task.Result.IdToken, _loginUI);
        });
    }

    public void LogInWithKakao()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    if (_loginUI == null) _loginUI = (UI_Login)Managers.UI.SceneUI;
    _loginUI.SetConnectionInfoText("카카오로 로그인 중...");
    KakaoBridge.Init(KakaoNativeAppKey);

    KakaoBridge.Login(
        onSuccess: (accessToken) => { CoroutineRunner.Start(CoExchangeAndSignIn(accessToken)); },
        onFail: (err) =>
        {
            Debug.LogError("[KAKAO] fail: " + err);
            _loginUI.SetConnectionInfoText("카카오 로그인 실패: " + err);
            // ✅ 버튼 되살리기
            _loginUI.ButtonInteractive(true);
            _loginUI.ShowReconnectButton(); // 필요 시
            _loginUI.ShowLoginButtons();    // ← 아래 추가 메서드 참고
        },
        forceAccountLogin: KakaoForceAccountLogin);
#else
        _loginUI?.SetConnectionInfoText("카카오 로그인은 Android에서만 지원됩니다.");
#endif
    }

    // Kakao accessToken -> (백엔드) Firebase Custom Token 교환 -> Firebase 로그인
    private IEnumerator CoExchangeAndSignIn(string kakaoAccessToken)
    {
        Debug.Log($"[KAKAO] start exchange → {FirebaseCustomTokenEndpoint}");

        var payload = JsonUtility.ToJson(new KakaoTokenReq { accessToken = kakaoAccessToken });

        var req = new UnityWebRequest(FirebaseCustomTokenEndpoint, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(payload));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.timeout = 12;

        yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
        bool isErr = req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError;
#else
    bool isErr = req.isNetworkError || req.isHttpError;
#endif
        Debug.Log($"[KAKAO] exchange done. code={req.responseCode}, result={req.result}, err={req.error}");
        Debug.Log($"[KAKAO] body: {req.downloadHandler.text}");

        if (isErr)
        {
            _loginUI?.SetConnectionInfoText($"커스텀 토큰 실패({req.responseCode})");
            _loginUI?.ButtonInteractive(true);
            _loginUI?.ShowLoginButtons(); // 아래 3) 참고
            yield break;
        }

        KakaoCustomTokenRes res = null;
        try
        {
            res = JsonUtility.FromJson<KakaoCustomTokenRes>(req.downloadHandler.text);
        }
        catch (Exception e)
        {
            Debug.LogError("[KAKAO] JSON parse error: " + e.Message);
            _loginUI?.SetConnectionInfoText("커스텀 토큰 파싱 실패");
            _loginUI?.ButtonInteractive(true);
            _loginUI?.ShowLoginButtons();
            yield break;
        }

        if (res == null || string.IsNullOrEmpty(res.customToken))
        {
            Debug.LogError("[KAKAO] empty/missing customToken");
            _loginUI?.SetConnectionInfoText("커스텀 토큰 없음");
            _loginUI?.ButtonInteractive(true);
            _loginUI?.ShowLoginButtons();
            yield break;
        }

        Debug.Log("[KAKAO] got customToken. signing in to Firebase...");
        Managers.Auth.SignInWithFirebaseCustomToken(res.customToken, _loginUI);
    }


    [Serializable] private class KakaoTokenReq { public string accessToken; }
    [Serializable] private class KakaoCustomTokenRes { public string customToken; public string error; }

    // 로그아웃 버튼에서 호출
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
            Managers.DB.SetUserStatus(Define.Status.Offline);   // 유저정보씬 -> 로그인씬 (status: offline)
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
                    // Managers.DB.SetUserStatus(Define.Status.Offline); 할 필요 없고 하면 안되기도 함 (이미 DB에서 해당 유저는 없어짐)
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
internal static class CoroutineRunner
{
    private class Host : MonoBehaviour { }
    private static Host _host;
    public static void Start(IEnumerator routine)
    {
        if (_host == null)
        {
            var go = new GameObject("[CoroutineRunner]");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _host = go.AddComponent<Host>();
        }
        _host.StartCoroutine(routine);
    }
}