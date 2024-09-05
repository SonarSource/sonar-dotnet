using System;
using System.Collections;
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
    private List<int> GetList() => null;
    private HashSet<int> GetSet() => null;
    private Queue<int> GetQueue() => null;
    private Stack<int> GetStack() => null;
    private ObservableCollection<int> GetObservableCollection() => null;
    private int[] GetArray() => null;
    private Dictionary<int, int> GetDictionary() => null;

    public void DefaultConstructor()
    {
        var list = new List<int>();
        list.Clear();                   // Noncompliant {{Remove this call, the collection is known to be empty here.}}
//      ^^^^^^^^^^^^
        var set = new HashSet<int>();
        set.Clear();                    // Noncompliant
        var queue = new Queue<int>();
        queue.Clear();                  // Noncompliant
        var stack = new Stack<int>();
        stack.Clear();                  // Noncompliant
        var obs = new ObservableCollection<int>();
        obs.Clear();                    // Noncompliant
        var array = new int[0];
        array.Clone();                  // Noncompliant
        var dict = new Dictionary<int, int>();
        dict.Clear();                   // Noncompliant
    }

    public void ConstructorWithCapacity()
    {
        var list = new List<int>(5);
        list.Clear();                   // Noncompliant
        var set = new HashSet<int>(5);
        set.Clear();                    // Noncompliant
        var queue = new Queue<int>(5);
        queue.Clear();                  // Noncompliant
        var stack = new Stack<int>(5);
        stack.Clear();                  // Noncompliant
        var dict = new Dictionary<int, int>(5);
        dict.Clear();                   // Noncompliant
    }

    public void ArrayWithCapacity()
    {
        int zero = 0;
        int five = 5;
        const int ZERO = 0;
        const int FIVE = 5;

        var array = new int[5];
        array.Clone();                  // Compliant
        array = new int[2 + 3];
        array.Clone();                  // Compliant
        array = new int[five];
        array.Clone();                  // Compliant
        array = new int[FIVE];
        array.Clone();                  // Compliant
        array = new int[0];
        array.Clone();                  // Noncompliant
        array = new int[2 - 2];
        array.Clone();                  // Noncompliant
        array = new int[zero];
        array.Clone();                  // FN
        array = new int[ZERO];
        array.Clone();                  // Noncompliant
    }

    public void ConstructorWithEnumerable()
    {
        var list = new List<int>(items);
        list.Clear();                   // Compliant
        var set = new HashSet<int>(items);
        set.Clear();                    // Compliant
        set = new HashSet<int>(items, EqualityComparer<int>.Default);
        set.Clear();                    // Compliant
        set = new HashSet<int>(comparer: EqualityComparer<int>.Default, collection: items);
        set.Clear();                    // Compliant
        var queue = new Queue<int>(items);
        queue.Clear();                  // Compliant
        var stack = new Stack<int>(items);
        stack.Clear();                  // Compliant
        var obs = new ObservableCollection<int>(items);
        obs.Clear();                    // Compliant
        var dict = new Dictionary<int, int>(dictionaryItems);
        dict.Clear();                   // Compliant
    }

    public void ConstructorWithEnumerableWithConstraint(bool condition)
    {
        var baseCollection = new List<int>();
        var set = new HashSet<int>(baseCollection);
        set.Clear();                    // Noncompliant

        baseCollection = new List<int>();
        set = new HashSet<int>(baseCollection, EqualityComparer<int>.Default);
        set.Clear();                    // Noncompliant

        baseCollection = new List<int>();
        set = new HashSet<int>(comparer: EqualityComparer<int>.Default, collection: baseCollection);
        set.Clear();                    // Noncompliant

        baseCollection = new List<int>();
        set = new HashSet<int>(condition ? baseCollection : baseCollection);
        set.Clear();                    // Noncompliant

        baseCollection = new List<int>();
        baseCollection.Add(1);
        set = new HashSet<int>(baseCollection);
        set.Clear();                    // Compliant
    }

    public void ConstructorWithEmptyInitializer()
    {
        var list = new List<int> { };
        list.Clear();                   // Noncompliant
        var set = new HashSet<int> { };
        set.Clear();                    // Noncompliant
        var queue = new Queue<int> { };
        queue.Clear();                  // Noncompliant
        var stack = new Stack<int> { };
        stack.Clear();                  // Noncompliant
        var obs = new ObservableCollection<int> { };
        obs.Clear();                    // Noncompliant
        var array = new int[] { };
        array.Clone();                  // Noncompliant
        var dict = new Dictionary<int, int> { };
        dict.Clear();                   // Noncompliant
    }

    public void ConstructorWithInitializer()
    {
        var list = new List<int> { 1, 2, 3 };
        list.Clear();                   // Compliant
        var set = new HashSet<int> { 1, 2, 3 };
        set.Clear();                    // Compliant
        var obs = new ObservableCollection<int> { 1, 2, 3 };
        obs.Clear();                    // Compliant
        var array = new int[] { 1, 2, 3 };
        array.Clone();                  // Compliant
        var dict = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 2,
            [3] = 3,
        };
        dict.Clear();                   // Compliant
    }

    public void Other_Initialization()
    {
        var list = GetList();
        list.Clear();                   // Compliant
        var set = GetSet();
        set.Clear();                    // Compliant
        var queue = GetQueue();
        queue.Clear();                  // Compliant
        var stack = GetStack();
        stack.Clear();                  // Compliant
        var obs = GetObservableCollection();
        obs.Clear();                    // Compliant
        var array = GetArray();
        array.Clone();                  // Compliant
        var dict = GetDictionary();
        dict.Clear();                   // Compliant

        array = Array.Empty<int>();
        array.Clone();                  // FN
        var enumerable = Enumerable.Empty<int>();
        enumerable.GetEnumerator();     // FN
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
        _ = list[1];                    // Compliant, should be part of S6466
        list[1] = 5;                    // Compliant, should be part of S6466

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
        set.UnionWith(items);           // Compliant, also learns NotEmpty

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
        _ = obs[1];                     // Compliant, should be part of S6466
        obs[1] = 5;                     // Compliant, should be part of S6466

        var array = new int[0];
        array.Clone();                  // Noncompliant
        array.CopyTo(null, 1);          // Noncompliant
        array.GetEnumerator();          // Noncompliant
        array.GetLength(1);             // Noncompliant
        array.GetLongLength(1);         // Noncompliant
        array.GetLowerBound(1);         // Noncompliant
        array.GetUpperBound(1);         // Noncompliant
        array.GetValue(1);              // Noncompliant
        array.Initialize();             // Noncompliant
        array.SetValue(5, 1);           // Noncompliant
        _ = array[1];                   // Compliant, should be part of S6466
        array[1] = 5;                   // Compliant, should be part of S6466

        var dict = new Dictionary<int, int>();
        dict.Clear();                   // Noncompliant
        dict.ContainsKey(1);            // Noncompliant
        dict.ContainsValue(5);          // Noncompliant
        dict.GetEnumerator();           // Noncompliant
        dict.Remove(5);                 // Noncompliant
        dict.TryGetValue(1, out i);     // Noncompliant
        _ = dict[1];                    // Compliant, should be part of S6466
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

    public void Methods_Set_NotEmpty(bool condition)
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
        set.UnionWith(items);           // Compliant
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
        dict = new Dictionary<int, int>();
        dict[1] = 5;
        dict.Clear();                   // Compliant
        dict = new Dictionary<int, int>();
        (condition ? dict : null)[1] = 5;
        dict.Clear();                   // Compliant
        dict = new Dictionary<int, int>();
        ((condition ? dict : null) as Dictionary<int, int>)[1] = 5;
        dict.Clear();                   // Compliant
        dict = new Dictionary<int, int>();
        (condition ? ((condition ? dict : null) as Dictionary<int, int>) : null)[1] = 5;
        dict.Clear();                   // Noncompliant FP, engine limitation
        IDictionary<int, int> idict = new Dictionary<int, int>();
        idict[1] = 5;
        idict.Clear();                  // Compliant
    }

    public void Method_Set_Empty(List<int> list, HashSet<int> set, Queue<int> queue, Stack<int> stack, ObservableCollection<int> obs, Dictionary<int, int> dict)
    {
        list.Clear();                   // Compliant
        list.Clear();                   // Noncompliant

        set.Clear();                    // Compliant
        set.Clear();                    // Noncompliant

        queue.Clear();                  // Compliant
        queue.Clear();                  // Noncompliant

        stack.Clear();                  // Compliant
        stack.Clear();                  // Noncompliant

        obs.Clear();                    // Compliant
        obs.Clear();                    // Noncompliant

        dict.Clear();                   // Compliant
        dict.Clear();                   // Noncompliant

        list.Add(5);
        list.RemoveAll(x => true);      // Compliant
        list.Clear();                   // FN
        list.Add(5);
        list.RemoveAll(x => x == 1);    // Compliant
        list.Clear();                   // Compliant

        set.Add(5);
        set.RemoveWhere(x => true);     // Compliant
        set.Clear();                    // FN
        set.Add(5);
        set.RemoveWhere(x => x == 1);   // Compliant
        set.Clear();                    // Compliant

        var empty = new List<int>();
        set.Add(5);
        set.IntersectWith(empty);       // Compliant
        set.Clear();                    // FN
        var notEmpty = new List<int> { 1 };
        set.Add(5);
        set.IntersectWith(notEmpty);    // Compliant
        set.Clear();                    // Compliant
    }
}

