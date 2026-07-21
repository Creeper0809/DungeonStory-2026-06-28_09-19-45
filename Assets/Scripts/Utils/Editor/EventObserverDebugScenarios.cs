using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class EventObserverDebugScenarios
{
    [MenuItem("DungeonStory/Debug/Infrastructure/Run Event Observer Scenarios")]
    public static void RunFromMenu()
    {
        if (!RunAll(true))
        {
            Debug.LogError("Event observer scenarios failed.");
        }
    }

    public static bool RunAll(bool logSuccess)
    {
        List<string> errors = new List<string>();
        RunScenario("발행 중 구독 변경", VerifyMutationSafeSnapshot, errors);
        RunScenario("중복 구독 방지", VerifyDuplicateSubscription, errors);
        RunScenario("파괴된 Unity listener 정리", VerifyDestroyedUnityListenerPruning, errors);
        RunScenario("listener 예외 격리", VerifyListenerExceptionIsolation, errors);
        RunScenario("SubsystemRegistration 초기화", VerifySubsystemReset, errors);
        RunScenario("이벤트 컬렉션 스냅샷", VerifyCollectionPayloadSnapshot, errors);
        RunScenario("live read-only view 변경 우회 차단", VerifyReadOnlyViews, errors);

        if (errors.Count > 0)
        {
            foreach (string error in errors)
            {
                Debug.LogError(error);
            }

            return false;
        }

        if (logSuccess)
        {
            Debug.Log("Event observer scenarios passed.");
        }

        return true;
    }

    private static void RunScenario(string name, Func<bool> scenario, List<string> errors)
    {
        ResetEventObserver();
        try
        {
            if (scenario())
            {
                return;
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            ResetEventObserver();
        }

        errors.Add(name);
    }

    private static bool VerifyMutationSafeSnapshot()
    {
        CountingListener addedDuringPublish = new CountingListener();
        CountingListener removedDuringPublish = new CountingListener();
        CountingListener stable = new CountingListener();
        MutatingListener mutating = new MutatingListener(removedDuringPublish, addedDuringPublish);

        EventObserver.AddListener<ProbeEvent>(stable);
        EventObserver.AddListener<ProbeEvent>(removedDuringPublish);
        EventObserver.AddListener<ProbeEvent>(mutating);

        EventObserver.TriggerEvent(new ProbeEvent());
        bool firstPublish = mutating.Count == 1
            && removedDuringPublish.Count == 1
            && stable.Count == 1
            && addedDuringPublish.Count == 0;

        EventObserver.TriggerEvent(new ProbeEvent());
        bool secondPublish = mutating.Count == 2
            && removedDuringPublish.Count == 1
            && stable.Count == 2
            && addedDuringPublish.Count == 1;

        return firstPublish && secondPublish;
    }

    private static bool VerifyDuplicateSubscription()
    {
        CountingListener listener = new CountingListener();
        EventObserver.AddListener<ProbeEvent>(listener);
        EventObserver.AddListener<ProbeEvent>(listener);
        EventObserver.TriggerEvent(new ProbeEvent());
        return listener.Count == 1;
    }

    private static bool VerifyDestroyedUnityListenerPruning()
    {
        EventObserverUnityCountingListener destroyedListener =
            ScriptableObject.CreateInstance<EventObserverUnityCountingListener>();
        EventObserver.AddListener<EventObserverProbeEvent>(destroyedListener);
        UnityEngine.Object.DestroyImmediate(destroyedListener);

        EventObserverUnityCountingListener liveListener =
            ScriptableObject.CreateInstance<EventObserverUnityCountingListener>();
        EventObserver.AddListener<EventObserverProbeEvent>(liveListener);
        EventObserver.TriggerEvent(new EventObserverProbeEvent());
        bool valid = liveListener.Count == 1;
        UnityEngine.Object.DestroyImmediate(liveListener);
        return valid;
    }

    private static bool VerifySubsystemReset()
    {
        CountingListener listener = new CountingListener();
        EventObserver.AddListener<ProbeEvent>(listener);
        ResetEventObserver();
        EventObserver.TriggerEvent(new ProbeEvent());
        return listener.Count == 0;
    }

    private static bool VerifyListenerExceptionIsolation()
    {
        CountingListener survivingListener = new CountingListener();
        ThrowingListener throwingListener = new ThrowingListener();
        EventObserver.AddListener<ProbeEvent>(survivingListener);
        EventObserver.AddListener<ProbeEvent>(throwingListener);

        ILogHandler previousHandler = Debug.unityLogger.logHandler;
        CapturingLogHandler capture = new CapturingLogHandler();
        Debug.unityLogger.logHandler = capture;
        try
        {
            EventObserver.TriggerEvent(new ProbeEvent());
        }
        finally
        {
            Debug.unityLogger.logHandler = previousHandler;
        }

        return throwingListener.Count == 1
            && survivingListener.Count == 1
            && capture.ExceptionCount == 1;
    }

    private static bool VerifyCollectionPayloadSnapshot()
    {
        List<int> source = new List<int> { 3, 5 };
        IReadOnlyList<int> snapshot = EventPayloadSnapshot.Copy(source);
        source[0] = 99;
        source.Add(7);

        bool mutationRejected = false;
        if (snapshot is IList<int> mutableView)
        {
            try
            {
                mutableView[0] = 42;
            }
            catch (NotSupportedException)
            {
                mutationRejected = true;
            }
        }

        return snapshot.Count == 2
            && snapshot[0] == 3
            && snapshot[1] == 5
            && mutationRejected;
    }

    private static bool VerifyReadOnlyViews()
    {
        List<int> list = new List<int> { 1 };
        HashSet<string> set = new HashSet<string> { "before" };
        Dictionary<string, int> dictionary = new Dictionary<string, int> { ["before"] = 1 };
        IReadOnlyList<int> listView = ReadOnlyView.List(list);
        IReadOnlyCollection<string> setView = ReadOnlyView.Collection(set);
        IReadOnlyDictionary<string, int> dictionaryView = ReadOnlyView.Dictionary(dictionary);

        list.Add(2);
        set.Add("after");
        dictionary["after"] = 2;

        return listView.Count == 2
            && listView[1] == 2
            && setView.Contains("after")
            && dictionaryView["after"] == 2
            && listView is not IList<int>
            && setView is not ISet<string>
            && dictionaryView is not IDictionary<string, int>;
    }

    private static void ResetEventObserver()
    {
        MethodInfo resetMethod = typeof(EventObserver).GetMethod(
            "ResetStaticState",
            BindingFlags.Static | BindingFlags.NonPublic);
        if (resetMethod == null)
        {
            throw new MissingMethodException(typeof(EventObserver).FullName, "ResetStaticState");
        }

        resetMethod.Invoke(null, null);
    }

    private readonly struct ProbeEvent { }

    private sealed class CountingListener : UtilEventListener<ProbeEvent>
    {
        public int Count { get; private set; }

        public void OnTriggerEvent(ProbeEvent eventType)
        {
            Count++;
        }
    }

    private sealed class MutatingListener : UtilEventListener<ProbeEvent>
    {
        private readonly CountingListener listenerToRemove;
        private readonly CountingListener listenerToAdd;

        public MutatingListener(CountingListener listenerToRemove, CountingListener listenerToAdd)
        {
            this.listenerToRemove = listenerToRemove;
            this.listenerToAdd = listenerToAdd;
        }

        public int Count { get; private set; }

        public void OnTriggerEvent(ProbeEvent eventType)
        {
            Count++;
            EventObserver.RemoveListener<ProbeEvent>(listenerToRemove);
            EventObserver.AddListener<ProbeEvent>(listenerToAdd);
        }
    }

    private sealed class ThrowingListener : UtilEventListener<ProbeEvent>
    {
        public int Count { get; private set; }

        public void OnTriggerEvent(ProbeEvent eventType)
        {
            Count++;
            throw new InvalidOperationException("Expected EventObserver probe failure.");
        }
    }

    private sealed class CapturingLogHandler : ILogHandler
    {
        public int ExceptionCount { get; private set; }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            ExceptionCount++;
        }

        public void LogFormat(
            LogType logType,
            UnityEngine.Object context,
            string format,
            params object[] args)
        {
        }
    }

}

internal readonly struct EventObserverProbeEvent { }

internal sealed class EventObserverUnityCountingListener : ScriptableObject,
    UtilEventListener<EventObserverProbeEvent>
{
    public int Count { get; private set; }

    public void OnTriggerEvent(EventObserverProbeEvent eventType)
    {
        Count++;
    }
}
