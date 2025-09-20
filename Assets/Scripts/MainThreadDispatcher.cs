using System;
using System.Collections.Concurrent;
using UnityEngine;

public sealed class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher _instance;
    private static readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("[MainThreadDispatcher]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<MainThreadDispatcher>();
    }

    public static void Enqueue(Action action)
    {
        if (action != null) _queue.Enqueue(action);
    }

    private void Update()
    {
        while (_queue.TryDequeue(out var a))
        {
            try { a(); } catch (Exception e) { Debug.LogException(e); }
        }
    }
}
