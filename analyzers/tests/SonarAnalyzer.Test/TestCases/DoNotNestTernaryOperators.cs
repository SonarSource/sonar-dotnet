using System;
using System.Linq;
using System.Linq.Expressions;

namespace Tests.Diagnostics
{
    class Program
    {
        public bool Foo()
        {
            var b = System.Environment.NewLine?.ToString();
            return true ? FooImpl(true, false) : true;
        }

        public bool FooImpl(bool isMale, bool isMarried)
        {
            var x = isMale ? "Mr. " : isMarried ? "Mrs. " : "Miss ";  // Noncompliant {{Extract this nested ternary operation into an independent statement.}}
//                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            var x2 = isMale ? "Mr. " : isMarried ? "Mrs. " : true ? "Miss " : "what? ";
//                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//                                                           ^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant@-1

            var x3 = isMale ? "Mr. " :
                 isMarried // Noncompliant
                ? "Mrs. "
                : "Miss ";

            var lambda1 = true // Compliant. Nested in Lambda is valid
                ? new Func<string>(() => true ? "a" : "b")
                : new Func<string>(() => true ? "c" : "d");

            var lambda2 = true
                ? new Func<string, string>(s => true ? "a" : "b")
                : new Func<string, string>(s => true ? "c" : "d");

            var lambda3 = new Func<string, string>(s => isMale ? "Mr. " : isMarried ? "Mrs. " : "Miss "); // Noncompliant

            return false;
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/9801
    // Nested ternaries inside expression-tree lambdas cannot be extracted to a local because
    // statement-bodied lambdas are not legal in expression trees and the lambda is translated to SQL.
    public class ExpressionTree
    {
        public class Entity
        {
            public bool A { get; set; }
            public bool B { get; set; }
            public bool C { get; set; }
        }

        public void IQueryableSelect(IQueryable<Entity> source) =>
            source.Select(x => x.A ? "a" : x.B ? "b" : x.C ? "c" : null);    // Compliant - expression tree

        public Expression<Func<Entity, string>> DirectExpression =>
            x => x.A ? "a" : x.B ? "b" : x.C ? "c" : null;                   // Compliant - expression tree

        public void LinqQuerySyntax(IQueryable<Entity> source)
        {
            var query = from x in source
                        select x.A ? "a" : x.B ? "b" : x.C ? "c" : null;     // Compliant - expression tree
        }

        // Negative control: a regular delegate lambda with nested ternaries is still flagged.
        public Func<Entity, string> DelegateNotExpression =>
            x => x.A ? "a" : x.B ? "b" : "c";                                // Noncompliant
    }
}
