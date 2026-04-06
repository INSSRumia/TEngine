using System;

namespace GameLogic
{   
    public static class GameEvent
    {
        public static TEngine.EventMgr EventMgr => TEngine.GameEvent.EventMgr;

        public static bool AddEventListener(EventId eventId, Action handler)
        {
            return TEngine.GameEvent.AddEventListener(eventId, handler);
        }
        public static bool AddEventListener<T>(EventId<T> eventId, Action<T> handler)
        {
            return TEngine.GameEvent.AddEventListener(eventId, handler);
        }
        public static bool AddEventListener<T1, T2>(EventId<T1, T2> eventId, Action<T1, T2> handler)
        {
            return TEngine.GameEvent.AddEventListener(eventId, handler);
        }
        public static bool AddEventListener<T1, T2, T3>(EventId<T1, T2, T3> eventId, Action<T1, T2, T3> handler)
        {
            return TEngine.GameEvent.AddEventListener(eventId, handler);
        }
        public static bool AddEventListener<T1, T2, T3, T4>(EventId<T1, T2, T3, T4> eventId, Action<T1, T2, T3, T4> handler)
        {
            return TEngine.GameEvent.AddEventListener(eventId, handler);
        }
        public static bool AddEventListener<T1, T2, T3, T4, T5>(EventId<T1, T2, T3, T4, T5> eventId, Action<T1, T2, T3, T4, T5> handler)
        {
            return TEngine.GameEvent.AddEventListener(eventId, handler);
        }
        public static bool AddEventListener<T1, T2, T3, T4, T5, T6>(EventId<T1, T2, T3, T4, T5, T6> eventId, Action<T1, T2, T3, T4, T5, T6> handler)
        {
            return TEngine.GameEvent.AddEventListener(eventId, handler);
        }

        public static void RemoveEventListener(EventId eventId, Action handler)
        {
            TEngine.GameEvent.RemoveEventListener(eventId, handler);
        }
        public static void RemoveEventListener<T>(EventId<T> eventId, Action<T> handler)
        {
            TEngine.GameEvent.RemoveEventListener(eventId, handler);
        }
        public static void RemoveEventListener<T1, T2>(EventId<T1, T2> eventId, Action<T1, T2> handler)
        {
            TEngine.GameEvent.RemoveEventListener(eventId, handler);
        }
        public static void RemoveEventListener<T1, T2, T3>(EventId<T1, T2, T3> eventId, Action<T1, T2, T3> handler)
        {
            TEngine.GameEvent.RemoveEventListener(eventId, handler);
        }
        public static void RemoveEventListener<T1, T2, T3, T4>(EventId<T1, T2, T3, T4> eventId, Action<T1, T2, T3, T4> handler)
        {
            TEngine.GameEvent.RemoveEventListener(eventId, handler);
        }
        public static void RemoveEventListener<T1, T2, T3, T4, T5>(EventId<T1, T2, T3, T4, T5> eventId, Action<T1, T2, T3, T4, T5> handler)
        {
            TEngine.GameEvent.RemoveEventListener(eventId, handler);
        }
        public static void RemoveEventListener<T1, T2, T3, T4, T5, T6>(EventId<T1, T2, T3, T4, T5, T6> eventId, Action<T1, T2, T3, T4, T5, T6> handler)
        {
            TEngine.GameEvent.RemoveEventListener(eventId, handler);
        }

        public static void Send(EventId eventId)
        {
            TEngine.GameEvent.Send(eventId);
        }

        public static void Send<T>(EventId<T> eventId, T arg)
        {
            TEngine.GameEvent.Send<T>(eventId, arg);
        }
        public static void Send<T1, T2>(EventId<T1, T2> eventId, T1 arg1, T2 arg2)
        {
            TEngine.GameEvent.Send<T1, T2>(eventId, arg1, arg2);
        }
        public static void Send<T1, T2, T3>(EventId<T1, T2, T3> eventId, T1 arg1, T2 arg2, T3 arg3)
        {
            TEngine.GameEvent.Send<T1, T2, T3>(eventId, arg1, arg2, arg3);
        }
        public static void Send<T1, T2, T3, T4>(EventId<T1, T2, T3, T4> eventId, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            TEngine.GameEvent.Send<T1, T2, T3, T4>(eventId, arg1, arg2, arg3, arg4);
        }
        public static void Send<T1, T2, T3, T4, T5>(EventId<T1, T2, T3, T4, T5> eventId, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            TEngine.GameEvent.Send<T1, T2, T3, T4, T5>(eventId, arg1, arg2, arg3, arg4, arg5);
        }
        public static void Send<T1, T2, T3, T4, T5, T6>(EventId<T1, T2, T3, T4, T5, T6> eventId, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            TEngine.GameEvent.Send<T1, T2, T3, T4, T5, T6>(eventId, arg1, arg2, arg3, arg4, arg5, arg6);
        }
    }
}