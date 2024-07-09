/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [DataTestMethod]
    [DataRow("Aggregate", "(x, y) => x")]
    [DataRow("Append", "1")]
    [DataRow("Average")]
    [DataRow("Concat", "[]")]
    [DataRow("Count")]
    [DataRow("ElementAt", "1")]
    [DataRow("ElementAtOrDefault", "1")]
    [DataRow("Except", "[]")]
    [DataRow("DefaultIfEmpty")]
    [DataRow("Distinct")]
    [DataRow("First")]
    [DataRow("FirstOrDefault")]
    [DataRow("GroupBy", "x => x")]
    [DataRow("GroupJoin", "[1], x => x, x => x, (x, y) => x")]
    [DataRow("Intersect", "[]")]
    [DataRow("Join", "[1], x => x, x => x, (x, y) => x")]
    [DataRow("Last")]
    [DataRow("LastOrDefault")]
    [DataRow("LongCount")]
    [DataRow("Max")]
    [DataRow("Min")]
    [DataRow("OrderBy", "x => x")]
    [DataRow("OrderByDescending", "x => x")]
    [DataRow("Prepend", "1")]
    [DataRow("Reverse")]
    [DataRow("Select", "x => x")]
    [DataRow("SelectMany", "x => new[] {1}")]
    [DataRow("SequenceEqual", "[]")]
    [DataRow("Single")]
    [DataRow("SingleOrDefault")]
    [DataRow("Skip", "1")]
    [DataRow("SkipWhile", "x => true")]
    [DataRow("Sum")]
    [DataRow("Take", "1")]
    [DataRow("TakeWhile", "x => true")]
    [DataRow("ToArray")]
    [DataRow("ToDictionary", "x => x, x => x")]
    [DataRow("ToList")]
    [DataRow("ToLookup", "x => x, x => x")]
    [DataRow("ToHashSet")]
    [DataRow("Union", "[]")]
    [DataRow("Where", "x => true")]
    [DataRow("Zip", "[1], (x, y) => x")]
    public void Invocation_LinqMethodsNullChecking_SetsNotNull(string method, string args = null)
    {
        var code = $$"""
            using System;
            using System.Linq;
            using System.Collections.Generic;
            public class Sample
            {
                public void Main(IEnumerable<int> collection)
                {
                    Tag("Before1", collection);
                    _ = collection.{{method}}({{args}});                                                 // Call as extension
                    Tag("After1", collection);

                    collection = Untracked();
                    Tag("Before2", collection);
                    _ = Enumerable.{{method}}(collection{{(args is null ? string.Empty : "," + args)}}); // Call as normal static method
                    Tag("After2", collection);
                }

                private IEnumerable<int> Untracked() => [ 1, 2, 3 ];

                private static void Tag(string name, object arg) { }
            }
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before1").Should().BeNull();
        validator.TagValue("After1").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before2").Should().BeNull();
        validator.TagValue("After2").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("Cast<short>()")]
    [DataRow("OfType<short>()")]
    [DataRow("ThenBy(x => x)")]
    [DataRow("ThenByDescending(x => x)")]
    public void Invocation_LinqMethodsWithGenericParamNullChecking_SetsNotNull(string invocation)
    {
        var code = $$"""
            using System;
            using System.Linq;
            using System.Collections.Generic;
            public class Sample
            {
                public void Main(IOrderedEnumerable<int> collection)
                {
                    Tag("Before", collection);
                    _ = collection.{{invocation}};
                    Tag("After", collection);
                }

                private static void Tag(string name, object arg) { }
            }
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before").Should().BeNull();
        validator.TagValue("After").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("AsEnumerable")]
    public void Invocation_LinqMethodNonNullChecking_SetsNoConstraint(string method, string args = null)
    {
        var code = $$"""
            using System;
            using System.Linq;
            using System.Collections.Generic;
            public class Sample
            {
                public void Main(IEnumerable<int> collection)
                {
                    Tag("Before1", collection);
                    _ = collection.{{method}}({{args}});                                                 // Call as extension
                    Tag("After1", collection);

                    collection = Untracked();
                    Tag("Before2", collection);
                    _ = Enumerable.{{method}}(collection{{(args is null ? string.Empty : "," + args)}}); // Call as normal static method
                    Tag("After2", collection);
                }

                private IEnumerable<int> Untracked() => [ 1, 2, 3 ];

                private static void Tag(string name, object arg) { }
            }
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, []).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before1").Should().BeNull();
        validator.TagValue("After1").Should().BeNull();
        validator.TagValue("Before2").Should().BeNull();
        validator.TagValue("After2").Should().BeNull();
    }

    [TestMethod]
    public void Invocation_ExtensionsNonLinq_SetsNoConstraint()
    {
        var code = $$"""
            using System;
            using System.Linq;
            using System.Collections.Generic;
            public class Sample
            {
                public void Main(IEnumerable<int> collection)
                {
                    Tag("Before1", collection);
                    _ = collection.NonLinqExtensionNonNullChecking();
                    Tag("After1", collection);

                    collection = Untracked();
                    Tag("Before2", collection);
                    _ = collection.NonLinqExtensionNullChecking();
                    Tag("After2", collection);
                }

                private IEnumerable<int> Untracked() => [ 1, 2, 3 ];

                private static void Tag(string name, object arg) { }
            }

            public static class MyExtensions
            {
                public static int NonLinqExtensionNonNullChecking(this IEnumerable<int> source) => 42;
                public static int NonLinqExtensionNullChecking(this IEnumerable<int> source)
                {
                    _ = source ?? throw new ArgumentNullException(nameof(source));
                    return 42;
                }
            }
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before1").Should().BeNull();
        validator.TagValue("After1").Should().BeNull();
        validator.TagValue("Before2").Should().BeNull();
        validator.TagValue("After2").Should().BeNull();
    }

    [DataTestMethod]
    [DataRow("Count")]
    [DataRow("ToArray")]
    [DataRow("ToList")]
    public void Invocation_LinqExtension_SetsNotNullOnSource_VB(string method)
    {
        var code = $$"""
            Imports System
            Imports System.Linq
            Imports System.Collections.Generic

            Public Class Sample
                Public Sub Main(collection As IEnumerable(Of Integer))
                    Tag("Before1", collection)
                    Dim i1 = collection.{{method}}()                     ' Call as extension
                    Tag("After1", collection)

                    collection = Untracked()
                    Tag("Before2", collection)
                    Dim i2 = collection.{{method.ToLower()}}()           ' Call as extension case insensitive
                    Tag("After2", collection)

                    collection = Untracked()
                    Tag("Before3", collection)
                    Dim i3 = collection.{{method}}                       ' Call as extension without parentheses
                    Tag("After3", collection)

                    collection = Untracked()
                    Tag("Before4", collection)
                    Dim i4 = Enumerable.{{method}}(collection)           ' Call as normal static method
                    Tag("After4", collection)

                    collection = Untracked()
                    Tag("Before5", collection)
                    Dim i5 = Enumerable.{{method.ToLower()}}(collection) ' Call as normal static method case insensitive
                    Tag("After5", collection)

                    collection = Untracked()
                    Tag("Before6", collection)
                    Dim i6 = collection.NonLinqExtensionNonNullChecking()
                    Tag("After6", collection)

                    collection = Untracked()
                    Tag("Before7", collection)
                    Dim i7 = collection.NonLinqExtensionNullChecking()
                    Tag("After7", collection)
                End Sub

                Private Function Untracked() As IEnumerable(Of Integer)
                    Return New List(Of Integer) From {1, 2, 3}
                End Function

                Private Shared Sub Tag(name As String, arg As Object)
                End Sub
            End Class

            Public Module MyExtensions
                <Runtime.CompilerServices.Extension>
                Public Function NonLinqExtensionNonNullChecking(source As IEnumerable(Of Integer)) As Integer
                    Return 42
                End Function
                <Runtime.CompilerServices.Extension>
                Public Function NonLinqExtensionNullChecking(source As IEnumerable(Of Integer)) As Integer
                    If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
                    Return 42
                End Function
            End Module
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.VisualBasic, []).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before1").Should().BeNull();
        validator.TagValue("After1").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before2").Should().BeNull();
        validator.TagValue("After2").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before3").Should().BeNull();
        validator.TagValue("After3").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before4").Should().BeNull();
        validator.TagValue("After4").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before5").Should().BeNull();
        validator.TagValue("After5").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before6").Should().BeNull();
        validator.TagValue("After6").Should().BeNull();
        validator.TagValue("Before7").Should().BeNull();
        validator.TagValue("After7").Should().BeNull();
    }

    [TestMethod]
    public void Invocation_CountOfList_SetsNotNull_VB()
    {
        var code = $$"""
            Imports System
            Imports System.Linq
            Imports System.Collections.Generic

            Public Class Sample
                Public Sub Main(list As List(Of integer))
                    Tag("Before1", list)
                    Dim i1 = list.Count
                    Tag("After1", list)

                    list = Untracked()
                    Tag("Before2", list)
                    Dim i2 = list.Count()
                    Tag("After2", list)
                End Sub

                Private Function Untracked() As List(Of Integer)
                    Return New List(Of Integer) From {1, 2, 3}
                End Function

                Private Shared Sub Tag(name As String, arg As Object)
                End Sub
            End Class
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.VisualBasic, []).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before1").Should().BeNull();
        validator.TagValue("After1").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Before2").Should().BeNull();
        validator.TagValue("After2").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("Count")]
    [DataRow("AsEnumerable")]
    public void Invocation_EnumerableExtension_SetsNoConstraintOnSource(string method)
    {
        var code = $$"""
            using System.Linq;
            using System.Collections.Generic;
            public class Sample
            {
                public void Main(IEnumerable<int> collection)
                {
                    Tag("Before1", collection);
                    _ = collection.{{method}}();           // Call as extension
                    Tag("After1", collection);

                    collection = Untracked();
                    Tag("Before2", collection);
                    _ = Enumerable.{{method}}(collection); // Call as normal static method
                    Tag("After2", collection);
                }

                private IEnumerable<int> Untracked() => [ 1, 2, 3 ];

                private static void Tag(string name, object arg) { }
            }

            public static class Enumerable
            {
                public static int {{method}}(this IEnumerable<int> source) => 42;
            }
            """;
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, []).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);
        validator.TagValue("Before1").Should().BeNull();
        validator.TagValue("After1").Should().BeNull();
        validator.TagValue("Before2").Should().BeNull();
        validator.TagValue("After2").Should().BeNull();
    }

    [DataTestMethod]
    [DataRow("arg.Append(arg)")]
    [DataRow("arg.AsEnumerable()")]
    [DataRow("arg.AsQueryable()")]
    [DataRow("arg.Cast<string>()")]
    [DataRow("arg.Concat(arg)")]
    [DataRow("arg.DefaultIfEmpty()")]   // Returns collection with a single default item in it in case the the source enumerable is empty
    [DataRow("arg.Distinct()")]
    [DataRow("Enumerable.Empty<string>()")]
    [DataRow("arg.Except(arg)")]
    [DataRow("arg.GroupBy(x => x);")]
    [DataRow("arg.GroupJoin(arg, x => x, x => x, (x, lst) => x);")]
    [DataRow("arg.Intersect(arg);")]
    [DataRow("arg.Join(arg, x => x, x => x, (x, lst) => x);")]
    [DataRow("arg.OfType<string>();")]
    [DataRow("arg.OrderBy(x => x);")]
    [DataRow("arg.OrderByDescending(x => x);")]
    [DataRow("arg.Prepend(null);")]
    [DataRow("Enumerable.Range(42, 42);")]
    [DataRow("Enumerable.Repeat(42, 42);")]
    [DataRow("arg.Reverse();")]
    [DataRow("arg.Select(x => x);")]
    [DataRow("arg.SelectMany(x => new[] { x });")]
    [DataRow("arg.Skip(42);")]
    [DataRow("arg.SkipWhile(x => x == null);")]
    [DataRow("arg.Take(42);")]
    [DataRow("arg.TakeWhile(x => x != null);")]
    [DataRow("arg.OrderBy(x => x).ThenBy(x => x);")]
    [DataRow("arg.OrderBy(x => x).ThenByDescending(x => x);")]
    [DataRow("arg.ToArray();")]
    [DataRow("arg.ToDictionary(x => x);")]
    [DataRow("arg.ToList();")]
    [DataRow("arg.ToLookup(x => x);")]
    [DataRow("arg.Union(arg);")]
    [DataRow("arg.Where(x => x != null);")]
    [DataRow("arg.Zip(arg, (x, y) => x);")]
