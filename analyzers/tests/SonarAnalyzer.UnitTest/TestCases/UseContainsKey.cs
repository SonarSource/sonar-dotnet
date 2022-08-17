using System.Collections.Generic;
using System.Linq;

class Compliant
{
    bool UsesContainsKey()
    {
        var dict = new Dictionary<int, int>();
        return dict.ContainsKey(42); // Compliant
    }

    bool KeysNotFollowedByContains()
    {
        var dict = new Dictionary<int, int>();
        return dict.Keys.Any(); // Compliant
    }

    IEnumerable<int> KeysNotFollowedByAnything()
    {
        var dict = new Dictionary<int, int>();
        return dict.Keys; // Compliant
    }

    bool NoDictionary()
    {
        var dict = new NoDictionary();
        return dict.Keys.Contains(42); // Compliant
    }
}

class NonCompliant
{
    bool KeysContains()
    { 
        var dict = new Dictionary<int, int>();
        return dict.Keys.Contains(42); // NonCompliant {{Use ContainsKey() instead.}}
        //               ^^^^^^^^
    }
}

class NoDictionary
{
    public List<int> Keys { get; }
}
