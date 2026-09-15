using System;

[System.Serializable]
public struct SerializableTuple<T1>
{
    public T1 Item1;
   
    public static implicit operator
    SerializableTuple<T1>(
        ValueTuple<T1> value
    )
    {
        return new SerializableTuple<T1>
        {
            Item1 = value.Item1
        };
    }
   
    public static implicit operator
    ValueTuple<T1>(
        SerializableTuple<T1> value
    )
    {
        return new ValueTuple<T1>
        {
            Item1 = value.Item1
        };
    }

    public override string ToString()
    {
        return $"({Item1})";
    }
}

[System.Serializable]
public struct SerializableTuple<T1, T2>
{
    public T1 Item1;
    public T2 Item2;
   
    public static implicit operator
    SerializableTuple<T1, T2>(
        ValueTuple<T1, T2> value
    )
    {
        return new SerializableTuple<T1, T2>
        {
            Item1 = value.Item1,
            Item2 = value.Item2
        };
    }
   
    public static implicit operator
    ValueTuple<T1, T2>(
        SerializableTuple<T1, T2> value
    )
    {
        return new ValueTuple<T1, T2>
        {
            Item1 = value.Item1,
            Item2 = value.Item2
        };
    }

    public override string ToString()
    {
        return $"({Item1}, {Item2})";
    }
}

[System.Serializable]
public struct SerializableTuple<T1, T2, T3>
{
    public T1 Item1;
    public T2 Item2;
    public T3 Item3;
   
    public static implicit operator
    SerializableTuple<T1, T2, T3>(
        ValueTuple<T1, T2, T3> value
    )
    {
        return new SerializableTuple<T1, T2, T3>
        {
            Item1 = value.Item1,
            Item2 = value.Item2,
            Item3 = value.Item3
        };
    }
   
    public static implicit operator
    ValueTuple<T1, T2, T3>(
        SerializableTuple<T1, T2, T3> value
    )
    {
        return new ValueTuple<T1, T2, T3>
        {
            Item1 = value.Item1,
            Item2 = value.Item2,
            Item3 = value.Item3
        };
    }

    public override string ToString()
    {
        return $"({Item1}, {Item2}, {Item3})";
    }
}

[System.Serializable]
public struct SerializableTuple<T1, T2, T3, T4>
{
    public T1 Item1;
    public T2 Item2;
    public T3 Item3;
    public T4 Item4;
   
    public static implicit operator
    SerializableTuple<T1, T2, T3, T4>(
        ValueTuple<T1, T2, T3, T4> value
    )
    {
        return new SerializableTuple<T1, T2, T3, T4>
        {
            Item1 = value.Item1,
            Item2 = value.Item2,
            Item3 = value.Item3,
            Item4 = value.Item4
        };
    }
   
    public static implicit operator
    ValueTuple<T1, T2, T3, T4>(
        SerializableTuple<T1, T2, T3, T4> value
    )
    {
        return new ValueTuple<T1, T2, T3, T4>
        {
            Item1 = value.Item1,
            Item2 = value.Item2,
            Item3 = value.Item3,
            Item4 = value.Item4
        };
    }

    public override string ToString()
    {
        return $"({Item1}, {Item2}, {Item3}, {Item4})";
    }
}

[System.Serializable]
public struct SerializableTuple<T1, T2, T3, T4, T5>
{
    public T1 Item1;
    public T2 Item2;
    public T3 Item3;
    public T4 Item4;
    public T5 Item5;
   
    public static implicit operator
    SerializableTuple<T1, T2, T3, T4, T5>(
        ValueTuple<T1, T2, T3, T4, T5> value
    )
    {
        return new SerializableTuple<T1, T2, T3, T4, T5>
        {
            Item1 = value.Item1,
            Item2 = value.Item2,
            Item3 = value.Item3,
            Item4 = value.Item4,
            Item5 = value.Item5
        };
    }
   
    public static implicit operator
    ValueTuple<T1, T2, T3, T4, T5>(
        SerializableTuple<T1, T2, T3, T4, T5> value
    )
    {
        return new ValueTuple<T1, T2, T3, T4, T5>
        {
            Item1 = value.Item1,
            Item2 = value.Item2,
            Item3 = value.Item3,
            Item4 = value.Item4,
            Item5 = value.Item5
        };
    }

    public override string ToString()
    {
        return $"({Item1}, {Item2}, {Item3}, {Item4}, {Item5})";
    }
}