#if NET
    [DataRow("arg.Chunk(42)")]
    [DataRow("arg.DistinctBy(x => x)")]
    [DataRow("arg.ExceptBy(arg, x => x)")]
    [DataRow("arg.IntersectBy(arg, x => x);")]
    [DataRow("arg.SkipLast(42);")]
    [DataRow("arg.UnionBy(arg, x => x);")]
    [DataRow("arg.TakeLast(42);")]
#endif
    public void Invocation_LinqEnumerableAndQueryable_SetNotNull(string expression)
    {
        var code = $"""
            var value = {expression};
            Tag("Value", value);
            """;
        var enumerableValidator = SETestContext.CreateCS(code, "IEnumerable<object> arg").Validator;
        enumerableValidator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);

        var queryableValidator = SETestContext.CreateCS(code, "IQueryable<object> arg").Validator;
        queryableValidator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("FirstOrDefault();")]
    [DataRow("LastOrDefault();")]
    [DataRow("SingleOrDefault();")]
    public void Invocation_LinqEnumerableAndQueryable_NullOrNotNull(string expression)
    {
        var code = $"""
            var value = arg.{expression};
            Tag("Value", value);
            """;
        var enumerableValidator = SETestContext.CreateCS(code, $"IEnumerable<object> arg").Validator;
        enumerableValidator.TagValues("Value").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));

        var queryableValidator = SETestContext.CreateCS(code, $"IQueryable<object> arg").Validator;
        queryableValidator.TagValues("Value").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]    // Just a few examples to demonstrate that we don't set ObjectContraint for all
    [DataRow("Min()")]
    [DataRow("ElementAtOrDefault(42)")]
    [DataRow("FirstOrDefault()")]
    [DataRow("LastOrDefault()")]
    [DataRow("SingleOrDefault()")]
    public void Invocation_LinqEnumerable_Unknown_Int(string expression)
    {
        var code = $"""
            var value = arg.{expression};
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code, $"IEnumerable<int> arg").Validator;
        validator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("FirstOrDefault()", "object")]
    [DataRow("LastOrDefault()", "object")]
    [DataRow("SingleOrDefault()", "object")]
    [DataRow("FirstOrDefault()", "int?")]
    [DataRow("LastOrDefault()", "int?")]
    [DataRow("SingleOrDefault()", "int?")]
    public void Invocation_ElementOrDefault_CollectionEmpty_NullableType(string expression, string type)
    {
        var code = $"""
            collection.Clear();
            var value = collection.{expression};
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code, $"List<{type}> collection").Validator;
        validator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.Null);
    }

    [DataTestMethod]
    [DataRow("FirstOrDefault()")]
    [DataRow("LastOrDefault()")]
    [DataRow("SingleOrDefault()")]
    public void Invocation_ElementOrDefault_CollectionNotEmpty_ReferenceElementType(string expression)
    {
        var code = $"""
            collection.Add(new object());
            var value = collection.{expression};
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code, $"List<object> collection").Validator;
        validator.TagValue("Value").Should().HaveNoConstraints();
    }

    [DataTestMethod]
    [DataRow("FirstOrDefault()")]
    [DataRow("LastOrDefault()")]
    [DataRow("SingleOrDefault()")]
    public void Invocation_ElementOrDefault_CollectionEmpty_ReferenceElementType(string expression)
    {
        var code = $"""
            collection.Clear();
            var value = collection.{expression};
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code, $"List<object> collection").Validator;
        validator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.Null);
    }

    [DataTestMethod]
    [DataRow("FirstOrDefault()", true)]
    [DataRow("LastOrDefault()", true)]
    [DataRow("SingleOrDefault()", true)]
    [DataRow("FirstOrDefault()", false)]
    [DataRow("LastOrDefault()", false)]
    [DataRow("SingleOrDefault()", false)]
    public void Invocation_ElementOrDefault_ValueElementType(string expression, bool empty)
    {
        var code = $"""
            collection.{(empty ? "Clear()" : "Add(1)")};
            var value = collection.{expression};
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code, $"List<int> collection").Validator;
        validator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("FirstOrDefault(x => x is {})")]
    [DataRow("LastOrDefault(x => x is {})")]
    [DataRow("SingleOrDefault(x => x is {})")]
    public void Invocation_ElementOrDefault_CallWithParameters(string expression)
    {
        var code = $"""
            var value = collection.{expression};
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code, $"List<object> collection").Validator;
        validator.TagValues("Value").Should().HaveCount(2)
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.Null))
            .And.ContainSingle(x => x.HasConstraint(ObjectConstraint.NotNull));
    }

    [DataTestMethod]
    [DataRow("FirstOrDefault()")]
    [DataRow("LastOrDefault()")]
    [DataRow("SingleOrDefault()")]
    public void Invocation_ElementOrDefault_Dictionary(string expression) // Dictionaries always return KeyValuePair<TKey, TValue> which is a value type
    {
        var code = $"""
            var value = dict.{expression};
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code, $"Dictionary<int, object> dict").Validator;
        validator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]    // Just a few examples to demonstrate that we don't set ObjectContraint for all
    [DataRow("First()")]
    [DataRow("ElementAtOrDefault(42);")]
    public void Invocation_LinqEnumerable_Unknown_Object(string expression)
    {
        var code = $"""
            var value = arg.{expression};
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code, $"IEnumerable<object> arg").Validator;
        validator.TagValue("Value").Should().BeNull();
    }

    [TestMethod]
    public void Invocation_ElementExistsCheckMethods_NoParameters_NoConstrains()
    {
        var code = """
            var value = collection.Any();
            Tag("Value");
            """;
        var enumerableValidator = SETestContext.CreateCS(code, $"IEnumerable<object> collection").Validator;
        var collectionSymbol = enumerableValidator.Symbol("collection");
        var valueSymbol = enumerableValidator.Symbol("value");
        enumerableValidator.TagStates("Value").Should().HaveCount(2)
            .And.ContainSingle(x => x[collectionSymbol].HasConstraint(CollectionConstraint.NotEmpty) && x[valueSymbol].HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x[collectionSymbol].HasConstraint(CollectionConstraint.Empty) && x[valueSymbol].HasConstraint(BoolConstraint.False));
    }

    [TestMethod]
    public void Invocation_ElementExistsCheckMethods_NoParameters_CollectionNotEmpty()
    {
        var code = """
            collection.Add(1);
            var value = collection.Any();
            Tag("Value", value);
            """;
        var enumerableValidator = SETestContext.CreateCS(code, $"List<int> collection").Validator;
        enumerableValidator.TagValue("Value").Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("Any(x => x is 1)")]
    [DataRow("Exists(x => x is 1)")]
    [DataRow("Contains(1)")]
    public void Invocation_ElementExistsCheckMethods_Parameters_CollectionNotEmpty(string expression)
    {
        var code = $"""
            collection.Add(1);
            var value = collection.{expression};
            Tag("Value", value);
            """;
        var enumerableValidator = SETestContext.CreateCS(code, $"List<int> collection").Validator;
        enumerableValidator.TagValue("Value").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("Any(x => x is 1)")]
    [DataRow("Exists(x => x is 1)")]
    [DataRow("Contains(1)")]
    public void Invocation_ElementExistsCheckMethods_NoPrexistingConstrains(string expression)
    {
        var code = $"""
            var value = collection.{expression};
            Tag("Value");
            """;
        var enumerableValidator = SETestContext.CreateCS(code, $"List<int> collection").Validator;
        var collectionSymbol = enumerableValidator.Symbol("collection");
        var valueSymbol = enumerableValidator.Symbol("value");
        enumerableValidator.TagStates("Value").Should().HaveCount(2)
            .And.ContainSingle(x => x[collectionSymbol].HasConstraint(CollectionConstraint.NotEmpty) && x[valueSymbol].HasConstraint(BoolConstraint.True))
            .And.ContainSingle(x => x[collectionSymbol].HasConstraint(CollectionConstraint.NotEmpty) && x[valueSymbol].AllConstraints.Any(x => !(x is BoolConstraint)));
    }

    [DataTestMethod]
    [DataRow("Any()")]
    [DataRow("Any(x => x is 1)")]
    [DataRow("Exists(x => x is 1)")]
    [DataRow("Contains(1)")]
    public void Invocation_ElementExistsCheckMethods_CollectionAlreadyHasContraint(string expression)
    {
        var code = $"""
            collection.Clear();
            var value = collection.{expression};
            Tag("Value", value);
            """;
        var enumerableValidator = SETestContext.CreateCS(code, $"List<int> collection").Validator;
        enumerableValidator.TagValue("Value").Should().HaveOnlyConstraints(BoolConstraint.False, ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void Invocation_Linq_VB()
    {
        const string code = """
            Dim Query = From Item In Items Where Item IsNot Nothing
            If Query.Count <> 0 Then
                Dim Value = Query(0)
                Tag("Value", Value)
            End If
            """;
        var validator = SETestContext.CreateVB(code, "Items() As Object").Validator;
        validator.TagValue("Value").Should().BeNull();
    }

    [TestMethod]
    public void Invocation_EnumerableCount_SetsNumberConstraint()
    {
        const string code = """
            var x = list.Count();
            Tag("Before", x);

            list.Clear();
            x = list.Count();
            Tag("Clear", x);

            list.Add(42);
            x = list.Count();
            Tag("Add", x);

            x = list.Count(_ => true);
            Tag("Predicate", x);
            """;
        var validator = SETestContext.CreateCS(code, "List<int> list").Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);

        validator.TagValue("Before").Should().HaveOnlyConstraints(NumberConstraint.From(0, null), ObjectConstraint.NotNull);
        validator.TagValue("Clear").Should().HaveOnlyConstraints(NumberConstraint.From(0), ObjectConstraint.NotNull);
        validator.TagValue("Add").Should().HaveOnlyConstraints(NumberConstraint.From(1, null), ObjectConstraint.NotNull);
        validator.TagValue("Predicate").Should().HaveOnlyConstraints(NumberConstraint.From(0, null), ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void Invocation_CollectionMethods_SetCollectionConstraint()
    {
        const string code = """
            var tag = "before";

            list.Clear();
            tag = "clear";

            list.Add(42);
            tag = "add";

            list.RemoveAt(0);
            tag = "remove";

            list.Add(1, 2); // Extension method
            tag = "addExtension";
            """;
        var validator = SETestContext.CreateCS(code, "List<int> list", new PreserveTestCheck("list")).Validator;
        validator.ValidateContainsOperation(OperationKind.Invocation);

        validator.TagValue("before", "list").Should().HaveNoConstraints();
        Verify("clear", ObjectConstraint.NotNull, CollectionConstraint.Empty);
        Verify("add", ObjectConstraint.NotNull, CollectionConstraint.NotEmpty);
        Verify("remove", ObjectConstraint.NotNull);
        Verify("addExtension", ObjectConstraint.NotNull, CollectionConstraint.NotEmpty);

        void Verify(string state, params SymbolicConstraint[] constraints) =>
            validator.TagValue(state, "list").Should().HaveOnlyConstraints(constraints);
    }

    [TestMethod]
    public void Invocation_CollectionArgument_RemovesCollectionConstraint()
    {
        const string code = """
            var tag = "before";

            list.Add(42);
            tag = "add";

            Use(list);
            tag = "argument";

            void Use(List<int> arg) { }
            """;
        var validator = SETestContext.CreateCS(code, "List<int> list", new PreserveTestCheck("list")).Validator;
        validator.ValidateContainsOperation(OperationKind.Argument);

        validator.TagValue("before", "list").Should().HaveNoConstraints();
        validator.TagValue("add", "list").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, CollectionConstraint.NotEmpty);
        validator.TagValue("argument", "list").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void MethodReference_RemovesCollectionConstraint()
    {
        const string code = """
                var tag = "before";

                list.Clear();
                tag = "clear";

                Invoke(list.Add);           // remove CollectionConstraint, we don't know what Invoke does
                tag = "methodReference1";

                list.Add(42);
                tag = "add";

                Action<int> action = list.RemoveAt;
                tag = "methodReference2";

                void Invoke(Action<int> action) { }
            """;
        var validator = SETestContext.CreateCS(code, "List<int> list", new PreserveTestCheck("list")).Validator;
        validator.ValidateContainsOperation(OperationKindEx.MethodReference);

        validator.TagValue("before", "list").Should().HaveNoConstraints();
        Verify("clear", ObjectConstraint.NotNull, CollectionConstraint.Empty);
        Verify("methodReference1", ObjectConstraint.NotNull);
        Verify("add", ObjectConstraint.NotNull, CollectionConstraint.NotEmpty);
        Verify("methodReference2", ObjectConstraint.NotNull);

        void Verify(string state, params SymbolicConstraint[] constraints) =>
            validator.TagValue(state, "list").Should().HaveOnlyConstraints(constraints);
    }
}
