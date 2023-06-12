using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

class CollectionTests
{
    private IEnumerable<int> items = new List<int> { 1, 2, 3 };
    private IDictionary<int, int> dictionaryItems = new Dictionary<int, int>();

    private static bool Predicate(int i) => true;
    private static void Action(int i) { }
    private List<int> ListFactory() => new List<int>();
    private HashSet<int> SetFactory() => new HashSet<int>();
    private Queue<int> QueueFactory() => new Queue<int>();
    private Stack<int> StackFactory() => new Stack<int>();
    private ObservableCollection<int> ObservableCollectionFactory() => new ObservableCollection<int>();
    private int[] ArrayFactory() => new int[0];
    private Dictionary<int, int> DictionaryFactory() => new Dictionary<int, int>();

    public void DefaultConstructor()
    {
        var list = new List<int>();
        list.Clear();   // Noncompliant {{Remove this call, the collection is known to be empty here.}}
//      ^^^^^^^^^^^^
        var set = new HashSet<int>();
        set.Clear();    // Noncompliant
        var queue = new Queue<int>();
        queue.Clear();  // Noncompliant
        var stack = new Stack<int>();
        stack.Clear();  // Noncompliant
        var obs = new ObservableCollection<int>();
        obs.Clear();    // Noncompliant
        var array = new int[0];
        array.Clone();  // FIXME Non-compliant
        var dict = new Dictionary<int, int>();
        dict.Clear();   // Noncompliant
    }

    public void ConstructorWithCapacity()
    {
        var list = new List<int>(5);
        list.Clear();   // Noncompliant
        var set = new HashSet<int>(5);
        set.Clear();    // Noncompliant
        var queue = new Queue<int>(5);
        queue.Clear();  // Noncompliant
        var stack = new Stack<int>(5);
        stack.Clear();  // Noncompliant
        var array = new int[5];
        array.Clone();  // Compliant
        var dict = new Dictionary<int, int>(5);
        dict.Clear();   // Noncompliant
    }

    public void ConstructorWithEnumerable()
    {
        var list = new List<int>(items);
        list.Clear();   // Noncompliant FP
        var set = new HashSet<int>(items);
        set.Clear();    // Noncompliant FP
        var queue = new Queue<int>(items);
        queue.Clear();  // Noncompliant FP
        var stack = new Stack<int>(items);
        stack.Clear();  // Noncompliant FP
        var obs = new ObservableCollection<int>(items);
        obs.Clear();    // Noncompliant FP
        var dict = new Dictionary<int, int>(dictionaryItems);
        dict.Clear();   // Noncompliant FP
    }

    public void ConstructorWithEmptyInitializer()
    {
        var list = new List<int> { };
        list.Clear();   // Noncompliant
        var set = new HashSet<int> { };
        set.Clear();    // Noncompliant
        var queue = new Queue<int> { };
        queue.Clear();  // Noncompliant
        var stack = new Stack<int> { };
        stack.Clear();  // Noncompliant
        var obs = new ObservableCollection<int> { };
        obs.Clear();    // Noncompliant
        var array = new int[] { };
        array.Clone();  // FIXME Non-compliant
        var dict = new Dictionary<int, int> { };
        dict.Clear();   // Noncompliant
    }

