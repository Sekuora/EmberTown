

using System;
using UnityEngine;

public class Args
{

    public object[] Values { get; private set; }

    public Args(params object[] arguments)
    {
        Values = arguments;
    }


    public T Get<T>(int index)
    {
        if (index >= 0 && index < Values.Length)
        {
            var value = Values[index];

            if (value == null) return default;

            if (value is T typedValue) return typedValue;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (InvalidCastException)
            {
                Debug.Log("Cast Failed");
            }
        }

        return default;
    }

}