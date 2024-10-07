using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

record R
{
    private static string[] staticStrings = new string[] { "a", "b", "c" };
    private string[] stringArray = new string[10];
    public string[] Property1
    {
        get { return (string[])stringArray.Clone(); } // Noncompliant {{Refactor 'Property1' into a method, properties should not copy collections.}}

        init { stringArray = (string[])staticStrings.Clone(); }
    }

    private List<string> foo = new();
    public IEnumerable<string> Property2
    {
        get => foo.ToArray();
    }

    public string[] Property3
    {
        init => stringArray = value.ToArray();
    }
}

class TestCase
{
    private string[] stringArray = new string[10];

    public string[] Property1
    {
        get { return [..stringArray]; } // FN
    }

    public string[][] Property2
    {
        get { return [stringArray]; } // FN
    }
}


class CSharp13Enumerables
{
    private ReadOnlySet<int> _setWrapper;

    public int[] SetWrapper
    {
        get { return _setWrapper.ToArray(); } // Noncompliant
    }

    public List<int> SetWrapperTwo
    {
        get { return _setWrapper.ToList(); } // Noncompliant
    }

    private OrderedDictionary<int, string> _orderedDict;

    public List<KeyValuePair<int, string>> OrderedDict
    {
        get { return _orderedDict.ToList(); } // Noncompliant
    }

    public KeyValuePair<int, string>[] OrderedDictDict
    {
        get { return _orderedDict.ToArray<KeyValuePair<int, string>>(); } // Noncompliant
    }
}

public partial class PartialProperty
{
    private string[] stringArray = new string[10];
    public partial string[] Property1 { get; }
    public partial IEnumerable<string> Property2 { get; }
    public partial IEnumerable<string> Property3 { get; }
    public partial IEnumerable<string> Property4 { get; }
    public partial IEnumerable<string> Property5 { get; }
    public partial string[] Property6 { get; }
    public partial string[] Property7 { get; }
    public partial string[] CompliantLazyInitialization1 { get; }
    public partial string[] CompliantLazyInitialization2 { get; }
    public partial string[] CompliantCloneInSetter { get; set; }
}

public partial class PartialProperty
{
    public partial string[] Property1
    {
        get { return (string[])stringArray.Clone(); } // Noncompliant
    }

    public partial IEnumerable<string> Property2
    {
        get { return stringArray.ToArray(); } // Noncompliant
    }

    public partial IEnumerable<string> Property3
    {
        get { return stringArray.ToList(); } // Noncompliant
    }

    public partial IEnumerable<string> Property4 => stringArray.ToList(); // Noncompliant

    public partial IEnumerable<string> Property5 => stringArray.Where(s => s != null).ToList(); // Noncompliant

    public partial string[] Property6 => (string[])stringArray.Clone(); // Noncompliant

    public partial string[] Property7
    {
        get => stringArray.ToArray(); // Noncompliant
    }

    public partial string[] CompliantLazyInitialization1
    {
        get { return stringArray ?? (stringArray = (string[])stringArray.Clone()); }
    }

    public partial string[] CompliantLazyInitialization2
    {
        get
        {
            var value = stringArray.ToArray();
            return value;
        }
    }

    public partial string[] CompliantCloneInSetter
    {
        get { return null; }
        set { stringArray = (string[])stringArray.Clone(); }
    }

    public string[] CloneInMethod()
    {
        return (string[])stringArray.Clone();
    }

}