    public void ConstructorWithInitializer()
    {
        var list = new List<int> { 1, 2, 3 };
        list.Clear();   // Noncompliant FP
        var set = new HashSet<int> { 1, 2, 3 };
        set.Clear();    // Noncompliant FP
        var obs = new ObservableCollection<int> { 1, 2, 3 };
        obs.Clear();    // Noncompliant FP
        var array = new int[] { 1, 2, 3 };
        array.Clone();  // Compliant
        var dict = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 2,
            [3] = 3,
        };
        dict.Clear();   // Noncompliant FP
    }

    public void Other_Initialization()
    {
        var list = ListFactory();
        list.Clear();   // Compliant
        var set = SetFactory();
        set.Clear();    // Compliant
        var queue = QueueFactory();
        queue.Clear();  // Compliant
        var stack = StackFactory();
        stack.Clear();  // Compliant
        var obs = ObservableCollectionFactory();
        obs.Clear();    // Compliant
        var array = ArrayFactory();
        array.Clone();  // Compliant
        var dict = DictionaryFactory();
        dict.Clear();   // Compliant
    }

    public void Methods_Raise_Issue()
    {
        int i;

        var list = new List<int>();
        list.BinarySearch(5);           // Noncompliant
        list.Clear();                   // Noncompliant
        list.Contains(5);               // Noncompliant
        list.ConvertAll(x => x);        // Noncompliant
        list.CopyTo(null, 1);           // Noncompliant
        list.Exists(Predicate);         // Noncompliant
        list.Find(Predicate);           // Noncompliant
        list.FindAll(Predicate);        // Noncompliant
        list.FindIndex(Predicate);      // Noncompliant
        list.FindLast(Predicate);       // Noncompliant
        list.FindLastIndex(Predicate);  // Noncompliant
        list.ForEach(Action);           // Noncompliant
        list.GetEnumerator();           // Noncompliant
        list.GetRange(1, 5);            // Noncompliant
        list.IndexOf(5);                // Noncompliant
        list.LastIndexOf(5);            // Noncompliant
        list.Remove(5);                 // Noncompliant
        list.RemoveAll(Predicate);      // Noncompliant
        list.RemoveAt(1);               // Noncompliant
        list.RemoveRange(1, 5);         // Noncompliant
        list.Reverse();                 // Noncompliant
        list.Sort();                    // Noncompliant
        list.TrueForAll(Predicate);     // Noncompliant
        _ = list[1];                    // FIXME Non-compliant
//          ~~~~~~~
        list[1] = 5;                    // FIXME Non-compliant
//      ~~~~~~~

        var set = new HashSet<int>();
        set.Clear();                    // Noncompliant
        set.Contains(5);                // Noncompliant
        set.CopyTo(null, 1);            // Noncompliant
        set.ExceptWith(items);          // Noncompliant
        set.GetEnumerator();            // Noncompliant
        set.IntersectWith(items);       // Noncompliant
        set.IsProperSubsetOf(items);    // Noncompliant
        set.IsProperSupersetOf(items);  // Noncompliant
        set.IsSubsetOf(items);          // Noncompliant
        set.IsSupersetOf(items);        // Noncompliant
        set.Overlaps(items);            // Noncompliant
        set.Remove(5);                  // Noncompliant
        set.RemoveWhere(Predicate);     // Noncompliant
        set.SymmetricExceptWith(items); // Noncompliant, also learns NotEmpty
        set = new HashSet<int>();
        set.TryGetValue(5, out i);      // Noncompliant
        set.UnionWith(items);           // Noncompliant, also learns NotEmpty

        var queue = new Queue<int>();
        queue.Clear();                  // Noncompliant
        queue.Contains(5);              // Noncompliant
        queue.CopyTo(null, 1);          // Noncompliant
        queue.Dequeue();                // Noncompliant
        queue.GetEnumerator();          // Noncompliant
        queue.Peek();                   // Noncompliant

        var stack = new Stack<int>();
        stack.Clear();                  // Noncompliant
        stack.Contains(5);              // Noncompliant
        stack.CopyTo(null, 0);          // Noncompliant
        stack.GetEnumerator();          // Noncompliant
        stack.Peek();                   // Noncompliant
        stack.Pop();                    // Noncompliant

        var obs = new ObservableCollection<int>();
        obs.Clear();                    // Noncompliant
        obs.Contains(5);                // Noncompliant
        obs.CopyTo(null, 1);            // Noncompliant
        obs.GetEnumerator();            // Noncompliant
        obs.IndexOf(5);                 // Noncompliant
        obs.Move(1, 2);                 // Noncompliant
        obs.Remove(5);                  // Noncompliant
        obs.RemoveAt(1);                // Noncompliant
        _ = obs[1];                     // FIXME Non-compliant
        obs[1] = 5;                     // FIXME Non-compliant

        var array = new int[0];
        array.Clone();                  // FIXME Non-compliant
        array.CopyTo(null, 1);          // FIXME Non-compliant
        array.GetEnumerator();          // FIXME Non-compliant
        array.GetLength(1);             // FIXME Non-compliant
        array.GetLongLength(1);         // FIXME Non-compliant
        array.GetLowerBound(1);         // FIXME Non-compliant
        array.GetUpperBound(1);         // FIXME Non-compliant
        array.GetValue(1);              // FIXME Non-compliant
        array.Initialize();             // FIXME Non-compliant
        array.SetValue(5, 1);           // FIXME Non-compliant
        _ = array[1];                   // FIXME Non-compliant
        array[1] = 5;                   // FIXME Non-compliant

        var dict = new Dictionary<int, int>();
        dict.Clear();                   // Noncompliant
        dict.ContainsKey(1);            // Noncompliant
        dict.ContainsValue(5);          // Noncompliant
        dict.GetEnumerator();           // Noncompliant
        dict.Remove(5);                 // Noncompliant
        dict.TryGetValue(1, out i);     // Noncompliant
        _ = dict[1];                    // FIXME Non-compliant
    }

    public void Methods_Ignored()
    {
        int i;

        var list = new List<int>();
        list.AsReadOnly();
        list.GetHashCode();
        list.GetType();
        list.Equals(items);
        list.ToString();
        list.TrimExcess();
        list.ToArray();

        var set = new HashSet<int>();
        set.Equals(items);
        set.GetHashCode();
        set.GetObjectData(null, new StreamingContext());
        set.GetType();
        set.OnDeserialization(null);
        set.SetEquals(items);
        set.TrimExcess();

        var queue = new Queue<int>();
        queue.Equals(items);
        queue.GetHashCode();
        queue.GetType();
        queue.ToArray();
        queue.ToString();
        queue.TrimExcess();

        var stack = new Stack<int>();
        stack.GetHashCode();
        stack.Equals(items);
        stack.GetType();
        stack.ToArray();
        stack.ToString();
        stack.TrimExcess();

        var obs = new ObservableCollection<int>();
        obs.GetHashCode();
        obs.Equals(items);
        obs.GetType();
        obs.ToString();
        _ = obs.Count;
        obs.CollectionChanged += (s, e) => throw new NotImplementedException();

        var array = new int[0];
        array.GetHashCode();
        array.Equals(new object());
        array.GetType();
        array.ToString();
        _ = array.Length;

        var dict = new Dictionary<int, int>();
        dict.GetHashCode();
        dict.GetObjectData(null, new StreamingContext());
        dict.Equals(items);
        dict.GetType();
        dict.OnDeserialization(null);
        dict.ToString();
        dict[5] = 5;
        (((dict[5]))) = 5;
    }

    public void Methods_Set_NotEmpty()
    {
        var list = new List<int>();
        list.Add(5);
        list.Clear();                   // Compliant
        list = new List<int>();
        list.AddRange(items);
        list.Clear();                   // Compliant
        list = new List<int>();
        list.Insert(1, 5);
        list.Clear();                   // Compliant
        list = new List<int>();
        list.InsertRange(1, items);
        list.Clear();                   // Compliant

        var set = new HashSet<int>();
        set.Add(1);
        set.Clear();                    // Compliant
        set = new HashSet<int>();
        set.SymmetricExceptWith(items); // Noncompliant
        set.Clear();                    // Compliant
        set = new HashSet<int>();
        set.UnionWith(items);           // Noncompliant
        set.Clear();                    // Compliant

        var queue = new Queue<int>();
        queue.Enqueue(5);
        queue.Clear();                  // Compliant

        var stack = new Stack<int>();
        stack.Push(5);
        stack.Clear();                  // Compliant

        var obs = new ObservableCollection<int>();
        obs.Add(5);
        obs.Clear();                    // Compliant
        obs = new ObservableCollection<int>();
        obs.Insert(0, 5);
        obs.Clear();                    // Compliant

        var dict = new Dictionary<int, int>();
        dict.Add(1, 5);
        dict.Clear();                   // Compliant
    }
}

