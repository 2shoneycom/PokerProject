#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;

internal class KotlinFunction2 : AndroidJavaProxy
{
    private readonly Action<AndroidJavaObject, AndroidJavaObject> _onInvoke;
    public KotlinFunction2(Action<AndroidJavaObject, AndroidJavaObject> onInvoke)
        : base("kotlin.jvm.functions.Function2") { _onInvoke = onInvoke; }
    public AndroidJavaObject invoke(AndroidJavaObject p1, AndroidJavaObject p2)
    {
        _onInvoke?.Invoke(p1, p2);
        return null; // Unit
    }
}

public static class KakaoBridge
{
    private static bool _inited;

    public static void Init(string nativeAppKey)
    {
        if (_inited) return;
        try
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var kakaoSdk    = new AndroidJavaClass("com.kakao.sdk.common.KakaoSdk");

            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                try { kakaoSdk.CallStatic("init", activity, nativeAppKey); _inited = true; Debug.Log("[KAKAO] KakaoSdk.init OK"); }
                catch (Exception e) { Debug.LogError("[KAKAO] KakaoSdk.init failed: " + e.Message); }
            }));
        }
        catch (Exception e) { Debug.LogError("[KAKAO] init wrapper failed: " + e.Message); }
    }

    public static void Login(Action<string> onSuccess, Action<string> onFail, bool forceAccountLogin = false)
    {
        var unityPlayer  = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity     = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var userApiClass = new AndroidJavaClass("com.kakao.sdk.user.UserApiClient");
        var userApi      = userApiClass.CallStatic<AndroidJavaObject>("getInstance");

        bool talkAvailable = false;
        try { talkAvailable = !forceAccountLogin && userApi.Call<bool>("isKakaoTalkLoginAvailable", activity); }
        catch { talkAvailable = false; }

        var callback = new KotlinFunction2((tokenObj, errorObj) =>
        {
            // 안드로이드 콜백 → Unity 메인 스레드로 디스패치
            MainThreadDispatcher.Enqueue(() =>
            {
                if (errorObj != null)
                {
                    string msg = SafeCall(errorObj, "getMessage") ?? "unknown error";
                    Debug.LogError("[KAKAO] login error: " + msg);
                    onFail?.Invoke(msg);
                    return;
                }
                string accessToken = SafeCall(tokenObj, "getAccessToken"); // 필드가 아니라 getter 호출
                if (string.IsNullOrEmpty(accessToken)) { onFail?.Invoke("empty access token"); return; }
                Debug.Log("[KAKAO] accessToken: " + accessToken);
                onSuccess?.Invoke(accessToken);
            });
        });

        activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            try
            {
                if (talkAvailable) { Debug.Log("[KAKAO] loginWithKakaoTalk()"); userApi.Call("loginWithKakaoTalk", activity, callback); }
                else               { Debug.Log("[KAKAO] loginWithKakaoAccount()"); userApi.Call("loginWithKakaoAccount", activity, callback); }
            }
            catch (Exception e)
            {
                Debug.LogError("[KAKAO] login call failed: " + e.Message);
                MainThreadDispatcher.Enqueue(() => onFail?.Invoke(e.Message));
            }
        }));
    }

    public static void Logout(Action onDone = null)
    {
        try
        {
            var unityPlayer  = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity     = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var userApiClass = new AndroidJavaClass("com.kakao.sdk.user.UserApiClient");
            var userApi      = userApiClass.CallStatic<AndroidJavaObject>("getInstance");

            var callback = new KotlinFunction2((_, __) => MainThreadDispatcher.Enqueue(() => onDone?.Invoke()));

            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                try { userApi.Call("logout", callback); }
                catch (Exception e) { Debug.LogWarning("[KAKAO] logout call failed: " + e.Message); MainThreadDispatcher.Enqueue(() => onDone?.Invoke()); }
            }));
        }
        catch (Exception e) { Debug.LogWarning("[KAKAO] Logout exception: " + e.Message); MainThreadDispatcher.Enqueue(() => onDone?.Invoke()); }
    }

    private static string SafeCall(AndroidJavaObject obj, string method)
    {
        if (obj == null) return null;
        try { return obj.Call<string>(method); }
        catch (Exception e) { Debug.LogWarning($"[KAKAO] SafeCall {method} failed: {e.Message}"); return null; }
    }
}
#endif
