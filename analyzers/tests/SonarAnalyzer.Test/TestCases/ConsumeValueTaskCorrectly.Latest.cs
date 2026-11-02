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
            var thing1 = service.DoThing1().AsTask(); // Noncompliant FP
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
                    var first = task.AsTask();  // FN https://sonarsource.atlassian.net/browse/NET-4912
                    return task.AsTask();
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
