using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
public sealed class PerformanceSensitiveAttribute : Attribute
{
    public bool AllowGenericEnumeration { get; set; }
}

namespace Tests.Diagnostics
{
    public class S3267
    {
        public bool WithOutArg(string p1, out string p2)
        {
            p2 = string.Empty;
            return true;
        }

        public bool WithRefArg(string p1, ref string p2) => true;

        public void ForEachWithBlockWithIfSuggestWhere(ICollection<string> collection, Predicate<string> condition, Predicate<int> intCondition)
        {
            var result = new List<string>();

            foreach (var element in collection) // Noncompliant {{Loops should be simplified using the "Where" LINQ method}}
            //                      ^^^^^^^^^^
            {
                if (condition(element))
                //  ^^^^^^^^^^^^^^^^^^ Secondary
                {
                    result.Add(element);
                }
            }

            foreach (var element in collection)
            {
                Foo(1);
                if (condition(element))
                {
                    result.Add(element);
                }
            }

            foreach (var element in collection)
            {
                result.Add(element);
                if (condition(element)) { }
            }

            foreach (var element in collection) // Noncompliant
            {
                if (condition(element))
                //  ^^^^^^^^^^^^^^^^^^ Secondary
                {
                    if (intCondition(element.Length))
                        result.Add(element);
                }
            }

            foreach (var element in collection.Select(e => e.Length).Where(l => l > 0)) // Noncompliant
            {
                if (intCondition(element))
                //  ^^^^^^^^^^^^^^^^^^^^^ Secondary
                {
                }
            }
        }

        public void ForEach_ConditionWithOutParameter_Compliant(ICollection<string> collection)
        {
            var result = new List<string>();

            foreach (var element in collection) // Compliant due to `out var a`
            {
                if (WithOutArg(element, out var a))
                {
                    result.Add(a);
                    result.Add(element);
                }
            }
        }

        public void ForEach_ConditionWithRefParameter_Compliant(ICollection<string> collection, ref string v)
        {
            var result = new List<string>();

            foreach (var element in collection) // Compliant due to `ref v`
            {
                if (WithRefArg(element, ref v))
                {
                    result.Add(element);
                }
            }
        }

        public void ForEach_NoIf_Compliant(ICollection<string> collection)
        {
            var builder = new StringBuilder();
            foreach (var s in collection)
            {
                builder.AppendFormat("\n    {0}", s);
            }
        }

        public void ForEach_PropertySet_Compliant(ICollection<Point> collection)
        {
            foreach (var point in collection) // Compliant - Selecting `X` and setting it's value will not work in this case.
            {
                point.X = point.X + 3;
            }

            foreach (var point in collection) // Compliant - Selecting `X` and setting it's value will not work in this case.
            {
                point.X += point.X + 3;
            }

            foreach (var point in collection) // Compliant - Selecting `X` and setting it's value will not work in this case.
            {
                point.X -= point.X + 3;
            }
        }

        public void ForEach_PropertyGet_Compliant(ICollection<Point> collection)
        {
            var sum = 0;
            foreach (var point in collection) // Noncompliant
            {
                sum = point.X + point.X + 3;
            }
        }

        public void ForEach_IfWithElseBranch_Compliant(ICollection<string> collection, Predicate<string> condition)
        {
            var result = new List<string>();

            foreach (var element in collection) // Compliant due to else branch
            {
                if (condition(element))
                {
                    result.Add(element);
                }
                else
                {
                    result.Add(string.Empty);
                }
            }
        }

        public void ForEachWithIfSuggestWhere(ICollection<string> collection, Predicate<string> condition)
        {
            var result = new List<string>();

            foreach (var element in collection) // Noncompliant {{Loops should be simplified using the "Where" LINQ method}}
            //                      ^^^^^^^^^^
                if (condition(element))
                //  ^^^^^^^^^^^^^^^^^^ Secondary
                {
                    result.Add(element);
                }
        }

