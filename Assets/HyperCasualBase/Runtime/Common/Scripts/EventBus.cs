
using System;
using System.Collections.Generic;

public static class EventBus
{
    // Dictionary kiểu event type -> List listener
    private static readonly Dictionary<Type, List<Delegate>> _eventTable
        = new Dictionary<Type, List<Delegate>>();

    /// <summary>
    /// Đăng ký lắng nghe một event E
    /// </summary>
    public static void Subscribe<T>(Action<T> listener)
    {
        var type = typeof(T);

        if (!_eventTable.TryGetValue(type, out var list))
        {
            list = new List<Delegate>(4);
            _eventTable[type] = list;
        }

        if (!list.Contains(listener))
            list.Add(listener);
    }

    /// <summary>
    /// Đăng ký event không có payload
    /// </summary>
    public static void Subscribe<T>(Action listener)
    {
        var type = typeof(T);

        if (!_eventTable.TryGetValue(type, out var list))
        {
            list = new List<Delegate>(4);
            _eventTable[type] = list;
        }

        if (!list.Contains(listener))
            list.Add(listener);
    }

    /// <summary>
    /// Hủy đăng ký
    /// </summary>
    public static void Unsubscribe<T>(Action<T> listener)
    {
        var type = typeof(T);

        if (_eventTable.TryGetValue(type, out var list))
        {
            list.Remove(listener);
        }
    }

    public static void Unsubscribe<T>(Action listener)
    {
        var type = typeof(T);

        if (_eventTable.TryGetValue(type, out var list))
        {
            list.Remove(listener);
        }
    }

    /// <summary>
    /// Gửi event có payload
    /// </summary>
    public static void Publish<T>(T eventData)
    {
        var type = typeof(T);

        if (!_eventTable.TryGetValue(type, out var list))
            return;

        // copy ra để tránh modify khi đang invoke
        var listeners = list.ToArray();

        for (int i = 0; i < listeners.Length; i++)
        {
            if (listeners[i] is Action<T> callback)
                callback(eventData);
        }
    }

    /// <summary>
    /// Gửi event không có payload
    /// </summary>
    public static void Publish<T>()
    {
        var type = typeof(T);

        if (!_eventTable.TryGetValue(type, out var list))
            return;

        var listeners = list.ToArray();

        for (int i = 0; i < listeners.Length; i++)
        {
            if (listeners[i] is Action callback)
                callback();
        }
    }

    /// <summary>
    /// Xoá sạch tất cả event (mỗi lần đổi scene có thể call)
    /// </summary>
    public static void Clear()
    {
        _eventTable.Clear();
    }
}