class AdvancedTests
{
    public void UntrackedCollections()
    {
        var sb = new StringBuilder();                       // StringBuilder would be good to add, but we do not support it at the moment
        sb.Clear();
        sb.Clear();                                         // Compliant

        var collectionLookALike = new CollectionLookALike();
        collectionLookALike.Clear();
        collectionLookALike.AddMethodWithDifferentName(5);
        collectionLookALike.Clear();                        // Compliant
    }

    public void AssignmentToInterface()
    {
        IList<int> iList = new List<int>();
        iList.Add(5);
        iList.Clear();                                      // Compliant

        ICollection<int> iCollection = new List<int>();
        iCollection.Add(5);
        iCollection.Clear();                                // Compliant

        ISet<int> iSet = new HashSet<int>();
        iSet.Add(5);
        iSet.Clear();                                       // Compliant

        IDictionary<int, int> iDictionary = new Dictionary<int, int>();
        iDictionary.Add(1, 5);
        iDictionary.Clear();                                // Compliant

        IList<int> customIList = new CustomIList();
        customIList.Clear();                                // Compliant, state is unknown
        customIList.Clear();                                // Noncompliant
        customIList.Add(5);
        customIList.Clear();                                // Compliant

        IEnumerable<int> enumerable = new List<int>();
        enumerable.GetEnumerator();                         // Noncompliant
        enumerable = new List<int> { 5 };
        enumerable.GetEnumerator();                         // Compliant

        IReadOnlyList<int> readOnly = new List<int>();
        readOnly.GetEnumerator();                           // Noncompliant
        readOnly = new List<int> { 5 };
        readOnly.GetEnumerator();                           // Compliant
    }

