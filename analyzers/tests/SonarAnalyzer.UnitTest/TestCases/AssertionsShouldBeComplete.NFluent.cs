using System;
using System.Threading.Tasks;
using NFluent;

public class Program
{
    public void CheckThat()
    {
        Check.That(0);     // Noncompliant {{Complete the assertion}}
        //    ^^^^
        Check.That<int>(); // Noncompliant

        Check.That(1).IsEqualTo(1);
    }

    public void CheckThatEnum()
    {
        Check.ThatEnum(AttributeTargets.All); // Noncompliant {{Complete the assertion}}
        //    ^^^^^^^^
        Check.ThatEnum<AttributeTargets>(AttributeTargets.All);  // Noncompliant

        Check.ThatEnum(AttributeTargets.All).IsEqualTo(AttributeTargets.All);
    }

    public void CheckThatCode()
    {
        Check.ThatCode(() => { });     // Noncompliant {{Complete the assertion}}
        //    ^^^^^^^^
        Check.ThatCode(() => 1);       // Noncompliant
        Check.ThatCode(CheckThatCode); // Noncompliant

        Check.ThatCode(() => { }).DoesNotThrow();
    }

    public async Task CheckThatAsyncCode()
    {
        Check.ThatAsyncCode(async () => await Task.CompletedTask); // Noncompliant {{Complete the assertion}}
        //    ^^^^^^^^^^^^^
        Check.ThatAsyncCode(async () => await Task.FromResult(1)); // Noncompliant
        Check.ThatAsyncCode(CheckThatAsyncCode);                   // Noncompliant

        Check.ThatAsyncCode(async () => await Task.CompletedTask).DoesNotThrow();
    }

    public async Task CheckThatDynamic(dynamic expando)
    {
        Check.ThatDynamic(1);       // Noncompliant {{Complete the assertion}}
        //    ^^^^^^^^^^^
        Check.ThatDynamic(expando); // Noncompliant

        Check.ThatDynamic(1).IsNotNull();
    }

    public ICheck<int> CheckReturnedByReturn()
    {
        return Check.That(1);
    }

    public ICheck<int> CheckReturnedByExpressionBody() =>
        Check.That(1);

    public void AnonymousInvocation(Func<Action> a) =>
        a()();
}

namespace OtherCheck
{
    public static class Check
    {
        public static void That(int i) { }
    }

    public class Test
    {
        public void CheckThat()
        {
            Check.That(1); // Compliant. Not NFluent
        }
    }
}