class AdvancedTests
{
    public void ExtensionMethods_Should_Not_Raise()
    {
        var list = new List<int>();
        list.Any();     // Compliant
    }

    public void ExtensionMethods_Remove_Constraints()
    {
        var list = new List<int>();
        list.Any();
        list.Clear();   // Noncompliant FP
    }

    public void PassingAsArgument_Removes_Constraints()
    {
        var list = new List<int>();
        Foo(list);
        list.Clear();   // Noncompliant FP
    }

    public void HigherRank_And_Jagged_Array_Is_NotEmpty()
    {
        var array1 = new int[0, 0];
        array1.Clone(); // Compliant
        int[][] array2 = new int[1][];
        array2.Clone(); // Compliant
    }

    void Foo(IEnumerable<int> items) { }
}

// This simulates the Dictionary from .NetCore 2.0+.
public class NetCoreDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
    public void TestTryAdd()
    {
        var dict = new NetCoreDictionary<string, object>();
        if (dict.TryAdd("foo", new object()))   // Compliant
        {
        }
    }

    public bool TryAdd(TKey key, TValue value)
    {
        return true;
    }
}

class Flows
{
    public void Conditional_Add(bool condition)
    {
        var list = new List<int>();
        if (condition)
        {
            list.Add(5);
        }
        list.Clear();   // Compliant
    }

