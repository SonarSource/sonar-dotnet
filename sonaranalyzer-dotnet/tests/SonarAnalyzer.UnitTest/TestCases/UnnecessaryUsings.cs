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
using MyNamespace1; // Compliant - used inside MyNamspace2 to access Ns1_1
using MyNamespace3; // Noncompliant {{Remove this unnecessary 'using'.}}
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Collections.ObjectModel;

[assembly: AssemblyVersion("1.0.0.0")]

namespace MyNamespace0
{
    class Ns0_0 { }
}

namespace MyNamespace1
{
    class Ns1_0 { }
}

namespace MyNamespace2
{
    class Ns2_0
    {
        Ns1_0 ns11;
    }
}

namespace MyNamespace2.Level1
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

namespace MyNamespace2.Level1.Level2
{
    using MyNamespace0;
    using MyNamespace2.Level1; // Noncompliant {{Remove this unnecessary 'using'.}}

    class Ns2_3
    {
        Ns0_0 ns00;
        Ns2_1 ns21;
    }
}

namespace MyNamespace3
{
    class Ns3_0 { }
}

namespace ReferencesInsideDocumentationTags
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

namespace AwaitExtensionHolder
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

namespace AwaitExtensionUser
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

namespace LinqQuery1
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

namespace LinqQuery2
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

namespace LinqQuery3.System.Linq { }

namespace LinqQuery3
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

namespace NoLinqQuery
{
    using System.Linq; // Noncompliant
    class UnusedLinqImport { }
}

namespace System
{
    using Linq; // Compliant - statement is needed for query syntax
    class Usage
    {
        public void DoQuery(List<string> myList)
        {
            var query = from item in myList select item.GetType();
        }
    }
}

// This test is for coverage, ensuring the rule does not crash if for some reason the using directive is not found when
// a QueryExpressionSyntax is in the code
namespace LinqQueryWithoutUsing
{
    class Usage
    {
        public void DoQuery(List<string> myList)
        {
            var query = from item in myList select item.GetType(); // Error [CS1935] - Could not find an implementation of the query pattern for source type 'List<string>'.
        }
    }
}

namespace CollectionInitializerExtensions1
{
    public static class ListExtensions
    {
        public static void Add(this List<string> list, string firstName, string lastName)
        {
            list.Add(firstName + " " + lastName);
        }
    }
}

namespace CollectionInitializerExtensions2
{
    public static class ListExtensions
    {
        public static void Add(this List<string> list, string firstName, int number)
        {
            list.Add(firstName + " nb" + number);
        }
    }
}

namespace CollectionInitializerExtensions3
{
    public static class ListExtensions
    {
        public static void Add(this List<string> list, int number, string lastName)
        {
            list.Add(number + ": " + lastName);
        }
    }
}

namespace CollectionInitializerUse
{
    using CollectionInitializerExtensions1;
    using CollectionInitializerExtensions2;
    using CollectionInitializerExtensions3; // Noncompliant - this extension is not used

    internal static class Program
    {
        private static void Main(string[] args)
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
