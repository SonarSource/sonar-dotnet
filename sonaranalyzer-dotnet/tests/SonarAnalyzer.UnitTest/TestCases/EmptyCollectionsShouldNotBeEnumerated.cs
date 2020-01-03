using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Tests.Diagnostics
{
    class ListTests
    {
        private IEnumerable<int> items = new List<int> { 1, 2, 3 };

        private static bool Predicate(int i) => true;
        private static void Action(int i) { }
        private List<int> Factory() => new List<int>();

        public void DefaultConstructor_Applies_Empty()
        {
            var list = new List<int>();
            list.Clear(); // Noncompliant {{Remove this call, the collection is known to be empty here.}}
//          ^^^^^^^^^^^^
        }

        public void ConstructorWithCapacity_Applies_Empty()
        {
            var list = new List<int>(5);
            list.Clear(); // Noncompliant
        }

        public void ConstructorWithEnumerable_Applies_NonEmpty()
        {
            var list = new List<int>(items);
            list.Clear(); // Compliant
        }

        public void ConstructorWithInitializer_Applies_NonEmpty()
        {
            var list = new List<int> { 1, 2, 3 };
            list.Clear(); // Compliant
        }

        public void ConstructorWithEmptyInitializer_Applies_Empty()
        {
            var list = new List<int> { };
            list.Clear(); // Noncompliant
        }

        public void Other_Initialization_Applies_No_Constraints()
        {
            var list = Factory();
            list.Clear(); // Compliant, we don't know anything about the list
        }

        public void Methods_Raise_Issue()
        {
            var list = new List<int>();
            list.Clear(); // Noncompliant
            list.Exists(Predicate); // Noncompliant
            list.Find(Predicate); // Noncompliant
            list.FindIndex(Predicate); // Noncompliant
            list.ForEach(Action); // Noncompliant
            list.IndexOf(1); // Noncompliant
            list.Remove(1); // Noncompliant
            list.RemoveAll(Predicate); // Noncompliant
            list.Reverse(); // Noncompliant
            list.Sort(); // Noncompliant
            var x = list[5]; // Noncompliant
//                  ^^^^^^^
            list[5] = 5; // Noncompliant
//          ^^^^^^^
        }

        public void Methods_Ignored()
        {
            var list = new List<int>();
            list.GetHashCode();
            list.Equals(items);
            list.GetType();
            list.ToString();
        }

        public void ExtensionMethods_Should_Not_Raise()
        {
            var list = new List<int>();
            list.Any(); // Compliant
        }

        public void Methods_Set_NotEmpty()
        {
            var list = new List<int>();
            list.Add(1);
            list.Clear(); // Compliant, will normally raise

            list = new List<int>();
            list.AddRange(items);
            list.Clear(); // Compliant

            list = new List<int>();
            list.Insert(0, 1);
            list.Clear(); // Compliant

            list = new List<int>();
            list.InsertRange(0, items);
            list.Clear(); // Compliant
        }

        public void ExtensionMethods_Remove_Constraints()
        {
            var list = new List<int>();
            list.Any(); // Compliant
            list.Clear(); // Compliant, this will normally raise
        }

        public void PassingToConstructor_Removes_Constraints()
        {
            var list = new List<int>();
            new Foo(1, 2, list); // Compliant
            list.Clear(); // Compliant, this will normally raise
        }
    }

    class QueueTests
    {
        private IEnumerable<int> items = new List<int> { 1, 2, 3 };

        private static bool Predicate(int i) => true;
        private static void Action(int i) { }
        private Queue<int> Factory() => new Queue<int>();

        public void DefaultConstructor_Applies_Empty()
        {
            var list = new Queue<int>();
            list.Clear(); // Noncompliant
        }

        public void ConstructorWithCapacity_Applies_Empty()
        {
            var list = new Queue<int>(5);
            list.Clear(); // Noncompliant
        }

        public void ConstructorWithEnumerable_Applies_NonEmpty()
        {
            var list = new Queue<int>(items);
            list.Clear(); // Compliant
        }

        public void ConstructorWithEmptyInitializer_Applies_Empty()
        {
            var list = new Queue<int> { };
            list.Clear(); // Noncompliant
        }

        public void Other_Initialization_Applies_No_Constraints()
        {
            var list = Factory();
            list.Clear(); // Compliant, we don't know anything about the list
        }

        public void Methods_Raise_Issue()
        {
            var list = new Queue<int>();
            list.Clear(); // Noncompliant
            list.Dequeue(); // Noncompliant
            list.Peek(); // Noncompliant
        }

        public void Methods_Ignored()
        {
            var list = new Queue<int>();
            list.GetHashCode();
            list.Equals(items);
            list.GetType();
            list.ToString();
        }

        public void Methods_Set_NotEmpty()
        {
            var list = new Queue<int>();
            list.Enqueue(1);
            list.Clear(); // Compliant, will normally raise
        }

        public void ExtensionMethods_Remove_Constraints()
        {
            var list = new Queue<int>();
            list.Any(); // Compliant
            list.Clear(); // Compliant, this will normally raise
        }

        public void PassingToConstructor_Removes_Constraints()
        {
            var list = new Queue<int>();
            new Foo(1, 2, list); // Compliant
            list.Clear(); // Compliant, this will normally raise
        }
    }

    class StackTests
    {
        private IEnumerable<int> items = new List<int> { 1, 2, 3 };

        private static bool Predicate(int i) => true;
        private static void Action(int i) { }
        private Stack<int> Factory() => new Stack<int>();

        public void DefaultConstructor_Applies_Empty()
        {
            var list = new Stack<int>();
            list.Clear(); // Noncompliant
        }

        public void ConstructorWithCapacity_Applies_Empty()
        {
            var list = new Stack<int>(5);
            list.Clear(); // Noncompliant
        }

        public void ConstructorWithEnumerable_Applies_NonEmpty()
        {
            var list = new Stack<int>(items);
            list.Clear(); // Compliant
        }

        public void ConstructorWithEmptyInitializer_Applies_Empty()
        {
            var list = new Stack<int> { };
            list.Clear(); // Noncompliant
        }

        public void Other_Initialization_Applies_No_Constraints()
        {
            var list = Factory();
            list.Clear(); // Compliant, we don't know anything about the list
        }

        public void Methods_Raise_Issue()
        {
            var list = new Stack<int>();
            list.Clear(); // Noncompliant
            list.Pop(); // Noncompliant
            list.Peek(); // Noncompliant
        }

        public void Methods_Ignored()
        {
            var list = new Stack<int>();
            list.GetHashCode();
            list.Equals(items);
            list.GetType();
            list.ToString();
        }

        public void Methods_Set_NotEmpty()
        {
            var list = new Stack<int>();
            list.Push(1);
            list.Clear(); // Compliant, will normally raise
        }

        public void ExtensionMethods_Remove_Constraints()
        {
            var list = new Stack<int>();
            list.Any(); // Compliant
            list.Clear(); // Compliant, this will normally raise
        }

        public void PassingToConstructor_Removes_Constraints()
        {
            var list = new Stack<int>();
            new Foo(1, 2, list); // Compliant
            list.Clear(); // Compliant, this will normally raise
        }
    }

    class ObservableCollectionTests
    {
        private IEnumerable<int> items = new ObservableCollection<int> { 1, 2, 3 };

        private static bool Predicate(int i) => true;
        private static void Action(int i) { }
        private ObservableCollection<int> Factory() => new ObservableCollection<int>();

        public void DefaultConstructor_Applies_Empty()
        {
            var list = new ObservableCollection<int>();
            list.Clear(); // Noncompliant
        }

        public void ConstructorWithEnumerable_Applies_NonEmpty()
        {
            var list = new ObservableCollection<int>(items);
            list.Clear(); // Compliant
        }

        public void ConstructorWithInitializer_Applies_NonEmpty()
        {
            var list = new ObservableCollection<int> { 1, 2, 3 };
            list.Clear(); // Compliant
        }

        public void ConstructorWithEmptyInitializer_Applies_Empty()
        {
            var list = new ObservableCollection<int> { };
            list.Clear(); // Noncompliant
        }

        public void Other_Initialization_Applies_No_Constraints()
        {
            var list = Factory();
            list.Clear(); // Compliant, we don't know anything about the list
        }

        public void Methods_Raise_Issue()
        {
            var list = new ObservableCollection<int>();
            list.Clear(); // Noncompliant
            list.IndexOf(1); // Noncompliant
            list.Move(1, 0); // Noncompliant
            list.Remove(1); // Noncompliant
            var x = list[5]; // Noncompliant
            list[5] = 5; // Noncompliant
        }

        public void Methods_Ignored()
        {
            var list = new ObservableCollection<int>();
            list.GetHashCode();
            list.Equals(items);
            list.GetType();
            list.ToString();
            var a = list.Count;
            list.CollectionChanged += List_CollectionChanged;
        }

        private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Methods_Set_NotEmpty()
        {
            var list = new ObservableCollection<int>();
            list.Add(1);
            list.Clear(); // Compliant, will normally raise

            list = new ObservableCollection<int>();
            list.Insert(0, 1);
            list.Clear(); // Compliant
        }

        public void ExtensionMethods_Remove_Constraints()
        {
            var list = new ObservableCollection<int>();
            list.Any(); // Compliant
            list.Clear(); // Compliant, this will normally raise
        }

        public void PassingToConstructor_Removes_Constraints()
        {
            var list = new ObservableCollection<int>();
            new Foo(1, 2, list); // Compliant
            list.Clear(); // Compliant, this will normally raise
        }
    }

    class ArrayTests
    {
        public void Zero_Length_Applies_Empty()
        {
            var list = new int[0];
            list.Clone(); // Noncompliant
        }

        public void NonZero_Length_Applies_NonEmpty()
        {
            var list = new int[5];
            list.Clone(); // Compliant
        }

        public void Initializer_Applies_NonEmpty()
        {
            var list = new int[] { 1, 2, 3 };
            list.Clone(); // Compliant
        }

        public void EmptyInitializer_Applies_Empty()
        {
            var list = new int[] { };
            list.Clone(); // Noncompliant
        }

        public void HigherRank_And_Jagged_Array_Is_NotEmpty()
        {
            var list1 = new int[0, 0];
            list1.Clone(); // Compliant
            int[][] list2 = new int[1][];
            list2.Clone(); // Compliant
        }

        public void Methods_Raise_Issue()
        {
            var other = new int[] { };
            var list = new int[] { };
            list.Clone(); // Noncompliant
            list.CopyTo(other, 0); // Noncompliant
            list.GetValue(5); // Noncompliant
            list.Initialize(); // Noncompliant
            list.SetValue(5, 1); // Noncompliant
            var x = list[5]; // Noncompliant
            list[5] = 5; // Noncompliant
        }

        public void Methods_Ignored()
        {
            var list = new int[] { };
            list.GetLength(0);
            list.GetLongLength(0);
            list.GetLowerBound(0);
            list.GetUpperBound(0);
            list.GetHashCode();
            list.Equals(new object());
            list.GetType();
            list.ToString();
            var a =list.Length;
        }

        public void ExtensionMethods_Remove_Constraints()
        {
            var list = new int[] { };
            list.Any(); // Compliant
            list.Clone(); // Compliant, this will normally raise
        }

        public void PassingToConstructor_Removes_Constraints()
        {
            var list = new int[] { };
            new Foo(1, 2, list); // Compliant
            list.Clone(); // Compliant, this will normally raise
        }
    }

    class HashSetTests
    {
        private IEnumerable<int> items = new HashSet<int> { 1, 2, 3 };

        private static bool Predicate(int i) => true;
        private HashSet<int> Factory() => new HashSet<int>();

        public void DefaultConstructor_Applies_Empty()
        {
            var set = new HashSet<int>();
            set.Clear(); // Noncompliant
        }

        public void ConstructorWithEnumerable_Applies_NonEmpty()
        {
            var set = new HashSet<int>(items);
            set.Clear(); // Compliant
        }

        public void ConstructorWithInitializer_Applies_NonEmpty()
        {
            var set = new HashSet<int> { 1, 2, 3 };
            set.Clear(); // Compliant
        }

        public void ConstructorWithEmptyInitializer_Applies_Empty()
        {
            var set = new HashSet<int> { };
            set.Clear(); // Noncompliant
        }

        public void Other_Initialization_Applies_No_Constraints()
        {
            var set = Factory();
            set.Clear(); // Compliant, we don't know anything about the set
        }

        public void Methods_Raise_Issue()
        {
            var set = new HashSet<int>();
            set.Clear(); // Noncompliant
            set.Remove(1); // Noncompliant
        }

        public void Methods_Ignored()
        {
            var set = new HashSet<int>();
            set.GetHashCode();
            set.Equals(items);
            set.GetType();
            set.Contains(5);
        }

        public void ExtensionMethods_Should_Not_Raise()
        {
            var set = new HashSet<int>();
            set.Any(); // Compliant
        }

        public void ExtensionMethods_Remove_Constraints()
        {
            var set = new HashSet<int>();
            set.Any(); // Compliant
            set.Clear(); // Compliant, this will normally raise
        }

        public void PassingToConstructor_Removes_Constraints()
        {
            var set = new HashSet<int>();
            new Foo(1, 2, set); // Compliant
            set.Clear(); // Compliant, this will normally raise
        }

        public void Methods_Set_NotEmpty()
        {
            var set = new HashSet<int>();
            set.Add(1);
            set.Clear(); // Compliant, will normally raise
        }
    }

    class DictionaryTests
    {
        private IDictionary<int, int> items = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 2,
            [3] = 3,
        };

        private static bool Predicate(int i) => true;
        private Dictionary<int, int> Factory() => new Dictionary<int, int>();

        public void DefaultConstructor_Applies_Empty()
        {
            var dictionary = new Dictionary<int, int>();
            dictionary.Clear(); // Noncompliant
        }

        public void ConstructorWithCapacity_Applies_Empty()
        {
            var dictionary = new Dictionary<int, int>(5);
            dictionary.Clear(); // Noncompliant
        }

        public void ConstructorWithEnumerable_Applies_NonEmpty()
        {
            var dictionary = new Dictionary<int, int>(items);
            dictionary.Clear(); // Compliant
        }

        public void ConstructorWithInitializer_Applies_NonEmpty()
        {
            var dictionary = new Dictionary<int, int>
            {
                [1] = 1,
                [2] = 2,
                [3] = 3,
            };
            dictionary.Clear(); // Compliant
        }

        public void ConstructorWithEmptyInitializer_Applies_Empty()
        {
            var dictionary = new Dictionary<int, int> { };
            dictionary.Clear(); // Noncompliant
        }

        public void Other_Initialization_Applies_No_Constraints()
        {
            var dictionary = Factory();
            dictionary.Clear(); // Compliant, we don't know anything about the dictionary
        }

        public void Methods_Raise_Issue()
        {
            var dictionary = new Dictionary<int, int>();
            dictionary.Clear(); // Noncompliant
            dictionary.Remove(1); // Noncompliant
            var x = dictionary[5]; // Noncompliant
            dictionary[5] = 5; // Compliant
            (((dictionary[5]))) = 5; // Compliant
        }

        public void Methods_Ignored()
        {
            var dictionary = new Dictionary<int, int>();
            dictionary.GetHashCode();
            dictionary.Equals(items);
            dictionary.GetType();
            dictionary.ContainsKey(5);
            dictionary.ContainsValue(5);
            int vv;
            dictionary.TryGetValue(5, out vv);
        }

        public void ExtensionMethods_Should_Not_Raise()
        {
            var dictionary = new Dictionary<int, int>();
            dictionary.Any(); // Compliant
        }

        public void Methods_Set_NotEmpty()
        {
            var dictionary = new Dictionary<int, int>();
            dictionary.Add(1, 1);
            dictionary.Clear(); // Compliant, will normally raise
        }

        public void TryAdd()
        {
            var dictionary = new NetCoreDictionary<string, object>();
            if (dictionary.TryAdd("foo", new object()))
            {
            }
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
            list.Clear(); // Compliant
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
            list.Clear(); // Compliant
        }

        public void Collection_Passed_As_Argument()
        {
            var list = new List<int>();
            DoSomething(list);
            list.Clear(); // Compliant, DoSomething could be adding items to the list
        }

        public void DoSomething(IEnumerable<int> items) { }
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

    class Foo
    {
        // Important to test not only the first argument
        public Foo(int param, int other, IEnumerable<int> arg)
        {
        }
    }

    // See https://github.com/SonarSource/sonar-csharp/issues/1002
    class Program
    {
        public Program(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        }
    }

    class LargeCfg
    {
        // Large CFG that causes the exploded graph to hit the exploration limit
        // See https://github.com/SonarSource/sonar-csharp/issues/767 (comments!)
        private static void MethodName(object[] table, object[][] aoTable2, ref Dictionary<double, string> dictPorts)
        {
            try
            {
                string sValue = "-1";
                switch (5)
                {
                    case 3: sValue = ""; break;
                    case 4: sValue = ""; break;
                    case 5: sValue = ""; break;
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
                            default: sValue2 = ""; break;
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
                    list.Sort(); // Compliant. This used to be a FP
                }
            }
            catch
            {
                // silently do nothing
            }
        }
    }

    // This class is here to simulate how the Dictionary from .NetCore 2.0+ behaves.
    public class NetCoreDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public bool TryAdd(TKey key, TValue value)
        {
            return true;
        }
    }
}

