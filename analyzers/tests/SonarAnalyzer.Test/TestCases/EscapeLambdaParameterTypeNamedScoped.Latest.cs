// The structure of this file must mirror EscapeLambdaParameterTypeNamedScoped.CSharp11-13.cs
// Both files must be kept in sync — add new test cases to both files.
// Annotations differ: C# 14 emits compiler errors for all noncompliant cases instead of S8381 diagnostics.

namespace TypeNamedScoped
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class LambdaParameters
    {
        delegate scoped Delegate();

        void Noncompliant()
        {
            var x1 = (scoped a) => { }; // Error [CS8917]

            var x2 = (scoped[] a) => { }; // Error [CS0119, CS0443, CS0103, CS1002, CS1026, CS1513, CS1002]

            var x3 = (scoped<int> a) => { }; // Error [CS0119, CS0119, CS0103, CS1002, CS1026, CS1002, CS1513]

            var x4 = (scoped ? a) => { }; // Error [CS0119, CS0103, CS1003, CS1525, CS1003, CS1002]

            var x5 = static (scoped a) => { }; // Error [CS8917]

            var x6 = async (scoped a) => Task.CompletedTask; // Error [CS8917]

            var x7 = (scoped a, scoped b) => { }; // Error [CS8917]

            var x8 = (scoped a) => (scoped b) => { }; // Error [CS8917]

            var x9 = (scoped[,] a) => { }; // Error [CS0119, CS0443, CS0443, CS0103, CS1002, CS1026, CS1002, CS1513]

            var x10 = (scoped[][] a) => { }; // Error [CS0119, CS0443, CS0443, CS0103, CS1002, CS1026, CS1002, CS1513]

            var x11 = (scoped<int>[] a) => { }; // Error [CS0119, CS0443, CS0103, CS1002, CS1026, CS1002, CS1513]

            var x12 = (scoped<int> ? a) => { }; // Error [CS0119, CS0103, CS1003, CS1525, CS1003, CS1002]

            var x13 = (scoped?[] a) => { }; // Error [CS0119, CS0443, CS0103, CS1002, CS1026, CS1002, CS1513]

            var x14 = (scoped[]? a) => { }; // Error [CS0119, CS0443, CS0103, CS1003, CS1525, CS1003, CS1002]

            var x15 = (scoped<int>[,] a) => { };   // Error [CS0119, CS0443, CS0443, CS0103, CS1002, CS1026, CS1002, CS1513]

            var x16 = (scoped<int>[][] a) => { }; // Error [CS0119, CS0443, CS0443, CS0103, CS1002, CS1026, CS1002, CS1513]

            var x17 = (scoped<int>?[] a) => { };   // Error [CS0119, CS0443, CS0103, CS1002, CS1026, CS1002, CS1513]

            var x18 = (scoped<int>[]? a) => { }; // Error [CS0119, CS0443, CS0103, CS1003, CS1525, CS1003, CS1002]

            Action<int> x19 = (scoped) => { }; // Error [CS9048, CS1001]

            Action<int, int> x20 = (a, scoped) => { }; // Error [CS9048, CS1001]

        }

        void Compliant()
        {
            var x1 = (@scoped s) => { };

            var x2 = (TypeNamedScoped.scoped s) => { };

            var x3 = (global::TypeNamedScoped.scoped s) => { };

            var x4 = (List<scoped> s) => { };

            var x5 = ((scoped, scoped) s) => { };

            var x6 = (@scoped? a) => { };

            var x7 = static (@scoped a) => { };

            var x8 = async (@scoped a) => Task.CompletedTask;

            var x9 = (int scoped) => { };

            Action<@scoped> x10 = delegate (@scoped s) { };

            var x11 = () => { };

            Action<int> x12 = (@scoped) => { };

            Action<int> x13 = scoped => { };

            Action<int, int> x14 = (a, @scoped) => { };

            var x15 = (@scoped<int> a) => { };

            var x16 = (@scoped<int>? a) => { };
        }
    }

    class @scoped { }

    class @scoped<T> { }
}

namespace ScopedModifier
{
    class LambdaParameters
    {
        delegate void Delegate(scoped scoped scoped);
        delegate void Delegate2(scoped scoped);
        delegate void DelegateRef(scoped ref @scoped s);
        delegate void DelegateIn(scoped in @scoped s);
        delegate void DelegateOut(scoped out @scoped s);

        void Compliant()
        {
            var x1 = (scoped global::ScopedModifier.scoped s) => { };
            var x2 = (scoped ScopedModifier.scoped s) => { };

            Delegate x3 = (scoped @scoped s) => { };

            Delegate x4 = delegate (scoped s) { };
            Delegate x5 = delegate (scoped scoped s) { };

            Delegate2 x6 = delegate (scoped s) { };
            Delegate2 x7 = delegate (scoped scoped s) { };

            DelegateRef x8 = (scoped ref @scoped s) => { };
            DelegateIn x9 = (scoped in @scoped s) => { };
            DelegateOut x10 = (scoped out @scoped s) => { s = default; };
        }

        void Noncompliant()
        {
            Delegate x1 = (scoped scoped s) => { }; // Error [CS1107]

            DelegateRef x2 = (scoped ref scoped s) => { }; // Error [CS1107]

            DelegateIn x3 = (scoped in scoped s) => { }; // Error [CS1107]

            DelegateOut x4 = (scoped out scoped s) => { s = default; }; //Error [CS1107]

            Delegate x5 = (scoped scoped) => { };

        }

        unsafe void Pointer()
        {
            var x1 = (@scoped* s) => { };
            var x2 = (scoped* s) => { }; // Error [CS0119, CS0103, CS1003, CS1002]
        }
    }

    ref struct @scoped { }
    ref struct @scoped<T> { }
}

namespace AliasedScoped
{
    using @scoped = System.Int32;
    using System.Collections.Generic;

    class LambdaParameters
    {
        void Noncompliant()
        {
            var x1 = (scoped a) => { }; // Error [CS8917]

            var x2 = (scoped[] a) => { }; // Error [CS0119, CS0443, CS0103, CS1002, CS1026, CS1002, CS1513]

            var x3 = (scoped? a) => { }; // Error [CS0119, CS0103, CS1003, CS1525, CS1003, CS1002]

            var x4 = (scoped[]? a) => { }; // Error [CS0119, CS0443, CS0103, CS1003, CS1525, CS1003, CS1002]
        }

        void Compliant()
        {
            var x1 = (@scoped s) => { };

            var x2 = (List<scoped> s) => { };

            var x3 = ((scoped, scoped) s) => { };

            var x4 = (@scoped? a) => { };
        }
    }
}