    public void DeriveFromTrackedCollection()
    {
        var custom = new CustomCollection<int>();
        custom.Clear();                                     // Compliant, state is unknown
        custom.Clear();                                     // Noncompliant
        custom.Add(5);
        custom.Clear();                                     // Compliant
    }

    public void UnknownExtensionMethods()
    {
        var list = new List<int>();
        list.CustomExtensionMethod();                       // Compliant
        list.Clear();                                       // Compliant
    }

    public void WellKnownExtensionMethods()
    {
        var list = new List<int>();
        list.All(x => true);                                // FN
        list.Any();                                         // FN
        list.AsEnumerable();
        list.AsQueryable();
        list.AsReadOnly();
        list.Average();                                     // FN
        list.Cast<byte>();                                  // FN
        list.Concat(list);                                  // FN
        list.Contains(5, EqualityComparer<int>.Default);    // FN
        list.Count();                                       // FN
        list.DefaultIfEmpty();                              // FN
        list.Distinct();                                    // FN
        list.Except(list);                                  // FN
        list.First();                                       // FN
        list.FirstOrDefault();                              // FN
        list.GroupBy(x => x);
        list.GroupJoin(list, x => x, x => x, (x, y) => x);  // FN
        list.Intersect(list);                               // FN
        list.Join(list, x => x, x => x, (x, y) => x);       // FN
        list.Last();                                        // FN
        list.LastOrDefault();                               // FN
        list.LongCount();                                   // FN
        list.Max();                                         // FN
        list.Min();                                         // FN
        list.OfType<int>();                                 // FN
        list.OrderBy(x => x);
        list.OrderByDescending(x => x);
        list.Select(x => x);                                // FN
        list.SelectMany(x => new int[5]);                   // FN
        list.SequenceEqual(list);                           // FN
        list.Single();                                      // FN
        list.SingleOrDefault();                             // FN
        list.Skip(1);                                       // FN
        list.SkipWhile(x => true);                          // FN
        list.Sum();                                         // FN
        list.Take(1);                                       // FN
        list.TakeWhile(x => true);                          // FN
        list.ToArray();                                     // FN
        list.ToDictionary(x => x);                          // FN
        list.ToList();                                      // FN
        list.ToLookup(x => x);
        list.Union(list);                                   // FN
        list.Where(x => true);                              // FN
        list.Zip(list, (x, y) => x);                        // FN
        Enumerable.Reverse(list);                           // FN
        list.Clear();                                       // FN, should raise, because the methods above should not reset the state
    }

