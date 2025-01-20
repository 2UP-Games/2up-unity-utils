using System;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Unity doesn't know how to serialize a Dictionary, but they do know how to write a docs article that contains sample code that serializes a dictionary.
/// https://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.html
/// </summary>
[Serializable]
public class InspectableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField]
    List<TKey> _keys = new();

    [SerializeField]
    List<TValue> _values = new();

    Dictionary<TKey, TValue> _dictionary = new();

    public Dictionary<TKey, TValue> Dictionary => _dictionary;

    public void OnBeforeSerialize()
    {
          _keys.Clear();
        _values.Clear();

        foreach (var kvp in _dictionary)
        {
              _keys.Add(kvp.Key);
            _values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        var newDict = new Dictionary<TKey, TValue>();

        int max = Mathf.Min(_keys.Count, _values.Count);

        for (int i = 0; i < max; i++)
        {
            if (newDict.ContainsKey(_keys[i]))
                continue;

            newDict.Add(_keys[i], _values[i]);
        }

        _dictionary = newDict;
    }
}