[System.Serializable]
public struct SerializableTuple<T1, T2, T3, T4, T5, T6>
{
    public T1 Item1;
    public T2 Item2;
    public T3 Item3;
    public T4 Item4;
    public T5 Item5;
    public T6 Item6;
   
    public static implicit operator
    SerializableTuple<T1, T2, T3, T4, T5, T6>(
        ValueTuple<T1, T2, T3, T4, T5, T6> value
    )
    {
        return new SerializableTuple<T1, T2, T3, T4, T5, T6>
        {
            Item1 = value.Item1,
            Item2 = value.Item2,
            Item3 = value.Item3,
            Item4 = value.Item4,
            Item5 = value.Item5,
            Item6 = value.Item6
        };
    }
   
    public static implicit operator
    ValueTuple<T1, T2, T3, T4, T5, T6>(
        SerializableTuple<T1, T2, T3, T4, T5, T6> value
    )
    {
        return new ValueTuple<T1, T2, T3, T4, T5, T6>
        {
            Item1 = value.Item1,
            Item2 = value.Item2,
            Item3 = value.Item3,
            Item4 = value.Item4,
            Item5 = value.Item5,
            Item6 = value.Item6
        };
    }

    public override string ToString()
    {
        return $"({Item1}, {Item2}, {Item3}, {Item4}, {Item5}, {Item6})";
    }
}

[System.Serializable]
public struct SerializableTuple<T1, T2, T3, T4, T5, T6, T7>
{
    public T1 Item1;
    public T2 Item2;
    public T3 Item3;
    public T4 Item4;
    public T5 Item5;
    public T6 Item6;
    public T7 Item7;
   
    public static implicit operator
    SerializableTuple<T1, T2, T3, T4, T5, T6, T7>(
        ValueTuple<T1, T2, T3, T4, T5, T6, T7> value
    )
    {
        return new SerializableTuple<T1, T2, T3, T4, T5, T6, T7>
        {
            Item1 = value.Item1,
            Item2 = value.Item2,
            Item3 = value.Item3,
            Item4 = value.Item4,
            Item5 = value.Item5,
            Item6 = value.Item6,
            Item7 = value.Item7
        };
    }
   
    public static implicit operator
    ValueTuple<T1, T2, T3, T4, T5, T6, T7>(
        SerializableTuple<T1, T2, T3, T4, T5, T6, T7> value
    )
    {
        return new ValueTuple<T1, T2, T3, T4, T5, T6, T7>
        {
            Item1 = value.Item1,
            Item2 = value.Item2,
            Item3 = value.Item3,
            Item4 = value.Item4,
            Item5 = value.Item5,
            Item6 = value.Item6,
            Item7 = value.Item7
        };
    }

    public override string ToString()
    {
        return $"({Item1}, {Item2}, {Item3}, {Item4}, {Item5}, {Item6}, {Item7})";
    }
}

[System.Serializable]
public struct SerializableTuple<T1, T2, T3, T4, T5, T6, T7, TRest>
  where TRest : struct
{
    public T1 Item1;
    public T2 Item2;
    public T3 Item3;
    public T4 Item4;
    public T5 Item5;
    public T6 Item6;
    public T7 Item7;
    public TRest Rest;
   
    public static implicit operator
    SerializableTuple<T1, T2, T3, T4, T5, T6, T7, TRest>(
        ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> value
    )
    {
        return new SerializableTuple<T1, T2, T3, T4, T5, T6, T7, TRest>
        {
            Item1 = value.Item1,
            Item2 = value.Item2,
            Item3 = value.Item3,
            Item4 = value.Item4,
            Item5 = value.Item5,
            Item6 = value.Item6,
            Item7 = value.Item7,
            Rest = value.Rest
        };
    }
   
    public static implicit operator
    ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>(
        SerializableTuple<T1, T2, T3, T4, T5, T6, T7, TRest> value
    )
    {
        return new ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>
        {
            Item1 = value.Item1,
            Item2 = value.Item2,
            Item3 = value.Item3,
            Item4 = value.Item4,
            Item5 = value.Item5,
            Item6 = value.Item6,
            Item7 = value.Item7,
            Rest = value.Rest
        };
    }

    public override string ToString()
    {
        return $"({Item1}, {Item2}, {Item3}, {Item4}, {Item5}, {Item6}, {Item7}, {Rest})";
    }
}