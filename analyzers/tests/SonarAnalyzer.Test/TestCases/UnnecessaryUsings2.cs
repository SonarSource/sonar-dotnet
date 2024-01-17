using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Concurrent; // Noncompliant {{Remove this unnecessary 'using'.}}
using System.IO;
using System.IO; // Warning [CS0105]
using static System.Console;
using static System.DateTime; // FN - System.DateTime is not a namespace symbol
using MySysAlias = System;
using MyOtherAlias = System.Collections; // FN - aliases not yet supported
using AppendedNamespaceForConcurrencyTest.MyNamespace1; // Compliant - used inside MyNamspace2 to access Ns1_1
using AppendedNamespaceForConcurrencyTest.MyNamespace3; // Noncompliant {{Remove this unnecessary 'using'.}}
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Collections.ObjectModel;

namespace AppendedNamespaceForConcurrencyTest.MyNamespace0
{
    class Ns0_0 { }
}

namespace AppendedNamespaceForConcurrencyTest.MyNamespace1
{
    class Ns1_0 { }
}

namespace AppendedNamespaceForConcurrencyTest.MyNamespace2
{
    class Ns2_0
    {
        Ns1_0 ns11;
    }
}

namespace AppendedNamespaceForConcurrencyTest.MyNamespace2.Level1
{
    using MyNamespace0;
    using MyNamespace0; // Warning [CS0105]
    using MyNamespace0; // Warning [CS0105]
    using MyNamespace1; // Warning [CS0105]
    using System.Linq; // Noncompliant {{Remove this unnecessary 'using'.}}
    using MyNamespace2.Level1; // Noncompliant {{Remove this unnecessary 'using'.}}
    using MyNamespace2; // Noncompliant {{Remove this unnecessary 'using'.}}

    class Ns2_1
    {
        Ns0_0 ns00;
        Ns2_0 ns20;
        Ns1_0 ns11;
        Ns2_1 ns21;
    }

    namespace Level2
    {
        using MyNamespace1; // Warning [CS0105]
        using System.IO; // Warning [CS0105]

        class Ns2_2
        {
            Ns1_0 ns11;

            void M(IEnumerable<DateTimeStyles> myEnumerable)
            {
                File.ReadAllLines("");
                WriteLine("");
                MySysAlias.Console.WriteLine("");
            }
        }
    }
}

namespace AppendedNamespaceForConcurrencyTest.MyNamespace2.Level1.Level2
{
    using MyNamespace0;
    using MyNamespace2.Level1; // Noncompliant {{Remove this unnecessary 'using'.}}

    class Ns2_3
    {
        Ns0_0 ns00;
        Ns2_1 ns21;
    }
}

namespace AppendedNamespaceForConcurrencyTest.MyNamespace3
{
    class Ns3_0 { }
}

namespace AppendedNamespaceForConcurrencyTest.ReferencesInsideDocumentationTags
{
    using System.ComponentModel;
    using System.Collections.Specialized;

    /// <summary> There is <see cref="Win32Exception"/> or <see cref="ListDictionary"/></summary>
    class ClassWithDoc { }

    class InnerClass
    {
        /// <exception cref="AesManaged"></exception>
        public void MethodWithDoc() { }

        /// <summary>
        ///   <seealso cref="ReadOnlyCollection{T}"/>
        /// </summary>
        public void MethodWithGenericClassDoc() { }

        /// This is just a comment
        public void MethodWithoutXMLDoc() { }
    }
}

namespace AppendedNamespaceForConcurrencyTest.AwaitExtensionHolder
{

    internal static class ExtensionHolder
    {
        public static TaskAwaiter<int> GetAwaiter(this Func<int> function)
        {
            Task<int> task = new Task<int>(function);
            task.Start();
            return task.GetAwaiter();
        }
    }
}

namespace AppendedNamespaceForConcurrencyTest.AwaitExtensionUser
{
    using AwaitExtensionHolder; // Compliant - statement is needed for the custom await usage
    class AwaitUser
    {
        async void AsyncMethodUsingAwaitExtension()
        {
            var result = await new Func<int>(() => 0);
        }
    }
}

namespace AppendedNamespaceForConcurrencyTest.LinqQuery1
{
    using System.Linq; // Compliant - statement is needed for query syntax
    class Usage
    {
        public void DoQuery(List<string> myList)
        {
            var query = from item in myList select item.GetType();
        }
    }
}

namespace AppendedNamespaceForConcurrencyTest.LinqQuery2
{
    using global::System.Linq; // Compliant - statement is needed for query syntax
    class Usage
    {
        public void DoQuery(List<string> myList)
        {
            var query = from item in myList select item.GetType();
        }
    }
}

namespace AppendedNamespaceForConcurrencyTest.LinqQuery3.System.Linq { }

namespace AppendedNamespaceForConcurrencyTest.LinqQuery3
{
    using System.Linq; // Noncompliant - This is 'LinqQuery3.System.Linq' whose import is indeed unnecessary
    using global::System.Linq;
    class Usage
    {
        public void DoQuery(List<string> myList)
        {
            var query = from item in myList select item.GetType();
        }
    }
}

namespace AppendedNamespaceForConcurrencyTest.NoLinqQuery
{
    using System.Linq; // Noncompliant
    class UnusedLinqImport { }
}

// This test is for coverage, ensuring the rule does not crash if for some reason the using directive is not found when
// a QueryExpressionSyntax is in the code
namespace AppendedNamespaceForConcurrencyTest.LinqQueryWithoutUsing
{
    class Usage
    {
        public void DoQuery(List<string> myList)
        {
            var query = from item in myList select item.GetType(); // Error [CS1935] - Could not find an implementation of the query pattern for source type 'List<string>'.
        }
    }
}

namespace AppendedNamespaceForConcurrencyTest.CollectionInitializerExtensions1
{
    public static class ListExtensions
    {
        public static void Add(this List<string> list, string firstName, string lastName)
        {
            list.Add(firstName + " " + lastName);
        }
    }
}

namespace AppendedNamespaceForConcurrencyTest.CollectionInitializerExtensions2
{
    public static class ListExtensions
    {
        public static void Add(this List<string> list, string firstName, int number)
        {
            list.Add(firstName + " nb" + number);
        }
    }
}

namespace AppendedNamespaceForConcurrencyTest.CollectionInitializerExtensions3
{
    public static class ListExtensions
    {
        public static void Add(this List<string> list, int number, string lastName)
        {
            list.Add(number + ": " + lastName);
        }
    }
}

namespace AppendedNamespaceForConcurrencyTest.CollectionInitializerUse
{
    using CollectionInitializerExtensions1;
    using CollectionInitializerExtensions2;
    using CollectionInitializerExtensions3; // Noncompliant - this extension is not used

    internal static class Program
    {
        private static void Main2(string[] args)
        {
            var list1 = new List<string>
            {
                { "John", "Smith" },
                { "John", 2 },
            };

            var list2 = new List<string>
            { };
        }
    }
}
