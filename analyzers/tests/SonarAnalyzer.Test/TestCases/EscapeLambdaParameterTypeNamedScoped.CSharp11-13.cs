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
            var x1 = (scoped a) => { }; // Noncompliant {{'scoped' should be escaped when used as a type name in lambda parameters}}
            //        ^^^^^^

            var x2 = (scoped[] a) => { }; // Noncompliant
            //        ^^^^^^

            var x3 = (scoped<int> a) => { }; // Noncompliant
            //        ^^^^^^

            var x4 = (scoped? a) => { }; // Noncompliant
            //        ^^^^^^

            var x5 = static (scoped a) => { }; // Noncompliant

            var x6 = async (scoped a) => Task.CompletedTask; // Noncompliant

            var x7 = (scoped a, scoped b) => { };
            //        ^^^^^^
            //                  ^^^^^^ @-1

            var x8 = (scoped a) => (scoped b) => { };
            //        ^^^^^^
            //                      ^^^^^^ @-1

            var x9 = (scoped[,] a) => { }; // Noncompliant
            //        ^^^^^^

            var x10 = (scoped[][] a) => { }; // Noncompliant
            //         ^^^^^^

            var x11 = (scoped<int>[] a) => { }; // Noncompliant
            //         ^^^^^^

            var x12 = (scoped<int>? a) => { }; // Noncompliant
            //         ^^^^^^

            var x13 = (scoped?[] a) => { }; // Noncompliant
            //         ^^^^^^

            var x14 = (scoped[]? a) => { }; // Noncompliant
            //         ^^^^^^

            var x15 = (scoped<int>[,] a) => { }; // Noncompliant
            //         ^^^^^^

            var x16 = (scoped<int>[][] a) => { }; // Noncompliant
            //         ^^^^^^

            var x17 = (scoped<int>?[] a) => { }; // Noncompliant
            //         ^^^^^^

            var x18 = (scoped<int>[]? a) => { }; // Noncompliant
            //         ^^^^^^

            Action<int> x19 = (scoped) => { }; // Noncompliant
            //                 ^^^^^^

            Action<int, int> x20 = (a, scoped) => { }; // Noncompliant
            //                         ^^^^^^
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
            Delegate x1 = (scoped scoped s) => { }; // Noncompliant
            //                    ^^^^^^

            DelegateRef x2 = (scoped ref scoped s) => { }; // Noncompliant
            //                           ^^^^^^

            DelegateIn x3 = (scoped in scoped s) => { }; // Noncompliant
            //                         ^^^^^^

            DelegateOut x4 = (scoped out scoped s) => { s = default; }; // Noncompliant
            //                           ^^^^^^

            Delegate x5 = (scoped scoped) => { }; // Noncompliant
            //             ^^^^^^
        }

        unsafe void Pointer()
        {
            var x1 = (@scoped* s) => { };
            var x2 = (scoped* s) => { }; // Noncompliant
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
            var x1 = (scoped a) => { }; // Noncompliant

            var x2 = (scoped[] a) => { }; // Noncompliant

            var x3 = (scoped? a) => { }; // Noncompliant

            var x4 = (scoped[]? a) => { }; // Noncompliant
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