    public void Conditional_Add_With_Loop(bool condition)
    {
        var list = new List<int>();
        while (true)
        {
            if (condition)
            {
                list.Add(5);
                break;
            }
        }
        list.Clear();   // Compliant
    }

    public void AddPassedAsParameter()
    {
        var list = new List<int>();

        DoSomething(list.Add);

        list.Clear();   // Noncompliant FP, see https://github.com/SonarSource/sonar-dotnet/issues/4261
    }

    private static void DoSomething(Action<int> callback) => callback(42);
}


class Flows2
{
    public static string UrlDecode(string s, Encoding e)
    {
        long len = s.Length;
        var bytes = new List<byte>();
        int xchar;
        char ch;

        for (int i = 0; i < len; i++)
        {
            ch = s[i];
            if (ch == '%' && i + 2 < len && s[i + 1] != '%')
            {
                if (s[i + 1] == 'u' && i + 5 < len)
                {
                    // unicode hex sequence
                    xchar = GetChar(s, i + 2, 4);
                    if (xchar != -1)
                    {
                        WriteCharBytes(bytes, (char)xchar, e);
                        i += 5;
                    }
                    else
                        WriteCharBytes(bytes, '%', e);
                }
                else if ((xchar = GetChar(s, i + 1, 2)) != -1)
                {
                    WriteCharBytes(bytes, (char)xchar, e);
                    i += 2;
                }
                else
                {
                    WriteCharBytes(bytes, '%', e);
                }
                continue;
            }

            if (ch == '+')
                WriteCharBytes(bytes, ' ', e);
            else
                WriteCharBytes(bytes, ch, e);
        }

        byte[] buf = bytes.ToArray();
        bytes = null;
        return e.GetString(buf);

    }

    private static void WriteCharBytes(List<byte> bytes, char v, Encoding e)
    {
    }

    private static int GetChar(string s, int v1, int v2)
    {
        return 1;
    }
}

// See https://github.com/SonarSource/sonar-dotnet/issues/1002
class Program
{
    public Program(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));
    }
}

class LargeCfg
{
    // Large CFG that causes the exploded graph to hit the exploration limit
    // See https://github.com/SonarSource/sonar-dotnet/issues/767 (comments!)
    private static void MethodName(object[] table, object[][] aoTable2, ref Dictionary<double, string> dictPorts)
    {
        try
        {
            string sValue = "-1";
            switch (5)
            {
                case 3:
                    sValue = "";
                    break;
                case 4:
                    sValue = "";
                    break;
                case 5:
                    sValue = "";
                    break;
            }

            var list = new List<double>();
            for (int i = 0; i < table.Length; i++)
            {
                if (Convert.ToInt32(table[3]) == 1 /* Normal */ &&
                    Convert.ToInt32(aoTable2[5][9]) == 1 /* On */)
                {
                    string sValue2;
                    switch (Convert.ToInt32(aoTable2[5][4]))
                    {
                        default:
                            sValue2 = "";
                            break;
                    }

                    if (dictPorts.ContainsKey(5))
                    {
                        list.Add(7);
                    }
                }
                else
                {
                    dictPorts.Add(5, sValue);
                }
            }

            if (list.Count > 0)
            {
                list.Sort();    // Compliant - used to be an FP
            }
        }
        catch
        {
            // silently do nothing
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/4478
public class Repro_4478
{
    public string Main()
    {
        var list = new List<String>();
        AddInLocalFunction();
        return list[0]; // FIXME Non-compliant FP

        void AddInLocalFunction()
        {
            list.Add("Item1");
        }
    }
}