    public void PassingAsArgument_Removes_Constraints(bool condition)
    {
        var list = new List<int>();
        Foo(list);
        list.Clear();                                       // Compliant

        list = new List<int>();
        Foo(condition ? list : null);
        list.Clear();                                       // Compliant

        list = new List<int>();
        Foo((condition ? list : null) as List<int>);
        list.Clear();                                       // Compliant
    }

    public void HigherRank_And_Jagged_Array()
    {
        var array1 = new int[0, 0];
        array1.Clone(); // Noncompliant
        var array2 = new int[0, 4];
        array2.Clone(); // Noncompliant
        var array3 = new int[5, 4];
        array3.Clone(); // Compliant
        int[][] array4 = new int[0][];
        array4.Clone(); // Noncompliant
        int[][] array5 = new int[1][];
        array5.Clone(); // Compliant
    }

    public void LearnConditions_Size(bool condition, List<int> arg)
    {
        List<int> isNull = null;
        var empty = new List<int>();
        var notEmpty = new List<int>() { 1, 2, 3 };

        // the tests below are messy for as long as we unlearn CollectionConstraints on empty.Count()

        if ((isNull ?? empty).Count == 0)
        {
            empty.Clear();  // Noncompliant
        }

        if ((isNull ?? notEmpty).Count == 0)
        {
            empty.Clear();  // Compliant, unreachable
        }

        if ((condition ? empty : empty).Count == 0)
        {
            empty.Clear();  // Noncompliant
        }
        else
        {
            empty.Clear();  // Compliant, unreachable
        }

        if (arg.Count < 0)
        {
            empty.Clear();  // Compliant, unreachable
        }
        else
        {
            empty.Clear();  // Noncompliant
        }

        if (empty.Count == 0)
        {
            empty.Clear();  // Noncompliant
        }
        else
        {
            empty.Clear();  // Compliant, unreachable
        }

        if (empty.Count() == 0)
        {
            empty.Clear();  // Noncompliant
        }
        else
        {
            empty.Clear();  // Compliant, unreachable
        }

        if (empty.Count(x => condition) == 0)
        {
            empty.Clear();  // Noncompliant
        }
        else
        {
            empty.Clear();  // Compliant, unreachable
        }

        if (notEmpty.Count(x => condition) == 0)
        {
            empty.Clear();  // Noncompliant
        }
        else
        {
            empty.Clear();  // Noncompliant
        }

        if (Enumerable.Count(empty) == 0)
        {
            empty.Clear();  // Noncompliant
        }
        else
        {
            empty.Clear();  // Compliant, unreachable
        }

        if (((condition ? empty : empty) as IList<int>).Count != 0)
        {
            empty.Clear();  // Compliant, unreachable
        }

        notEmpty.Clear();   // Compliant, prevents LVA from throwing notEmpty away during reference capture
    }

    public void LearnConditions_Size_Array(bool condition)
    {
        int[] isNull = null;
        var empty = new int[0];
        var notEmpty = new[] { 1, 2, 3 };

        if ((condition ? empty : empty).Length == 0)
        {
            empty.Clone();  // Noncompliant
        }
        else
        {
            empty.Clone();  // Compliant, unreachable
        }

        if (empty.Length == 0)
        {
            empty.Clone();  // Noncompliant
        }
        else
        {
            empty.Clone();  // Compliant, unreachable
        }

        if (empty.Count() == 0)
        {
            empty.Clone();  // Noncompliant
        }
        else
        {
            empty.Clone();  // Compliant, unreachable
        }

        notEmpty.Clone();   // Compliant, prevents LVA from throwing notEmpty away during reference capture
    }

    public void Dynamics(dynamic item)
    {
        var list = new List<int>();
        list.Add(item);
        list.Clear();       // Noncompliant FP
    }