namespace CSharp8
{
    public class NullCoalescenceAssignment
    {
        public void Test()
        {
            List<int> list = null;
            list ??= new List<int>();
            list.Clear(); // Noncompliant
        }
    }

    public class SwitchExpression
    {
        private static bool Predicate(int i) => true;

        public bool InsideSwitch(int type)
        {
            var list = new List<int>();

            return type switch
            {
                1 => list.Exists(Predicate), // Noncompliant
                _ => false
            };
        }

        public void UsingSwitchResult(bool cond)
        {
            var list = cond switch
            {
                _ => new List<int>()
            };

            list.Clear(); // Noncompliant
        }

        public void UsingSwitchResult_Compliant(bool cond)
        {
            var list = cond switch
            {
                true => new List<int> { 5 },
                _ => new List<int>()
            };

            list.Clear();
        }
    }

    public interface IWithDefaultMembers
    {
        public void Test()
        {
            var list = new List<int>();
            list.Clear(); // Noncompliant
        }
    }

    public class LocalStaticFunctions
    {
        public void Method(object arg)
        {
            void LocalFunction(object o)
            {
                var list = new List<int>();
                list.Clear(); // Compliant - FN: local functions are not supported by the CFG
            }

            static void LocalStaticFunction(object o)
            {
                var list = new List<int>();
                list.Clear(); // Compliant - FN: local functions are not supported by the CFG
            }
        }
    }

    public class Ranges
    {
        public void Method(string[] words)
        {
            var someWords = words[1..4];
            someWords.Clone(); // Compliant

            var noWords = words[1..1];
            noWords.Clone(); // Compliant - FN, the collection is empty (https://github.com/SonarSource/sonar-dotnet/issues/2944)
        }
    }
}

