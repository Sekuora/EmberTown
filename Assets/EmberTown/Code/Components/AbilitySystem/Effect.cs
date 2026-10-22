using System;
using UnityEngine;

public class Effect
{

    public EffectsDB Id { get; private set; }

    public object Value { get; private set; }


    public T GetValue<T>()
    {
        if (Value == null) { return default; }

        if (Value is T typedValue) { return typedValue; }

        try
        {
            return (T)Convert.ChangeType(Value, typeof(T));
        }
        catch (InvalidCastException)
        {
            Debug.Log("Effect Cast Failed");
        }

        return default;
    }


    // Effect Initializer
    public Effect(EffectsDB inId, object inValue)
    {
        Id = inId;

        Value = inValue;
    }

}
