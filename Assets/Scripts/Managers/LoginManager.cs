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
    public void LogOut(PointerEventData _)
    {
        // UI 안내
        _loginUI?.SetConnectionInfoText("로그아웃 중...");
        _loginUI?.ButtonInteractive(false);

        // 실제 로그아웃 순서를 코루틴으로 처리
        CoroutineRunner.Start(CoLogOut());
    }

    private IEnumerator CoLogOut()
    {
        // 1) Kakao 로그아웃 (Android만)
#if UNITY_ANDROID && !UNITY_EDITOR
        bool kakaoDone = false;
        try
        {
            KakaoBridge.Logout(() => { kakaoDone = true; });
        }
        catch (Exception e)
        {
            Debug.LogWarning("[KAKAO] logout call failed: " + e.Message);
            kakaoDone = true; // 실패해도 진행
        }
        // 최대 3초까지만 대기
        float t = 0f;
        while (!kakaoDone && t < 3f) { t += Time.unscaledDeltaTime; yield return null; }
#endif

        // 2) Firebase 로그아웃
        try
        {
            Managers.Auth.SignOutFirebase();
        }
        catch (Exception e)
        {
            Debug.LogWarning("[FIREBASE] SignOut fail: " + e.Message);
        }

        // (선택) 자동로그인 플래그가 있으면 끄기
        // PlayerPrefs.SetInt("WantsAutoLogin", 0); PlayerPrefs.Save();

        // 3) Google 로그아웃(있으면)
        try
        {
            GoogleSignIn.DefaultInstance?.SignOut();
            GoogleSignIn.DefaultInstance?.Disconnect(); // 계정 연결 해제(토큰 폐기)
        }
        catch (Exception e)
        {
            Debug.LogWarning("[GOOGLE] SignOut/Disconnect fail: " + e.Message);
        }

        // 4) 네트워크/세션 정리
        try { Managers.Photon.DisconnectPhoton(); } catch { }

        // 5) 로그인 씬으로 복귀
        try { Managers.Scene.LoadScene(Define.Scene.Login); } catch (Exception e) { Debug.LogError("Scene load fail: " + e.Message); }

        yield break;
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