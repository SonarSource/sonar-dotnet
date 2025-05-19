using System;
using System.IO;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    class ValueTaskProvider
    {
        public ValueTask<int> ReadAsync() => new ValueTask<int>();
        public void Foo(int x) { }

    }

    class Tests
    {
        // see https://devblogs.microsoft.com/dotnet/understanding-the-whys-whats-and-whens-of-valuetask/
        // and https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1?view=netstandard-2.1

        public async void Foo1(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            var once = await valueTask; // Noncompliant {{Refactor this 'ValueTask' usage to consume it only once.}}
//                           ^^^^^^^^^
            var twice = await valueTask;
//                            ^^^^^^^^^ Secondary {{The 'ValueTask' is consumed here again}}
        }

        public void Foo2(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            var once = valueTask.AsTask(); // Noncompliant
            var twice = valueTask.AsTask(); // Secondary {{The 'ValueTask' is consumed here again}}
        }

        public async void Foo3(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            var once = await valueTask; // Noncompliant {{Refactor this 'ValueTask' usage to consume it only once.}}
//                           ^^^^^^^^^
            var twice = valueTask.AsTask();
//                      ^^^^^^^^^ Secondary
        }

        public async void Foo4(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            var once = valueTask.AsTask(); // Noncompliant
            var twice = await valueTask; // Secondary

        }

        public void Foo5(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            var once = valueTask.Result; // Noncompliant {{Refactor this 'ValueTask' usage to consume the result only if the operation has completed successfully.}}
//                     ^^^^^^^^^
        }

        public void Foo6(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            var once = valueTask.GetAwaiter().GetResult(); // Noncompliant
        }

        public void Foo7(ValueTaskProvider stream)
        {
            int bytesRead = 0;
            ValueTask<int> valueTask = stream.ReadAsync();
            if (valueTask.IsCompletedSuccessfully)
            {
                // because they're called after completed successfully, we don't count them
                bytesRead = valueTask.Result;
            }

            if (bytesRead > 9)
            {
                bytesRead = valueTask.Result; // FN - it is checked above and we don't use the CFG to verify the blocks
            }
        }

        public void Foo8(ValueTask<object> valueTask)
        {
            if (valueTask.IsCompletedSuccessfully)
            {
                // because they're called after completed successfully, we don't count them
                var bytesRead = valueTask.Result; // FN
                var once = valueTask.GetAwaiter().GetResult(); // FN
            }
        }

        public void Foo9(ValueTaskProvider stream)
        {
            int bytesRead;
            ValueTask<int> valueTask = stream.ReadAsync();
            if (valueTask.IsCompletedSuccessfully)
            {
                // because they're called after completed successfully, we don't count them
                var once = valueTask.GetAwaiter().GetResult(); // FN
                bytesRead = valueTask.Result; // FN
            }
        }

        public void Foo10(ValueTaskProvider stream)
        {
            int bytesRead;
            ValueTask<int> valueTask = stream.ReadAsync();
            if (valueTask.IsCompletedSuccessfully)
            {
                // because they're called after completed successfully, we don't count them
                var once = valueTask.GetAwaiter().GetResult(); // FN
                var twice = valueTask.GetAwaiter().GetResult(); // FN
            }
        }

        public async void Foo11(ValueTaskProvider stream, bool b, string s, int o)
        {
            var valueTask = stream.ReadAsync();
            var once = await valueTask; // Noncompliant
            if (b)
            {
                while (o > 0)
                {
                    if (o == 10)
                    {
                        return;
                    }
                    if (s.Contains(o + ""))
                    {
                        stream.Foo(await valueTask); // Secondary
                    }
                    --o;
                }
            }
        }

        public void Foo12(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            if (valueTask.IsCompleted) // we don't know if it's successful
            {
                var once = valueTask.Result; // Noncompliant
            }
        }

        public void Foo13(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            if (valueTask.IsCompleted) // we don't know if it's successful
            {
                var once = valueTask.GetAwaiter().GetResult(); // Noncompliant
            }
        }

        // two different symbols , one bad , one ok
        public async void Foo14(ValueTaskProvider stream)
        {
            var firstValueTask = stream.ReadAsync();
            var once = firstValueTask.GetAwaiter().GetResult(); // Noncompliant

            int bytesRead;
            ValueTask<int> secondReadTask = stream.ReadAsync();
            if (secondReadTask.IsCompletedSuccessfully)
            {
                bytesRead = secondReadTask.Result; // Compliant, has completed successfully
            }
            else
            {
                bytesRead = await secondReadTask;
            }
        }

        // two different symbols , both bad
        public void Foo15(ValueTaskProvider stream)
        {
            var firstValueTask = stream.ReadAsync();
            var firstValueTaskR1 = firstValueTask.AsTask(); // Noncompliant
            var firstValueTaskR2 = firstValueTask.AsTask(); // Secondary

            var secondValueTask = stream.ReadAsync();
            var secondValueTaskR1 = secondValueTask.AsTask(); // Noncompliant
            var secondValueTaskR2 = secondValueTask.AsTask(); // Secondary
        }

        public void Foo16()
        {
            var stream = new ValueTaskProvider();
            var awaiter = stream.ReadAsync().GetAwaiter();
            awaiter.GetResult(); // Noncompliant
        }

        public async void Foo17(ValueTask<string> valueTask)
        {
            string x;
            if (!valueTask.IsCompletedSuccessfully)
            {
                x = await valueTask; // Noncompliant
                x = await valueTask; // Secondary
            }
        }

        public async void Foo18(ValueTask<string> valueTask)
        {
            string x;
            x = await valueTask; // Noncompliant
            x = valueTask.Result; // Secondary
// Noncompliant@-1
            if (!valueTask.IsCompletedSuccessfully)
            {
                 x = await valueTask; // Secondary
            }
            x = valueTask.Result; // FN , after if
        }

        public void FooFalseNegative(ValueTaskProvider stream)
        {
            int bytesRead;
            ValueTask<int> valueTask = stream.ReadAsync();
            GetResult(valueTask); // FN - we don't inspect inside the method body
            GetResult(valueTask); // FN
        }

        public async void FooFalseNegative2(ValueTask<string> valueTask)
        {
            string x;
            if (!valueTask.IsCompletedSuccessfully)
            {
                 x = await valueTask;
            }
            x = valueTask.Result; // FN - checked above in the if, we are blind now
            x = valueTask.Result; // FN
            x = valueTask.Result; // FN
            x = valueTask.Result; // FN
        }

        private async Task<int> GetResult(ValueTask<int> valueTask) => await valueTask;

        public async void Compliant1(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            var once = await valueTask;
        }

        public void Compliant2(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            var once = valueTask.AsTask();
        }

        public void Compliant3(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            if (valueTask.IsCompletedSuccessfully)
            {
                var once = valueTask.Result;
            }
        }

        public void Compliant4(ValueTask<int> valueTask)
        {
            if (valueTask.IsCompletedSuccessfully)
            {
                var once = valueTask.GetAwaiter().GetResult();
            }
        }

        public async void Compliant5(ValueTaskProvider stream, bool b, string s, int o)
        {
            var valueTask = stream.ReadAsync();
            var once = await valueTask;
            if (b)
            {
                while (o > 0)
                {
                    if (o == 10)
                    {
                        return;
                    }
                    if (valueTask.IsCompletedSuccessfully)
                    {
                        valueTask.GetAwaiter().GetResult();
                    }
                    --o;
                }
            }
        }

        public async void Compliant6(ValueTaskProvider stream)
        {
            int bytesRead;
            ValueTask<int> readTask = stream.ReadAsync();
            if (readTask.IsCompletedSuccessfully)
            {
                bytesRead = readTask.Result; // Compliant, has completed successfully
            }
            else
            {
                bytesRead = await readTask;
            }
        }

        public async void Compliant7(ValueTaskProvider stream)
        {
            int result = await stream.ReadAsync();

            result = await stream.ReadAsync().ConfigureAwait(false);

            Task<int> t = stream.ReadAsync().AsTask();
            var r1 = t.Result;
            var r2 = t.Result;
            var r3 = await t;
            var r4 = await t;

            var valueTask = stream.ReadAsync();
            var task = valueTask.AsTask();
            var t1 = task.Result;
            var t2 = task.Result;
        }

        public async void Compliant8(ValueTask<string> valueTask)
        {
            string x;
            if (!valueTask.IsCompletedSuccessfully)
            {
                 x = await valueTask;
            }
            x = valueTask.Result;
        }
    }

    class DifferentSyntaxes
    {
        public void Foo1(ValueTask<string> valueTask)
        {
            LocalMethod();

            async void LocalMethod()
            {
                var once = await valueTask; // Noncompliant
                var twice = await valueTask; // Secondary
            }
        }

        public int Foo2(ValueTask<int> valueTask) => valueTask.Result; // Noncompliant

        private ValueTask<int> valueTask;
        public int Property
        {
            get => this.valueTask.Result; // Noncompliant
        }


        public string Foo3(ValueTask<string> valueTask)
        {
            System.Func<string> anonMethod = () => valueTask.Result; // Noncompliant

            return anonMethod();
        }

        public DifferentSyntaxes(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            var once = valueTask.Result; // Noncompliant
        }


        ~DifferentSyntaxes()
        {
            var once = this.valueTask.Result; // Noncompliant
        }

        public static implicit operator int(DifferentSyntaxes f)
        {
            return f.valueTask.Result; // Noncompliant
        }

        public static DifferentSyntaxes operator -(DifferentSyntaxes b, DifferentSyntaxes c)
        {
            c.valueTask.GetAwaiter().GetResult(); // FN
            return null;
        }

        public static DifferentSyntaxes operator +(DifferentSyntaxes b, DifferentSyntaxes c)
        {
            var once = b.valueTask.Result; // Noncompliant
            return null;
        }
    }

    class StressTest
    {
        public async void Foo18(ValueTask<string> valueTask)
        {
            string x;
            x = await valueTask; // Noncompliant
            x = await valueTask; // Secondary
            x = valueTask.Result; // Secondary
// Noncompliant@-1
            x = valueTask.Result; // Secondary
// Noncompliant@-1

            x = await valueTask; // Secondary
            x = await valueTask; // Secondary
            if (true && false && !valueTask.IsCompletedSuccessfully)
                x = await valueTask; // Secondary

            x = await valueTask; // Secondary
            x = await valueTask; // Secondary
            x = valueTask.Result;
            x = valueTask.Result;
            x = valueTask.Result;
            x = valueTask.Result;

            x = valueTask.Result;
            if (!valueTask.IsCompletedSuccessfully)
                if (!valueTask.IsCompletedSuccessfully)
                    if (!valueTask.IsCompletedSuccessfully)
                         x = await valueTask; // Secondary

        }
    }

    // https://sonarsource.atlassian.net/browse/NET-1360
    internal class Repro_1360
    {
        public static async Task MethodCallChain()
        {
            var stream = new ValueTaskProvider();
            var t1 = stream.ReadAsync().AsTask(); // Noncompliant FP ReadAsync returns a new ValueTask on each invocation. t1 and t2 are not the same
            var t2 = stream.ReadAsync().AsTask(); // Secondary

            await Task.WhenAll(t1, t2);
        }

        public static async Task StaticMethodCallChain()
        {
            var t1 = Repro_1360.StaticValueTask().AsTask(); // Noncompliant FP
            var t2 = Repro_1360.StaticValueTask().AsTask(); // Secondary

            await Task.WhenAll(t1, t2);
        }

        public static async Task MethodCallChainThreeMethods()
        {
            var t1 = Repro_1360.Factory().InstanceValueTask().AsTask(); // Compliant
            var t2 = Repro_1360.Factory().InstanceValueTask().AsTask();

            await Task.WhenAll(t1, t2);
        }

        public static async Task MethodCallChainTwoMethods()
        {
            var c = Repro_1360.Factory();
            var t1 = c.InstanceValueTask().AsTask(); // Noncompliant FP
            var t2 = c.InstanceValueTask().AsTask(); // Secondary

            await Task.WhenAll(t1, t2);
        }

        public ValueTask<int> InstanceValueTask() => new ValueTask<int>(1);
        public static ValueTask<int> StaticValueTask() => new ValueTask<int>(1);
        public static Repro_1360 Factory() => new Repro_1360();
    }
}
