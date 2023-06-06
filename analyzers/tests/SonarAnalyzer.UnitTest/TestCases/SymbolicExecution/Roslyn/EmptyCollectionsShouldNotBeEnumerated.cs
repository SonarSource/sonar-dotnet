using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Tests.Diagnostics
{
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
//          ~~~~~~~~~~~~
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
            var list = new List<int>();
            list.Clear();               // FIXME Non-compliant
            list.Exists(Predicate);     // FIXME Non-compliant
            list.Find(Predicate);       // FIXME Non-compliant
            list.FindIndex(Predicate);  // FIXME Non-compliant
            list.ForEach(Action);       // FIXME Non-compliant
            list.IndexOf(1);            // FIXME Non-compliant
            list.Remove(1);             // FIXME Non-compliant
            list.RemoveAll(Predicate);  // FIXME Non-compliant
            list.Reverse();             // FIXME Non-compliant
            list.Sort();                // FIXME Non-compliant
            _ = list[5];                // FIXME Non-compliant
//              ~~~~~~~
            list[5] = 5;                // FIXME Non-compliant
//          ~~~~~~~

            var set = new HashSet<int>();
            set.Clear();                // FIXME Non-compliant
            set.Remove(1);              // FIXME Non-compliant

            var queue = new Queue<int>();
            queue.Clear();              // FIXME Non-compliant
            queue.Dequeue();            // FIXME Non-compliant
            queue.Peek();               // FIXME Non-compliant

            var stack = new Stack<int>();
            stack.Clear();              // FIXME Non-compliant
            stack.Pop();                // FIXME Non-compliant
            stack.Peek();               // FIXME Non-compliant

            var obs = new ObservableCollection<int>();
            obs.Clear();                // FIXME Non-compliant
            obs.IndexOf(1);             // FIXME Non-compliant
            obs.Move(1, 0);             // FIXME Non-compliant
            obs.Remove(1);              // FIXME Non-compliant
            _ = obs[5];                 // FIXME Non-compliant
            obs[5] = 5;                 // FIXME Non-compliant

            var array = new int[0];
            array.Clone();              // FIXME Non-compliant
            array.CopyTo(null, 0);      // FIXME Non-compliant
            array.GetValue(5);          // FIXME Non-compliant
            array.Initialize();         // FIXME Non-compliant
            array.SetValue(5, 1);       // FIXME Non-compliant
            _ = array[5];               // FIXME Non-compliant
            array[5] = 5;               // FIXME Non-compliant

            var dict = new Dictionary<int, int>();
            dict.Clear();               // FIXME Non-compliant
            dict.Remove(1);             // FIXME Non-compliant
            _ = dict[5];                // FIXME Non-compliant
        }

        public void Methods_Ignored()
        {
            var list = new List<int>();
            list.GetHashCode();
            list.Equals(items);
            list.GetType();
            list.ToString();

            var set = new HashSet<int>();
            set.GetHashCode();
            set.Equals(items);
            set.GetType();
            set.Contains(5);

            var queue = new Queue<int>();
            queue.GetHashCode();
            queue.Equals(items);
            queue.GetType();
            queue.ToString();

            var stack = new Stack<int>();
            stack.GetHashCode();
            stack.Equals(items);
            stack.GetType();
            stack.ToString();

            var obs = new ObservableCollection<int>();
            obs.GetHashCode();
            obs.Equals(items);
            obs.GetType();
            obs.ToString();
            _ = obs.Count;
            obs.CollectionChanged += (s, e) => throw new NotImplementedException();

            var array = new int[0];
            array.GetLength(0);
            array.GetLongLength(0);
            array.GetLowerBound(0);
            array.GetUpperBound(0);
            array.GetHashCode();
            array.Equals(new object());
            array.GetType();
            array.ToString();
            _ = array.Length;

            var dict = new Dictionary<int, int>();
            dict.GetHashCode();
            dict.Equals(items);
            dict.GetType();
            dict.ContainsKey(5);
            dict.ContainsValue(5);
            int i;
            dict.TryGetValue(5, out i);
            dict[5] = 5;
            (((dict[5]))) = 5;
        }

        public void Methods_Set_NotEmpty()
        {
            var list = new List<int>();
            list.Add(1);
            list.Clear();   // Compliant
            list = new List<int>();
            list.AddRange(items);
            list.Clear();   // Compliant
            list = new List<int>();
            list.Insert(0, 1);
            list.Clear();   // Compliant
            list = new List<int>();
            list.InsertRange(0, items);
            list.Clear();   // Compliant

            var set = new HashSet<int>();
            set.Add(1);
            set.Clear();    // Compliant

            var queue = new Queue<int>();
            queue.Enqueue(1);
            queue.Clear();  // Compliant

            var stack = new Stack<int>();
            stack.Push(1);
            stack.Clear();  // Compliant

            var obs = new ObservableCollection<int>();
            obs.Add(1);
            obs.Clear();    // Compliant
            obs = new ObservableCollection<int>();
            obs.Insert(0, 1);
            obs.Clear();    // Compliant

            var dict = new Dictionary<int, int>();
            dict.Add(1, 1);
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

    public class CSharp8
    {
        public void NullCoalescenceAssignment()
        {
            List<int> list = null;
            list ??= new List<int>();
            list.Clear();   // FIXME Non-compliant
        }

        public bool InsideSwitch(int type)
        {
            var list = new List<int>();

            return type switch
            {
                1 => list.Exists(Predicate),    // FIXME Non-compliant
                _ => false
            };
        }

        public void UsingSwitchResult(bool cond)
        {
            var list = cond switch
            {
                _ => new List<int>()
            };

            list.Clear();   // FIXME Non-compliant
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

        public interface IWithDefaultMembers
        {
            public void Test()
            {
                var list = new List<int>();
                list.Clear();   // FIXME Non-compliant
            }
        }

        public void LocalFunctions()
        {
            void LocalFunction()
            {
                var list = new List<int>();
                list.Clear();   // Compliant - FN: local functions are not supported by the CFG
            }

            static void LocalStaticFunction()
            {
                var list = new List<int>();
                list.Clear();   // Compliant - FN: local functions are not supported by the CFG
            }

            https://github.com/SonarSource/sonar-dotnet/issues/4478
            var list = new List<int>();
            AddInLocalFunction();
            list.Clear();       // FIXME Non-compliant FP

            void AddInLocalFunction()
            {
                list.Add(1);
            }
        }

        public void Ranges(string[] words)
        {
            var someWords = words[1..4];
            someWords.Clone();  // Compliant

            var noWords = words[1..1];
            noWords.Clone();    // Compliant - FN, the collection is empty (https://github.com/SonarSource/sonar-dotnet/issues/2944)
        }

        private static bool Predicate(int i) => true;
    }
}
