using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

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

            foreach (var element in collection) // Noncompliant {{Loops should be simplified with "LINQ" expressions}}
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

            foreach (var element in collection) // Noncompliant {{Loops should be simplified with "LINQ" expressions}}
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
            foreach (var t in values) // Compliant `min` var is changing during the loop. We should sugest Linq Min() instead.
            {
                if (min > t)
                {
                    min = t;
                }
            }

            var max = 0;
            foreach (var t in values) // Compliant `min` var is changing during the loop. We should sugest Linq Max() instead.
            {
                if (max < t)
                {
                    max = t;
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

        public void M(DataTable rawSheetData)
        {
            var row = rawSheetData.Rows[0];
            var data = new Dictionary<string, string>();
            foreach (DataColumn column in rawSheetData.Columns) // Noncompliant - FP DataColumnCollection does not implement IEnumerable<T>
                                                                // See https://github.com/SonarSource/sonar-dotnet/issues/4996
            {
                var datum = Convert.ToString(row[column.ColumnName]);
                data.Add(column.ColumnName, datum);
            }
        }

        private void Foo(int s) { }

        public class Point
        {
            public int X { get; set; }
            public int? Y { get; set; }
            public string Property { get; set; }

            public int GetX() => X;
        }
    }
}