        public void ForEachWithPropertyGet_SuggestsSelect(ICollection<string> collection, ICollection<Point> points, int[] values)
        {
            var result = new List<int>();

            foreach (var element in collection) // Noncompliant {{Loop should be simplified by calling Select(element => element.Length))}}
            //                      ^^^^^^^^^^
            {
                var someValue = element.Length;
            }

            foreach (var element in collection) // Compliant: property is used only once and not in an assignment
            {
                Foo(element.Length);
            }

            foreach (var element in collection) // Noncompliant {{Loop should be simplified by calling Select(element => element.Length))}}
            //                      ^^^^^^^^^^
            {
                var someValue = element.Length;
                if (someValue > 0)
                {
                    result.Add(someValue);
                }
            }

            foreach (var element in collection) // Noncompliant
            {
                var someValue = element.Length;
                if (someValue != null)
                {
                    result.Add(someValue);
                }
            }

            foreach (var element in collection) // Compliant: `element` is used
            {
                var someValue = element.Length;
                if (element == "") { }
            }

            foreach (var element in collection) // Compliant: IsNormalized is a method
            {
                var someValue = element.IsNormalized();
            }

            foreach (var element in collection) // Compliant: indexer is used
            {
                var someValue = element[0];
            }

            foreach (var point in points) // Compliant (FN): when multiple properties are used we can suggest a tupple.
            {
                var x = point.X;
                var y = point.Y;
            }

            foreach (var point in points) // Compliant: we ignore method invocations.
            {
                var someValue = point.GetX();
                if (someValue > 0)
                {
                    result.Add(someValue);
                }
            }

            var sum = 0;
            for (int i = 0; i < values.Length; i++) // Compliant (FN): we can suggest `sum = values.Sum();`
                sum += values[i];

            for (int i = 0; i < values.Length; i++) // Compliant (FN): we can suggest `sum = values.Aggregate(sum, (current, t) => current - t);`
                sum -= values[i];

            var min = 0;
            foreach (var t in values)   // Noncompliant
            {
                if (min > t)            // Secondary
                {
                    min = t;
                }
            }

            var max = 0;
            foreach (var t in values)   // Noncompliant
            {
                if (max < t)            // Secondary
                {
                    max = t;
                }
            }

            bool test = false;
            foreach (var t in values)   // Noncompliant
            {
                if(test)                // Secondary
                {
                    test = t == 42;
                }
            }

            foreach (var t in values)   // Noncompliant
            {
                if (!test)              // Secondary
                {
                    test = t == 42;
                }
            }

        }

        public void ForEachWithEarlyExitIsNotCompliant(ICollection<string> collection, Predicate<string> condition)
        {
            var result = new List<string>();

            foreach (var element in collection) // Noncompliant
            {
                if (condition(element))
                //  ^^^^^^^^^^^^^^^^^^ Secondary
                {
                    result.Add(element);
                    return;
                }
            }
        }

        public void ForEach_Compliant(ICollection<string> collection, ICollection<Point> collection2, Predicate<string> condition)
        {
            var result = new List<string>();

            // We could improve the rule suggesting: result.AddRange(collection.Where(x => condition(x)));
            foreach (var element in collection.Where(x => condition(x)))
            {
                result.Add(element);
            }

            // We could improve the rule suggesting: result.AddRange(collection2.Select(x => x.Property).Where(y => y != null));
            foreach (var someValue in collection2.Select(x => x.Property).Where(y => y != null))
            {
                result.Add(someValue);
            }
        }

        public void ForEach_WithLambda(Func<SortedSet<int>> lambda)
        {
            foreach (var x in lambda())  // Noncompliant
                if (x is 0) { }          // Secondary
        }

        public void ForEach_WithMethod(Func<SortedSet<int>> lambda)
        {
            foreach (var x in MethodReturningList())  // Noncompliant
                if (x is 0) { }                       // Secondary
        }

        public void ForEach_WithLocalFunction(Func<SortedSet<int>> lambda)
        {
            foreach (var x in LocalFunctionReturningList())  // Noncompliant
                if (x is 0) { }                              // Secondary

            List<int> LocalFunctionReturningList() => new List<int>();
        }