    public void Reassignment()
    {
        var list = new List<int>();
        var other = list;
        other.Clear();      // Noncompliant

        other.Add(5);
        list.Clear();       // Noncompliant FP
    }

    public void AddKnownCollection()
    {
        // Repro for https://sonarsource.atlassian.net/browse/NET-210
        var empty = new List<int>();
        var notEmpty = new List<int> { 1, 2, 3 };

        var list = new List<int>();
        list.AddRange(empty);
        list.Clear();                       // FN
        list.AddRange(notEmpty);
        list.Clear();                       // Compliant

        list.InsertRange(0, empty);
        list.Clear();                       // FN
        list.InsertRange(0, notEmpty);
        list.Clear();                       // Compliant

        var set = new HashSet<int>();
        set.UnionWith(empty);
        set.Clear();                        // FN
        set.UnionWith(notEmpty);
        set.Clear();                        // Compliant

        set.SymmetricExceptWith(empty);     // Noncompliant
        set.Clear();                        // FN
        set.SymmetricExceptWith(notEmpty);  // Noncompliant
        set.Clear();                        // Compliant
    }

    void Foo(List<int> items) { }

    class CollectionLookALike
    {
        public void Clear() { }
        public void AddMethodWithDifferentName(int item) { }
    }

    class CustomCollection<T> : Collection<T> { }

    class CustomIList : IList<int>
    {
        public int this[int index] { get => 0; set => Add(value); }
        public int Count => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();
        public void Add(int item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(int item) => throw new NotImplementedException();
        public void CopyTo(int[] array, int arrayIndex) => throw new NotImplementedException();
        public IEnumerator<int> GetEnumerator() => throw new NotImplementedException();
        public int IndexOf(int item) => throw new NotImplementedException();
        public void Insert(int index, int item) => throw new NotImplementedException();
        public bool Remove(int item) => throw new NotImplementedException();
        public void RemoveAt(int index) => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}

static class CustomExtensions
{
    public static void CustomExtensionMethod(this List<int> list) { }
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

    // https://github.com/SonarSource/sonar-dotnet/issues/4261
    public void AddPassedAsParameter()
    {
        var list = new List<int>();
        DoSomething(list.Add);
        list.Clear();   // Compliant

        list = new List<int>();
        DoSomething(list.Clear); // We don't raise here to avoid FPs

        DoSomething(StaticMethodWithoutInstance);

        list = new List<int>();
        DoSomething(x => list.Add(x));
        list.Clear();    // Noncompliant FP, we don't analyze sub CFGs for lambdas

        list = new List<int>();
        Action<int> add = list.Add;
        list.Clear();    // FN
        add(5);
        list.Clear();    // Noncompliant FP

        list = new List<int>();
        Action clear = list.Clear;  // We don't raise here to avoid FPs
        clear();                    // FN
        clear();                    // FN

        list = new List<int> { 42 };
        clear = list.Clear;         // Compliant
        clear();                    // Compliant
        add(5);                     // Adds to another instance, not the current list
        clear();                    // FN
    }

    public void AddPassedAsParameter_Dictionary()   // Reproducer from Peach
    {
        var d = new Dictionary<int, int>();
        AddSomething(
            d.ContainsKey,  // Compliant
            (k, v) => d[k] = v);

        void AddSomething(Func<int, bool> containsKey, Action<int, int> add)
        { }
    }

    private static void StaticMethodWithoutInstance() { }

    public void Count()
    {
        var list = GetList();
        if (list.Count == 0)
            list.Clear();       // Noncompliant
        else
            list.Clear();       // Compliant

        list = GetList();
        if (list.Count() == 0)
            list.Clear();       // FN
        else
            list.Clear();       // Compliant

        list = GetList();
        if (list.Count == 5)
            list.Clear();       // Compliant
        else
            list.Clear();       // Compliant

        list = GetList();
        if (list.Count() == 5)
            list.Clear();       // Compliant
        else
            list.Clear();       // Compliant

        list = GetList();
        if (list.Count != 0)
            list.Clear();       // Compliant
        else
            list.Clear();       // Noncompliant

        list = GetList();
        if (list.Count() != 0)
            list.Clear();       // Compliant
        else
            list.Clear();       // FN

        list = GetList();
        if (list.Count > 0)
            list.Clear();       // Compliant
        else
            list.Clear();       // Noncompliant

        list = GetList();
        if (list.Count() > 0)
            list.Clear();       // Compliant
        else
            list.Clear();       // FN

        list = GetList();
        if (list.Count > 1)
            list.Clear();       // Compliant
        else
            list.Clear();       // Compliant

        list = GetList();
        if (list.Count() > 1)
            list.Clear();       // Compliant
        else
            list.Clear();       // Compliant

        var array = GetArray();
        if (array.Length == 0)
            array.Clone();      // Noncompliant
        else
            array.Clone();      // Compliant
    }

