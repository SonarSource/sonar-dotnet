using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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
        list.Clear();   // FIXME Non-compliant {{Remove this call, the collection is known to be empty here.}}
//      ~~~~~~~~~~~~
        var set = new HashSet<int>();
        set.Clear();    // FIXME Non-compliant
        var queue = new Queue<int>();
        queue.Clear();  // FIXME Non-compliant
        var stack = new Stack<int>();
        stack.Clear();  // FIXME Non-compliant
        var obs = new ObservableCollection<int>();
        obs.Clear();    // FIXME Non-compliant
        var array = new int[0];
        array.Clone();  // FIXME Non-compliant
        var dict = new Dictionary<int, int>();
        dict.Clear();   // FIXME Non-compliant
    }

    public void ConstructorWithCapacity()
    {
        var list = new List<int>(5);
        list.Clear();   // FIXME Non-compliant
        var set = new HashSet<int>(5);
        set.Clear();    // FIXME Non-compliant
        var queue = new Queue<int>(5);
        queue.Clear();  // FIXME Non-compliant
        var stack = new Stack<int>(5);
        stack.Clear();  // FIXME Non-compliant
        var array = new int[5];
        array.Clone();  // Compliant
        var dict = new Dictionary<int, int>(5);
        dict.Clear();   // FIXME Non-compliant
    }

    public void ConstructorWithEnumerable()
    {
        var list = new List<int>(items);
        list.Clear();   // Compliant
        var set = new HashSet<int>(items);
        set.Clear();    // Compliant
        var queue = new Queue<int>(items);
        queue.Clear();  // Compliant
        var stack = new Stack<int>(items);
        stack.Clear();  // Compliant
        var obs = new ObservableCollection<int>(items);
        obs.Clear();    // Compliant
        var dict = new Dictionary<int, int>(dictionaryItems);
        dict.Clear();   // Compliant
    }

    public void ConstructorWithEmptyInitializer()
    {
        var list = new List<int> { };
        list.Clear();   // FIXME Non-compliant
        var set = new HashSet<int> { };
        set.Clear();    // FIXME Non-compliant
        var queue = new Queue<int> { };
        queue.Clear();  // FIXME Non-compliant
        var stack = new Stack<int> { };
        stack.Clear();  // FIXME Non-compliant
        var obs = new ObservableCollection<int> { };
        obs.Clear();    // FIXME Non-compliant
        var array = new int[] { };
        array.Clone();  // FIXME Non-compliant
        var dict = new Dictionary<int, int> { };
        dict.Clear();   // FIXME Non-compliant
    }

    public void ConstructorWithInitializer()
    {
        var list = new List<int> { 1, 2, 3 };
        list.Clear();   // Compliant
        var set = new HashSet<int> { 1, 2, 3 };
        set.Clear();    // Compliant
        var obs = new ObservableCollection<int> { 1, 2, 3 };
        obs.Clear();    // Compliant
        var array = new int[] { 1, 2, 3 };
        array.Clone();  // Compliant
        var dict = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 2,
            [3] = 3,
        };
        dict.Clear();   // Compliant
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
        list.BinarySearch(5);           // FIXME Non-compliant
        list.Clear();                   // FIXME Non-compliant
        list.Contains(5);               // FIXME Non-compliant
        list.ConvertAll(x => x);        // FIXME Non-compliant
        list.CopyTo(null, 1);           // FIXME Non-compliant
        list.Exists(Predicate);         // FIXME Non-compliant
        list.Find(Predicate);           // FIXME Non-compliant
        list.FindAll(Predicate);        // FIXME Non-compliant
        list.FindIndex(Predicate);      // FIXME Non-compliant
        list.FindLast(Predicate);       // FIXME Non-compliant
        list.FindLastIndex(Predicate);  // FIXME Non-compliant
        list.ForEach(Action);           // FIXME Non-compliant
        list.GetEnumerator();           // FIXME Non-compliant
        list.GetRange(1, 5);            // FIXME Non-compliant
        list.IndexOf(5);                // FIXME Non-compliant
        list.LastIndexOf(5);            // FIXME Non-compliant
        list.Remove(5);                 // FIXME Non-compliant
        list.RemoveAll(Predicate);      // FIXME Non-compliant
        list.RemoveAt(1);               // FIXME Non-compliant
        list.RemoveRange(1, 5);         // FIXME Non-compliant
        list.Reverse();                 // FIXME Non-compliant
        list.Sort();                    // FIXME Non-compliant
        list.TrueForAll(Predicate);     // FIXME Non-compliant
        _ = list[1];                    // FIXME Non-compliant
//          ~~~~~~~
        list[1] = 5;                    // FIXME Non-compliant