        public void ForEach_IterableNonEnumerable(DataTable rawSheetData)
        {
            var row = rawSheetData.Rows[0];
            var data = new Dictionary<string, string>();
            foreach (DataColumn column in rawSheetData.Columns) // Compliant, it does not implement IEnumerable<T>
            {
                var datum = Convert.ToString(row[column.ColumnName]);
                data.Add(column.ColumnName, datum);
            }
        }

        public void Print(IEnumerable<Point> points)
        {
            foreach (var point in points) // Noncompliant
            {
                Console.WriteLine(point.X + " " + point.X);
            }
        }

        void IsPattern(List<int> list)
        {
            foreach (var item in list) // Noncompliant
            {
                if (item is 42) // Secondary
                {
                    Console.WriteLine("The meaning of Life.");
                }
            }
        }

        private List<int> MethodReturningList() => new List<int>();

        private void Foo(int s) { }

        public class Point
        {
            public int X { get; set; }
            public int? Y { get; set; }
            public string Property { get; set; }

            public int GetX() => X;
        }
    }

    class Repro_7776
    {
        // https://github.com/SonarSource/sonar-dotnet/issues/7776
        public void ForEach_WithListAndLogicalOperators(List<char> s)
        {
            foreach (var c in s)                    // Noncompliant
                if (c == ' ') { }                   // Secondary
            foreach (var c in s)                    // Noncompliant
                if (c != ' ') { }                   // Secondary
            foreach (var c in s)                    // Noncompliant
                if (c != ' ' || c == ' ' && c != '4') { }       // Secondary
        }
    }

    // https://sonarsource.atlassian.net/browse/NET-1222
    class Repro_1222
    {
        public static T? FirstOrNone<T>(IEnumerable<T> enumerable, Predicate<T> predicate) where T : struct
        {
            // this could be re-written via LINQ but it isn't obvious:
            // enumerable.Cast<T?>().FirstOrDefault(x => predicate(x!.Value));
            foreach (var element in enumerable)
            {
                if (predicate(element))
                {
                    return element;
                }
            }
            return null;
        }

        public static T FirstOrNone<T>(IEnumerable<T?> enumerable, Predicate<T?> predicate) where T : struct
        {
            foreach (var element in enumerable) // Noncompliant {{Loops should be simplified using the "Where" LINQ method}}
            {
                if (predicate(element))         // Secondary
                {
                    return (T)element;
                }
            }
            return default(T);
        }

        private static IEnumerable<T> GetEnumerable<T>() { return null; }
        private static bool Filter<T>(T v) { return true; }

        public static int? LocalFunction()
        {
            int? value = null;
            foreach (var i in GetEnumerable<int>())
            {
                if (Filter(i))
                {
                    value = i;
                    break;
                }
            }
            foreach (var element in GetEnumerable<int>())
            {
                if (Filter(element))
                    return element;
            }
            return null;
            int? MyLocalFunction(IEnumerable<int> enumerable, Predicate<int> predicate)
            {
                foreach (var element in enumerable)
                {
                    if (predicate(element))
                    {
                        {
                            return element;
                        }
                    }
                }
                return null;
            }
        }

        public static int? operator +(Repro_1222 _i1, int _i2)
        {
            foreach (var i in GetEnumerable<int>()) // Compliant
            {
                if (Filter(i))
                {
                    return i;
                }
            }
            return null;
        }

        public int? Integer
        {
            get
            {
                foreach (var i in GetEnumerable<int>()) // Compliant
                {
                    if (Filter(i))
                    {
                        return i;
                    }
                }
                return null;
            }
            set
            {
                value = null;
                foreach (var i in GetEnumerable<int>()) // Compliant
                {
                    if (Filter(i))
                    {
                        value = i;
                        break;
                    }
                }
            }
        }
    }

    class PerformanceSensitive
    {
        private static IEnumerable<T> GetEnumerable<T>() { return null; }
        private static bool Filter<T>(T v) { return true; }

        [PerformanceSensitive]
        public PerformanceSensitive()
        {
            int value = 0;
            foreach (var i in GetEnumerable<int>())
            {
                if (Filter(i))
                {
                    value = i;
                    break;
                }
            }
        }


