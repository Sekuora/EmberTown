using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

/// <summary>
/// Dictionary of TArgs 
/// It's necessary to wrap the elements passed onto the dictionary
/// into a TArgs T[] Values array for this method to work properly.
/// </summary>
public class DArgs
{

    private readonly Dictionary<Type, object> _packedArgs = new Dictionary<Type, object>();

    public DArgs(params object[] TArgsArray)
    {
        foreach (var item in TArgsArray)
        {
            if (item == null) continue;

            Type itemType = item.GetType();

            if (itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(TArgs<>))
            {
                Type innerType = itemType.GetGenericArguments()[0];

                _packedArgs[innerType] = item;
            }
            else
            {
                Debug.Log($"DArgs constructor expects TArgs<T> objects. Ignored type: {itemType.Name}");
            }
        }
    }


    public TArgs<T> GetAll<T>()
    {
        if (_packedArgs.TryGetValue(typeof(T), out var packedArg))
        {
            return (TArgs<T>)packedArg;

        }

        return new TArgs<T>();
    }


    public T Get<T>(int index)
    {
        TArgs<T> packedArg = GetAll<T>();
        return packedArg.Get(index);
    }

    public T GetByString<T>(string key)
    {
        TArgs<T> packedArg = GetAll<T>();
        return packedArg.Get(key);
    }
}

