using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventObserver
{
    private static readonly Dictionary<Type, List<UtilEventListenerBase>> SubscribersByType =
        new Dictionary<Type, List<UtilEventListenerBase>>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        SubscribersByType.Clear();
    }

    public static void AddListener<Event>(UtilEventListener<Event> listener) where Event : struct
    {
        if (!IsAlive(listener))
        {
            return;
        }

        Type eventType = typeof(Event);
        if (!SubscribersByType.TryGetValue(eventType, out List<UtilEventListenerBase> listeners))
        {
            listeners = new List<UtilEventListenerBase>();
            SubscribersByType[eventType] = listeners;
        }

        PruneDeadListeners(listeners);
        if (!SubscriptionExists(listeners, listener))
        {
            listeners.Add(listener);
        }
    }

    public static void RemoveListener<Event>(UtilEventListener<Event> listener) where Event : struct
    {
        Type eventType = typeof(Event);
        if (!SubscribersByType.TryGetValue(eventType, out List<UtilEventListenerBase> listeners))
        {
            return;
        }

        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            UtilEventListenerBase candidate = listeners[i];
            if (!IsAlive(candidate) || ReferenceEquals(candidate, listener))
            {
                listeners.RemoveAt(i);
            }
        }

        RemoveEmptyList(eventType, listeners);
    }

    public static void TriggerEvent<Event>(Event newEvent) where Event : struct
    {
        Type eventType = typeof(Event);
        if (!SubscribersByType.TryGetValue(eventType, out List<UtilEventListenerBase> listeners))
        {
            return;
        }

        PruneDeadListeners(listeners);
        if (listeners.Count == 0)
        {
            return;
        }

        UtilEventListenerBase[] snapshot = listeners.ToArray();
        for (int i = snapshot.Length - 1; i >= 0; i--)
        {
            if (!IsAlive(snapshot[i]) || snapshot[i] is not UtilEventListener<Event> listener)
            {
                continue;
            }

            try
            {
                listener.OnTriggerEvent(newEvent);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    private static bool SubscriptionExists(
        IReadOnlyList<UtilEventListenerBase> listeners,
        UtilEventListenerBase listener)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            if (ReferenceEquals(listeners[i], listener))
            {
                return true;
            }
        }

        return false;
    }

    private static void PruneDeadListeners(List<UtilEventListenerBase> listeners)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            if (!IsAlive(listeners[i]))
            {
                listeners.RemoveAt(i);
            }
        }
    }

    private static void RemoveEmptyList(Type eventType, List<UtilEventListenerBase> listeners)
    {
        if (listeners.Count == 0)
        {
            SubscribersByType.Remove(eventType);
        }
    }

    private static bool IsAlive(UtilEventListenerBase listener)
    {
        if (listener == null)
        {
            return false;
        }

        return listener is not UnityEngine.Object unityObject || unityObject != null;
    }
}

public static class EventPayloadSnapshot
{
    public static IReadOnlyList<T> Copy<T>(IReadOnlyList<T> source)
    {
        if (source == null || source.Count == 0)
        {
            return Array.Empty<T>();
        }

        T[] copy = new T[source.Count];
        for (int index = 0; index < source.Count; index++)
        {
            copy[index] = source[index];
        }

        return Array.AsReadOnly(copy);
    }
}

public static class ReadOnlyView
{
    public static IReadOnlyList<T> List<T>(IList<T> source)
    {
        return new ListAdapter<T>(source ?? throw new ArgumentNullException(nameof(source)));
    }

    public static IReadOnlyCollection<T> Collection<T>(ICollection<T> source)
    {
        return new CollectionAdapter<T>(source ?? throw new ArgumentNullException(nameof(source)));
    }

    public static IReadOnlyDictionary<TKey, TValue> Dictionary<TKey, TValue>(
        IDictionary<TKey, TValue> source)
    {
        return new DictionaryAdapter<TKey, TValue>(
            source ?? throw new ArgumentNullException(nameof(source)));
    }

    private sealed class ListAdapter<T> : IReadOnlyList<T>
    {
        private readonly IList<T> source;

        public ListAdapter(IList<T> source)
        {
            this.source = source;
        }

        public int Count => source.Count;
        public T this[int index] => source[index];
        public IEnumerator<T> GetEnumerator() => source.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class CollectionAdapter<T> : IReadOnlyCollection<T>
    {
        private readonly ICollection<T> source;

        public CollectionAdapter(ICollection<T> source)
        {
            this.source = source;
        }

        public int Count => source.Count;
        public IEnumerator<T> GetEnumerator() => source.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class DictionaryAdapter<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> source;
        private readonly IReadOnlyCollection<TKey> keys;
        private readonly IReadOnlyCollection<TValue> values;

        public DictionaryAdapter(IDictionary<TKey, TValue> source)
        {
            this.source = source;
            keys = new CollectionAdapter<TKey>(source.Keys);
            values = new CollectionAdapter<TValue>(source.Values);
        }

        public int Count => source.Count;
        public TValue this[TKey key] => source[key];
        public IEnumerable<TKey> Keys => keys;
        public IEnumerable<TValue> Values => values;
        public bool ContainsKey(TKey key) => source.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value) => source.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => source.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

public interface UtilEventListenerBase { }

public interface UtilEventListener<T> : UtilEventListenerBase
{
    void OnTriggerEvent(T eventType);
}

public static class EventRegister
{
    public delegate void Delegate<T>(T eventType);

    public static void EventStartListening<EventType>(this UtilEventListener<EventType> caller) where EventType : struct
    {
        EventObserver.AddListener<EventType>(caller);
    }

    public static void EventStopListening<EventType>(this UtilEventListener<EventType> caller) where EventType : struct
    {
        EventObserver.RemoveListener<EventType>(caller);
    }
}
public class EventListenerWrapper<TOwner, TTarget, TEvent> : UtilEventListener<TEvent>, IDisposable
    where TEvent : struct
{
    private Action<TTarget> _callback;

    private TOwner _owner;
    public EventListenerWrapper(TOwner owner, Action<TTarget> callback)
    {
        _owner = owner;
        _callback = callback;
        RegisterCallbacks(true);
    }

    public void Dispose()
    {
        RegisterCallbacks(false);
        _callback = null;
    }

    protected virtual TTarget OnEvent(TEvent eventType) => default;
    public void OnTriggerEvent(TEvent eventType)
    {
        var item = OnEvent(eventType);
        _callback?.Invoke(item);
    }

    private void RegisterCallbacks(bool b)
    {
        if (b)
        {
            this.EventStartListening<TEvent>();
        }
        else
        {
            this.EventStopListening<TEvent>();
        }
    }
}
public struct GameEvent
{
    public string EventName;
    public GameEvent(string newName)
    {
        EventName = newName;
    }

    public static void Trigger(string newName)
    {
        EventObserver.TriggerEvent(new GameEvent(newName));
    }
}
