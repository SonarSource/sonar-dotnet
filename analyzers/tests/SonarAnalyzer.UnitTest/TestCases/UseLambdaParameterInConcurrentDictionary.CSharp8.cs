using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class Programs
{
    void GetOrAdd(ConcurrentDictionary<int, int> dictionary, int key)
    {
        // GetOrAdd(TKey, Func<TKey,TValue>)
        dictionary.GetOrAdd(key, _ => key + 42); // Noncompliant {{Use the lambda parameter instead of capturing the argument 'key'}}
        //                       ^^^^^^^^^^^^^
        dictionary.GetOrAdd(key, _ => key); // Noncompliant

        dictionary.GetOrAdd(42, _ => key + 42);
        dictionary.GetOrAdd(key, key => key + 42); // Compliant (the 'key' referenced is the lambda parameter)
        dictionary.GetOrAdd(42, key => key + 42);
        dictionary.GetOrAdd(key, delegate (int k) { return key + 42; }); // Noncompliant
        dictionary.GetOrAdd(key, delegate (int key) { return key + 42; });

        // GetOrAdd(TKey, TValue)
        dictionary.GetOrAdd(42, 42);
        dictionary.GetOrAdd(key, 42);
        dictionary.GetOrAdd(key, key);

        // GetOrAdd<TArg>(TKey, Func<TKey,TArg,TValue>, TArg)
        dictionary.GetOrAdd(key, (_, arg) => key + 42, 42);   // Noncompliant
        //                       ^^^^^^^^^^^^^^^^^^^^
        dictionary.GetOrAdd(key, delegate (int _, int arg) { return key + 42; }, 42);   // Noncompliant
        dictionary.GetOrAdd(key, (_, arg) => arg + 42, 42);
        dictionary.GetOrAdd(42, (key, arg) => arg + 42, 42);
        dictionary.GetOrAdd(key, (key, arg) => arg + 42, 42);
    }

    void AddOrUpdate(ConcurrentDictionary<int, int> dictionary, int key)
    {
        // AddOrUpdate(TKey, Func<TKey,TValue>, Func<TKey,TValue,TValue>)
        dictionary.AddOrUpdate(key, _ => key, (_, oldValue) => oldValue + 42); // Noncompliant
        //                          ^^^^^^^^
        dictionary.AddOrUpdate(key, _ => key, (_, oldValue) => key + 42);
        //                          ^^^^^^^^ {{Use the lambda parameter instead of capturing the argument 'key'}}
        //                                    ^^^^^^^^^^^^^^^^^^^^^^^^^ @-1 {{Use the lambda parameter instead of capturing the argument 'key'}}
        dictionary.AddOrUpdate(key, _ => 42, (_, oldValue) => oldValue + 42);
        dictionary.AddOrUpdate(42, _ => 42, (_, oldValue) => oldValue + 42);
        dictionary.AddOrUpdate(42, _ => 42, (key, oldValue) => key + 42);

        // AddOrUpdate(TKey, TValue, Func<TKey,TValue,TValue>)
        dictionary.AddOrUpdate(key, 42, (_, oldValue) => key + 42); // Noncompliant
        dictionary.AddOrUpdate(42, key, (_, oldValue) => key + 42);
        dictionary.AddOrUpdate(42, 42, (_, oldValue) => key + 42);
        dictionary.AddOrUpdate(key, key, (_, oldValue) => oldValue + 42);
        dictionary.AddOrUpdate(42, 42, (key, oldValue) => key + 42);
        dictionary.AddOrUpdate(42, 42, (_, oldValue) => oldValue + 42);

        // AddOrUpdate<TArg>(TKey, Func<TKey,TArg,TValue>, Func<TKey,TValue,TArg,TValue>, TArg)
        dictionary.AddOrUpdate(key, (_, arg) => key, (_, oldValue, arg) => oldValue + arg, 42); // Noncompliant
        dictionary.AddOrUpdate(key, (_, arg) => key, (_, oldValue, arg) => oldValue + key + arg, 42);
        //                          ^^^^^^^^^^^^^^^
        //                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-1
        dictionary.AddOrUpdate(key, (_, arg) => arg, (_, oldValue, arg) => oldValue + arg, 42);
        dictionary.AddOrUpdate(42, (_, arg) => key, (_, oldValue, arg) => oldValue + arg, 42);
        dictionary.AddOrUpdate(42, (_, arg) => key, (_, oldValue, arg) => oldValue + arg, 42);
        dictionary.AddOrUpdate(42, (_, arg) => arg, (_, oldValue, arg) => oldValue + arg, key);
        dictionary.AddOrUpdate(42, (_, arg) => key, (_, oldValue, arg) => oldValue + arg, key);
        dictionary.AddOrUpdate(key, (_, arg) => key, (_, key, arg) => arg, key); // Noncompliant
        dictionary.AddOrUpdate(42, (_, arg) => key, (_, key, arg) => key + arg, key);
    }

    void CompliantInvocations(ConcurrentDictionary<int, int> dictionary, HidesMethod<int, int> hidesMethod, List<int> list, int key)
    {
        dictionary.TryAdd(key, 42);
        list.Any(x => key > 0);
        hidesMethod.GetOrAdd(key, _ => key);
    }

    void MyDictionary(MyConcurrentDictionary dictionary, int key)
    {
        dictionary.GetOrAdd(key, _ => key + 42); // Noncompliant
    }

    void NameOf(ConcurrentDictionary<string, string> dictionary, string key, string str)
    {
        dictionary.GetOrAdd(key, _ => nameof(key)); // Compliant
        dictionary.GetOrAdd(key, x =>
        {
            var something = $"The name should be {nameof(key)} and not {nameof(x)}"; // Compliant
            return x;
        });
    }

    class MyConcurrentDictionary : ConcurrentDictionary<int, int> { }

    class HidesMethod<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) => default(TValue);
    }
}
