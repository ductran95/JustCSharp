using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace JustCSharp.Utility.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out var obj) ? obj : default;
        }
        
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory)
        {
            if (dictionary.TryGetValue(key, out var obj))
            {
                return obj;
            }

            return dictionary[key] = factory(key);
        }
        
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> factory) where TKey: notnull
        {
            return dictionary.GetOrAdd(key, _ => factory());
        }
        
        public static TValue GetOrAdd<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> factory) where TKey: notnull
        {
            return dictionary.GetOrAdd(key, _ => factory());
        }
    }
}