        [PerformanceSensitive]
        public int Property
        {
            get
            {
                foreach (var i in GetEnumerable<int>())
                {
                    if (Filter(i))
                    {
                        return i;
                    }
                }
                return 0;
            }
            set
            {
                value = 0;
                foreach (var i in GetEnumerable<int>())
                {
                    if (Filter(i))
                    {
                        value = i;
                        break;
                    }
                }
            }
        }

        [PerformanceSensitive]
        public static int Method()
        {
            foreach (var i in GetEnumerable<int>())
            {
                if (Filter(i))
                {
                    return i;
                }
            }
            return 0;
        }

        [PerformanceSensitive(AllowGenericEnumeration = true)]
        public static int AllowGenericEnumerationTrue()
        {
            foreach (var i in GetEnumerable<int>())
            {
                if (Filter(i))
                {
                    return i;
                }
            }
            return 0;
        }

        [PerformanceSensitive(AllowGenericEnumeration = false)]
        public static int AllowGenericEnumerationFalse()
        {
            foreach (var i in GetEnumerable<int>()) // Noncompliant
            {
                if (Filter(i))                      // Secondary
                {
                    return i;
                }
            }
            return 0;
        }

        [Obsolete("Not PerformanceSensitive")]
        public static int AnotherAttribute()
        {
            foreach (var i in GetEnumerable<int>()) // Noncompliant
            {
                if (Filter(i))                      // Secondary
                {
                    return i;
                }
            }
            return 0;
        }
    }

    class NotNullable
    {
        public static T FirstOrNone<T>(IEnumerable<T> enumerable, Predicate<T> predicate) where T : struct
        {
            foreach (var element in enumerable) // Noncompliant
            {
                if (predicate(element))         // Secondary
                {
                    return element;
                }
            }
            return default(T);
        }

        private static IEnumerable<T> GetEnumerable<T>() { return null; }
        private static bool Filter<T>(T v) { return true; }

        public static int LocalFunction()
        {
            int value = 0;
            foreach (var i in GetEnumerable<int>()) // Noncompliant
            {
                if (Filter(i))                      // Secondary
                {
                    value = i;
                    break;
                }
            }
            foreach (var element in GetEnumerable<int>()) // Noncompliant
            {
                if (Filter(element))                      // Secondary
                    return element;
            }
            return 0;
            int MyLocalFunction(IEnumerable<int> enumerable, Predicate<int> predicate)
            {
                foreach (var element in enumerable) // Noncompliant
                {
                    if (predicate(element))         // Secondary
                    {
                        {
                            return element;
                        }
                    }
                }
                return 0;
            }
        }

        public static int operator +(NotNullable _i1, int _i2)
        {
            foreach (var i in GetEnumerable<int>()) // Noncompliant
            {
                if (Filter(i))                      // Secondary
                {
                    return i;
                }
            }
            return 0;
        }

        public int Integer
        {
            get
            {
                foreach (var i in GetEnumerable<int>()) // Noncompliant
                {
                    if (Filter(i))                      // Secondary
                    {
                        return i;
                    }
                }
                return 0;
            }
            set
            {
                value = 0;
                foreach (var i in GetEnumerable<int>()) // Noncompliant
                {
                    if (Filter(i))                      // Secondary
                    {
                        value = i;
                        break;
                    }
                }
            }
        }
    }

    class EmptyReturn
    {
        public static void MyMethod<T>(IEnumerable<T> enumerable, Predicate<T> predicate) where T : struct
        {
            foreach (var element in enumerable) // Noncompliant
            {
                if (predicate(element))         // Secondary
                {
                    return;
                }
            }
        }
    }

    class Lambda
    {
        private Predicate<Object> LambdaMethod(IEnumerable<int> enumerable, Predicate<int> predicate)
        {
            Predicate<Object> lambda = null;
            foreach (var element in enumerable) // Noncompliant
            {
                if (predicate(element))         // Secondary
                {
                    lambda = instance => false;
                    break;
                }
            }

            return lambda;
        }
    }
}
