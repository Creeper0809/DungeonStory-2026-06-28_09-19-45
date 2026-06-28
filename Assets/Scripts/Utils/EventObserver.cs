using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public static class EventObserver
{
    private static Dictionary<Type, List<UtilEventListenerBase>> _subscribersList;

    static EventObserver()
    {
        _subscribersList = new Dictionary<Type, List<UtilEventListenerBase>>();
    }
    public static void AddListener<Event>(UtilEventListener<Event> listener) where Event : struct
    {
        Type eventType = typeof(Event);

        if (!_subscribersList.ContainsKey(eventType))
        {
            _subscribersList[eventType] = new List<UtilEventListenerBase>();
        }

        if (!SubscriptionExists(eventType, listener))
        {
            _subscribersList[eventType].Add(listener);
        }
    }
    public static void RemoveListener<Event>(UtilEventListener<Event> listener) where Event : struct
    {
        Type eventType = typeof(Event);

        if (!_subscribersList.ContainsKey(eventType))
        {
            return;
        }
        List<UtilEventListenerBase> subscriberList = _subscribersList[eventType];
        for (int i = subscriberList.Count - 1; i >= 0; i--)
        {
            if (subscriberList[i] == listener)
            {
                subscriberList.Remove(subscriberList[i]);
                if (subscriberList.Count == 0)
                {
                    _subscribersList.Remove(eventType);
                }

                return;
            }
        }
    }
    public static void TriggerEvent<Event>(Event newEvent) where Event : struct
    {
        List<UtilEventListenerBase> list;
        if (!_subscribersList.TryGetValue(typeof(Event), out list))
            return;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            (list[i] as UtilEventListener<Event>).OnTriggerEvent(newEvent);
        }
    }
    private static bool SubscriptionExists(Type type, UtilEventListenerBase receiver)
    {
        List<UtilEventListenerBase> receivers;

        if (!_subscribersList.TryGetValue(type, out receivers)) return false;

        bool exists = false;

        for (int i = receivers.Count - 1; i >= 0; i--)
        {
            if (receivers[i] == receiver)
            {
                exists = true;
                break;
            }
        }

        return exists;
    }
}
public interface UtilEventListenerBase { };
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
    static GameEvent e;
    public static void Trigger(string newName)
    {
        e.EventName = newName;
        EventObserver.TriggerEvent(e);
    }
}