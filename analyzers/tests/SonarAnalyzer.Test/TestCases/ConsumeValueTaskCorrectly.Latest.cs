using System.Collections.Generic;
using System.Threading.Tasks;

// Repro for https://github.com/SonarSource/sonar-dotnet/issues/6779
class Repro_FN_6779
{
    class SomeService
    {
        public ValueTask DoThing1() => ValueTask.CompletedTask;
        public ValueTask DoThing2() => ValueTask.CompletedTask;
    }

    class Consumer
    {
        public async Task ConsumeTasks()
        {
            var service = new SomeService();

            // The reason seems to be that 'GetLeftMostIdentifier' uses 'service' instead of 'service.DoThing1' as the identifier.
            // Invocation -> SimpleMember -> Invocation -> SimpleMember
            var thing1 = service.DoThing1().AsTask(); // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-1847
            var thing2 = service.DoThing2().AsTask();
//                       ^^^^^^^ Secondary
        }
    }
}


// Repro for https://github.com/SonarSource/sonar-dotnet/issues/9661
class Test
{
    static ValueTask<string> MethodAsync(ValueTask<string> task) =>
        task.IsCompletedSuccessfully
            ? ValueTask.FromResult<string>(task.Result) // Noncompliant, FP
            : AnotherMethodAsync(task);

    static async ValueTask<string> AnotherMethodAsync(ValueTask<string> task) =>
        await task;
}

class ReproMissedConsumptions
{
    private ValueTask<int> field1, field2;

    async Task ConfigureAwaitTwice(ValueTask<int> vt)
    {
        await vt.ConfigureAwait(false);                 // FN https://sonarsource.atlassian.net/browse/NET-5305
        await vt.ConfigureAwait(false);
    }

    async Task AwaitThenConfigureAwait(ValueTask<int> vt)
    {
        await vt;                                       // FN https://sonarsource.atlassian.net/browse/NET-5305
        await vt.ConfigureAwait(false);
    }

    int ConfiguredGetResult(ValueTask<int> vt) =>
        vt.ConfigureAwait(false).GetAwaiter().GetResult();  // FN https://sonarsource.atlassian.net/browse/NET-5305

    int NullForgivingLocal(ValueTask<int> vt) => vt!.Result;                    // FN https://sonarsource.atlassian.net/browse/NET-5306
    int NullForgivingField() => this.field1!.Result;                            // FN https://sonarsource.atlassian.net/browse/NET-5306
    int NullForgivingAwaiter() => this.field2!.GetAwaiter().GetResult();        // FN https://sonarsource.atlassian.net/browse/NET-5306
}


namespace CSharp14
{
    public class NullConditionalAssignment
    {
        async Task Method()
        {
            var a = new Tester();
            ValueTask<int> NonCompliant = new ValueTask<int>(42);

            a?.Test = await NonCompliant;   // Noncompliant
            a?.Test = await NonCompliant;   // Secondary

            ValueTask<int> Compliant = new ValueTask<int>(42);
            a?.Test = await Compliant;     // Compliant
        }

        public class Tester
        {
            public int Test;
        }
    }
}

namespace CSharp15
{
    public static class ValueTaskExtensions
    {
        extension(ValueTask<int> task)
        {
            public Task<int> Property
            {
                get
                {
                    var first = task.AsTask();  // Noncompliant
                    return task.AsTask();       // Secondary
                }
            }

            public Task<int> this[int index]
            {
                get
                {
                    var first = task.AsTask();  // Noncompliant https://sonarsource.atlassian.net/browse/NET-4912
                    return task.AsTask();       // Secondary
                }
            }

            public Task<int> ConsumedOnce => task.AsTask();     // Compliant

            public int ResultIfCompleted
            {
                get
                {
                    if (task.IsCompletedSuccessfully)           // Compliant
                    {
                        return task.Result;                     // Compliant
                    }
                    return 0;
                }
            }

            public Task<int> this[string key] => task.AsTask(); // Compliant

            public int this[long index]
            {
                get
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        return task.Result;                     // Compliant
                    }
                    return 0;
                }
            }
        }
    }
}