    private static void DoSomething(Action<int> callback) { }
    private static void DoSomething(Action callback) { }
    private List<int> GetList() => null;
    private int[] GetArray() => null;
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

namespace Extensions
{
    class ExtensionMethods
    {
        public void Test()
        {
            var list = new List<int>();
            list.Clear(3); // Noncompliant
        }
    }

    public static class Extensions
    {
        public static void Clear(this List<int> list, int i) { }
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
                list.Sort();    // Compliant
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
    public void Main()
    {
        var list = new List<String>();
        AddInLocalFunction();
        list.Clear();   // Noncompliant FP

        void AddInLocalFunction()
        {
            list.Add("Item1");
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/2147
public class Repro_2147
{
    private List<string> _lines;

    public virtual object Clone()
    {
        var clonedEntry = (Repro_2147)MemberwiseClone();
        clonedEntry._lines = new List<string>();
        _lines.ForEach(x => { });   // Compliant, different field
        return clonedEntry;
    }
}

public class ReproAD0001
{
    List<string> list;
    public static int Count => 0;

    public void ThrowingAD0001()
    {
        var c = ReproAD0001.Count; // Reproducer to avoid AD0001
        list.Clear(); // This forces ShouldExecute to become true
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/7582
public class Repro_7582
{
    private static void Demo(IEnumerable<int> input)
    {
        var list = new List<int>();
        var flag = false;

        foreach (var item in input)
        {
            if (item > 10)
            {
                flag = true;
                continue;
            }

            list.Add(item);
        }

        if (flag)
        {
            foreach (var item in list)  // Compliant
            {
                // do something with items that were <= 10
                // only when items > 10 were found.
            }
        }
    }
}

// https://community.sonarsource.com/t/c-s4158-false-positive/97192
public class ReproCommunity_97192
{
    public class SomeObject
    {
        public int Value { get; set; }
    }

    private static void Method()
    {
        var myList = new List<SomeObject>();

        foreach(var other in Enumerable.Range(0,100))
        {
            myList.Add(new SomeObject { Value = other});
        }

        myList.ForEach(i => Console.WriteLine(i)); // Compliant - Was a FP on 9.7.0.75501
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8041
static class Repro_8041
{
    static void Func<T>(this ICollection<T> toDo) where T : class, IFoo<T>
    {
        Queue<T> toDoNow = new Queue<T>();
        HashSet<T> toDoLater = new HashSet<T>();

        foreach (T item in toDo)
        {
            if (item.someProperty)
            {
                toDoNow.Enqueue(item);
            }
            else
            {
                _ = toDoLater.Add(item);
            }
        }

        while (toDoNow.Count > 0)
        {
            T current = toDoNow.Dequeue();

            // Handle current

            foreach (T item in current.Successors)
            {
                _ = toDoLater.Remove(item);     // Compliant
                toDoNow.Enqueue(item);
            }
        }
    }

    interface IFoo<T>
    {
        bool someProperty { get; }

        IEnumerable<T> Successors { get; }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9425
class Repro9425
{
    int Id { get; set; }
    bool Flag { get; set; }

    void Test(List<Repro9425> aList)
    {
        List<int> collectionOfIds = new List<int>();
        aList.FindAll(x => x.Flag).ForEach(x => collectionOfIds.Add(x.Id));
        collectionOfIds.Clear();  // Noncompliant FP
    }
}

//https://github.com/SonarSource/sonar-dotnet/issues/9444
class Repro9444
{
    List<int> Numbers { get; set; }

    void Initialize()
    {
        Numbers = new List<int>();
        InitializeNumbers();
        foreach (var number in Numbers) // Noncompliant FP
        {
            System.Console.WriteLine(number);
        }
    }

    void InitializeNumbers()
    {
        Numbers.Add(1);
        Numbers.Add(2);
        Numbers.Add(3);
    }
}
