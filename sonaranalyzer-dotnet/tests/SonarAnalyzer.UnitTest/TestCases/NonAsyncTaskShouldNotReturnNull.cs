﻿using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class CompliantUseCases
    {
        public async Task GetTaskAsync1()
        {
            return null; // async
        }

        public async Task<object> GetTaskAsync2()
        {
            return null; // async
        }

        public Task GetTask1()
        {
            return Task.FromResult(true);
        }

        public Task GetTask2()
        {
            var x = new Task<object>(() => null); // Compliant

            return Task.Delay(0);
        }

        public Task Foo { get; set; }

        public Task<object> GetFooAsync()
        {
            return Task.Run(() =>
            {
                if (false)
                {
                    return new object();
                }
                else
                {
                    return null;
                }
            });
        }
    }

    public class NonCompliantUseCases
    {
        public Task GetTask1()
        {
            return null; // Noncompliant {{Do not return null from this method, instead return 'Task.FromResult<T>(null)', 'Task.CompletedTask' or 'Task.Delay(0)'.}}
//                 ^^^^
        }

        public Task<object> GetTask2()
        {
            return null; // Noncompliant
        }

        public Task GetTaskAsync3(int a)
        {
            if (a > 42)
            {
                return null; // Noncompliant
            }

            return Task.Delay(0);
        }

        public Task<string> GetTask4() => null; // Noncompliant

        public Task<string> GetTask5(int a)
        {
            if (a > 0)
            {
                return null; // Noncompliant
            }
            else
            {
                return null; // Noncompliant
            }
        }

        public Task GetTask6()
        {
            return (null); // Noncompliant
        }

        public Task GetTask7(bool condition)
        {
            return condition ? Task.FromResult(5) : null; // Should be non-compliant
        }

        public Task Foo
        {
            get
            {
                return null; // Noncompliant
            }
            set
            {
                foo = value;
            }
        }

        public Task<int> AgeAsync
        {
            get => null; // Noncompliant
        }
    }
}
