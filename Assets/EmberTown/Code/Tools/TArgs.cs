// <summary>
using System.Collections.Generic;

/// Generic Arguments -
/// 
/// Pass N Arguments with an especified Args Data Type
/// </summary>
/// <typeparam name="T"></typeparam>
public class TArgs<T>
{

    public T[] Values { get; private set; }

    public readonly Dictionary<string, T> NamedValues;


    // Default Initializer
    public TArgs(params T[] arguments)
    {

        Values = arguments;
        NamedValues = new Dictionary<string, T>();
    }

    // Dictionary Initializer
    public TArgs(string key, T value)
    {

        NamedValues = new Dictionary<string, T>();
        NamedValues[key] = value;


        Values = new T[] { value };
    }

    // Get by Index
    public T Get(int index)
    {
        if (index >= 0 && index < Values.Length)
            return Values[index];

        return default;
    }

    public T Get(string key)
    {
        // If the key is valid gather the T value result
        if (NamedValues != null && NamedValues.TryGetValue(key, out T result))
            return result;


        return default;
    }


    // Use Examples
    /*// 1: 3 args of type 'int'
        GArgs<int> enemyStats = new GArgs<int>(100, 50, 10);
        Debug.Log("Vida del enemigo: " + enemyStats.Get(0));

        // 2: 5 args of type 'string'
        GArgs<string> dialogLines = new GArgs<string>(
            "Hola", 
            "¿Qué tal?", 
            "Adiós", 
            "Vuelve pronto", 
            "..."
        );
        
        // 3: Custom data types
        GArgs<CharacterSubsystem.CharacterNames> teamMembers = new GArgs<CharacterSubsystem.CharacterNames>(
            CharacterSubsystem.CharacterNames.Hero,
            CharacterSubsystem.CharacterNames.Amira
        );

        // Iteration
        foreach (string line in dialogLines.Values)
        {
            Debug.Log(line);
        }
        */



}
