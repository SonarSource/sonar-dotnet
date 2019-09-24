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
            var once = await valueTask; // Noncompliant { "Refactor this 'ValueTask' usage to consume it only once." }
//                           ^^^^^^^^^
            var twice = await valueTask;
//                            ^^^^^^^^^ Secondary
        }

        public void Foo2(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            var once = valueTask.AsTask(); // Noncompliant
            var twice = valueTask.AsTask(); // Secondary
        }

        public async void Foo3(ValueTaskProvider stream)
        {
            var valueTask = stream.ReadAsync();
            var once = await valueTask; // Noncompliant { "Refactor this 'ValueTask' usage to consume it only once." }
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
            var once = valueTask.Result; // Noncompliant {"Refactor this 'ValueTask' usage to consume the result only if the operation has completed successfully."}
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
                bytesRead = valueTask.Result; // Noncompliant
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

        public void FooFalseNegative(ValueTaskProvider stream)
        {
            int bytesRead;
            ValueTask<int> valueTask = stream.ReadAsync();
            GetResult(valueTask); // FN - we don't inspect inside the method body
            GetResult(valueTask); // FN

            var awaiter = stream.ReadAsync().GetAwaiter();
            awaiter.GetResult(); // FN - we don't track the variable source
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

    }
}
