namespace GameLogic
{   
    public struct EventId
    {
        public readonly int Id;
        public EventId(int id)
        {
            Id = id;
        }
        public static implicit operator int(EventId id)
        {
            return id.Id;
        }
        public static implicit operator EventId(int id)
        {
            return new EventId(id);
        }
    }

    public struct EventId<T>
    {
        public readonly int Id;
        public EventId(int id)
        {
            Id = id;
        }
        public static implicit operator int(EventId<T> id)
        {
            return id.Id;
        }
        public static implicit operator EventId<T>(int id)
        {
            return new EventId<T>(id);
        }
    }

    public struct EventId<T1, T2>
    {
        public readonly int Id;
        public EventId(int id)
        {
            Id = id;
        }
        public static implicit operator int(EventId<T1, T2> id)
        {
            return id.Id;
        }
        public static implicit operator EventId<T1, T2>(int id)
        {
            return new EventId<T1, T2>(id);
        }
    }
    public struct EventId<T1, T2, T3>
    {
        public readonly int Id;
        public EventId(int id)
        {
            Id = id;
        }
        public static implicit operator int(EventId<T1, T2, T3> id)
        {
            return id.Id;
        }
        public static implicit operator EventId<T1, T2, T3>(int id)
        {
            return new EventId<T1, T2, T3>(id);
        }
    }
    public struct EventId<T1, T2, T3, T4>
    {
        public readonly int Id;
        public EventId(int id)
        {
            Id = id;
        }
        public static implicit operator int(EventId<T1, T2, T3, T4> id)
        {
            return id.Id;
        }
        public static implicit operator EventId<T1, T2, T3, T4>(int id)
        {
            return new EventId<T1, T2, T3, T4>(id);
        }
    }

    public struct EventId<T1, T2, T3, T4, T5>
    {
        public readonly int Id;
        public EventId(int id)
        {
            Id = id;
        }
        public static implicit operator int(EventId<T1, T2, T3, T4, T5> id)
        {
            return id.Id;
        }
        public static implicit operator EventId<T1, T2, T3, T4, T5>(int id)
        {
            return new EventId<T1, T2, T3, T4, T5>(id);
        }
    }
    public struct EventId<T1, T2, T3, T4, T5, T6>
    {
        public readonly int Id;
        public EventId(int id)
        {
            Id = id;
        }
        public static implicit operator int(EventId<T1, T2, T3, T4, T5, T6> id)
        {
            return id.Id;
        }
        public static implicit operator EventId<T1, T2, T3, T4, T5, T6>(int id)
        {
            return new EventId<T1, T2, T3, T4, T5, T6>(id);
        }
    }
}