//      ~~~~~~~

        var set = new HashSet<int>();
        set.Clear();                    // FIXME Non-compliant
        set.Contains(5);                // FIXME Non-compliant
        set.CopyTo(null, 1);            // FIXME Non-compliant
        set.ExceptWith(items);          // FIXME Non-compliant
        set.GetEnumerator();            // FIXME Non-compliant
        set.IntersectWith(items);       // FIXME Non-compliant
        set.IsProperSubsetOf(items);    // FIXME Non-compliant
        set.IsProperSupersetOf(items);  // FIXME Non-compliant
        set.IsSubsetOf(items);          // FIXME Non-compliant
        set.IsSupersetOf(items);        // FIXME Non-compliant
        set.Overlaps(items);            // FIXME Non-compliant
        set.Remove(5);                  // FIXME Non-compliant
        set.RemoveWhere(Predicate);     // FIXME Non-compliant
        set.SymmetricExceptWith(items); // FIXME Non-compliant
        set.TryGetValue(5, out i);      // FIXME Non-compliant
        set.UnionWith(items);           // FIXME Non-compliant

        var queue = new Queue<int>();
        queue.Clear();                  // FIXME Non-compliant
        queue.Contains(5);              // FIXME Non-compliant
        queue.CopyTo(null, 1);          // FIXME Non-compliant
        queue.Dequeue();                // FIXME Non-compliant
        queue.GetEnumerator();          // FIXME Non-compliant
        queue.Peek();                   // FIXME Non-compliant
        queue.TryDequeue(out i);        // FIXME Non-compliant
        queue.TryPeek(out i);           // FIXME Non-compliant

        var stack = new Stack<int>();
        stack.Clear();                  // FIXME Non-compliant
        stack.Contains(5);              // FIXME Non-compliant
        stack.CopyTo(null, 0);          // FIXME Non-compliant
        stack.GetEnumerator();          // FIXME Non-compliant
        stack.Peek();                   // FIXME Non-compliant
        stack.Pop();                    // FIXME Non-compliant
        stack.TryPeek(out i);           // FIXME Non-compliant
        stack.TryPop(out i);            // FIXME Non-compliant

        var obs = new ObservableCollection<int>();
        obs.Clear();                    // FIXME Non-compliant
        obs.Contains(5);                // FIXME Non-compliant
        obs.CopyTo(null, 1);            // FIXME Non-compliant
        obs.GetEnumerator();            // FIXME Non-compliant
        obs.IndexOf(5);                 // FIXME Non-compliant
        obs.Move(1, 2);                 // FIXME Non-compliant
        obs.Remove(5);                  // FIXME Non-compliant
        obs.RemoveAt(1);                // FIXME Non-compliant
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
        dict.Clear();                   // FIXME Non-compliant
        dict.ContainsKey(1);            // FIXME Non-compliant
        dict.ContainsValue(5);          // FIXME Non-compliant
        dict.GetEnumerator();           // FIXME Non-compliant
        dict.Remove(5);                 // FIXME Non-compliant
        dict.TryGetValue(1, out i);     // FIXME Non-compliant
        _ = dict[1];                    // FIXME Non-compliant
    }

    public void Methods_Ignored()
    {
        int i;

        var list = new List<int>();
        list.AsReadOnly();
        list.GetHashCode();
        list.GetType();
        list.EnsureCapacity(5);
        list.Equals(items);
        list.ToString();
        list.TrimExcess();
        list.ToArray();

        var set = new HashSet<int>();
        set.EnsureCapacity(5);
        set.Equals(items);
        set.GetHashCode();
        set.GetObjectData(null, default);
        set.GetType();
        set.OnDeserialization(null);
        set.SetEquals(items);
        set.TrimExcess();

        var queue = new Queue<int>();
        queue.EnsureCapacity(5);
        queue.Equals(items);
        queue.GetHashCode();
        queue.GetType();
        queue.ToArray();
        queue.ToString();
        queue.TrimExcess();

        var stack = new Stack<int>();
        stack.EnsureCapacity(5);
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
        array.AsReadOnly();
        array.GetHashCode();
        array.GetLength(0);
        array.GetLongLength(0);
        array.GetLowerBound(0);
        array.GetUpperBound(0);
        array.Equals(new object());
        array.GetType();
        array.ToString();
        _ = array.Length;

        var dict = new Dictionary<int, int>();
        dict.EnsureCapacity(5);
        dict.GetHashCode();
        dict.GetObjectData(null, default);
        dict.Equals(items);
        dict.GetType();
        dict.OnDeserialization(null);
        dict.ToString();
        dict.TrimExcess();
        dict.TryGetValue(5, out i);
        dict[5] = 5;
        (((dict[5]))) = 5;
    }

    public void Methods_Set_NotEmpty()
    {
        var list = new List<int>();
        list.Add(5);
        list.Clear();   // Compliant
        list = new List<int>();
        list.AddRange(items);
        list.Clear();   // Compliant
        list = new List<int>();
        list.Insert(1, 5);
        list.Clear();   // Compliant
        list = new List<int>();
        list.InsertRange(1, items);
        list.Clear();   // Compliant

        var set = new HashSet<int>();
        set.Add(1);
        set.Clear();    // Compliant
        set = new HashSet<int>();
        set.SymmetricExceptWith(items);
        set.Clear();    // Compliant
        set = new HashSet<int>();
        set.UnionWith(items);
        set.Clear();    // Compliant

        var queue = new Queue<int>();
        queue.Enqueue(5);
        queue.Clear();  // Compliant

        var stack = new Stack<int>();
        stack.Push(5);
        stack.Clear();  // Compliant

        var obs = new ObservableCollection<int>();
        obs.Add(5);
        obs.Clear();    // Compliant
        obs = new ObservableCollection<int>();
        obs.Insert(0, 5);
        obs.Clear();    // Compliant

        var dict = new Dictionary<int, int>();
        dict.Add(1, 5);
        dict.Clear();   // Compliant
        dict = new Dictionary<int, int>();
        dict.TryAdd(1, 5);
        dict.Clear();   // Compliant
    }
}

class AdvancedTests
{
    public void ExtensionMethods_Should_Not_Raise()
    {
        var list = new List<int>();
        list.Any(); // Compliant
    }

    public void ExtensionMethods_Remove_Constraints()
    {
        var list = new List<int>();
        list.Any();
        list.Clear(); // Compliant
    }

    public void PassingAsArgument_Removes_Constraints()
    {
        var list = new List<int>();
        Foo(list);
        list.Clear(); // Compliant
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

        list.Clear();   // FIXME Non-compliant - FP, see https://github.com/SonarSource/sonar-dotnet/issues/4261
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
                list.Sort();    // Compliant. This used to be a FP
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
