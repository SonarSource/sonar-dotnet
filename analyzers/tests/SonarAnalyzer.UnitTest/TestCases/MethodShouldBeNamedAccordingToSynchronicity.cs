using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Tests.Diagnostics
{
    public class NonCompliantCases
    {
        public ValueTask<T> FooValueTaskT<T>() // Noncompliant
        {
            return default(ValueTask<T>);
        }

        public Task FooTask() // Noncompliant {{Add the 'Async' suffix to the name of this method.}}
//                  ^^^^^^^
        {
            return Task.Delay(0);
        }

        public Task<int> FooTaskInt() // Noncompliant
        {
            return Task.FromResult(1);
        }

        public Task<T> FooTaskT<T>() // Noncompliant
//                     ^^^^^^^^
        {
            return Task.FromResult(default(T));
        }

        public void BarVoidAsync() // Noncompliant {{Remove the 'Async' suffix to the name of this method.}}
//                  ^^^^^^^^^^^^
        {
        }

        public int BarIntAsync() // Noncompliant
        {
            return 1;
        }

        public object BarObjectAsync() // Noncompliant
        {
            return null;
        }

        public Task MyMethodAsync()
        {
            if (true)
            {
                return OtherSubMethod();
            }

            return SubMethod();

            Task SubMethod() => Task.Delay(0); // Compliant - should not be but requires C# 7 syntax

            Task OtherSubMethod()// Compliant - should not be but requires C# 7 syntax
            {
                return Task.Delay(0);
            }
        }
    }

    public interface IFoo
    {
        Task Do(); // Noncompliant
    }

    public class BaseClass : IFoo
    {
        public virtual Task<string> MyMethod()// Noncompliant
        {
            return Task.FromResult("foo");
        }

        public Task Do() // Compliant - comes from interface so not possible to change
        {
            return null;
        }
    }

    public class CompliantCases : BaseClass
    {
        public Task FooTaskAsync()
        {
            return Task.Delay(0);
        }

        public Task<int> FooTaskIntAsync()
        {
            return Task.FromResult(1);
        }

        public Task<T> FooTaskTAsync<T>()
        {
            return Task.FromResult(default(T));
        }

        public void BarVoid()
        {
        }

        public int BarInt()
        {
            return 1;
        }

        public object BarObject()
        {
            return null;
        }

        public override Task<string> MyMethod()
        {
            return Task.FromResult("foo");
        }
    }

    public class Program1
    {
        public static async Task Main() { }
    }

    public class Program2
    {
        public static async Task<int> Main() { return 0; }
    }

    public class Program3
    {
        public static async Task Main(string[] args) { }
    }

    public class Program4
    {
        public static async Task<int> Main(string[] args) { return 0; }
    }

    public class TestAttributes
    {
        [System.ComponentModel.Browsable(true)]
        public async Task OtherAttributes() { } // Noncompliant
    }

    public class MyHub : Hub
    {
        public MyHub()
        {
        }

        public Task<string> MyMethod() // Compliant - Public methods from types derived from `Microsoft.AspNet.SignalR.Hub` are considered an exception.
        {
            return Task.FromResult("foo");
        }

        private Task<string> PrivateMethod() // Noncompliant
        {
            return Task.FromResult("foo");
        }
    }

    public interface IChatClient
    {
        Task ReceiveMessage(string user, string message); // Noncompliant

        Task ReceiveMessage(string message); // Noncompliant
    }

    public class StronglyTypedChatHub : Hub<IChatClient>
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.ReceiveMessage(user, message);
        }

        public Task SendMessageToCaller(string message)
        {
            return Clients.Caller.ReceiveMessage(message);
        }

        public Task ThrowException()
        {
            throw new HubException("This error will be sent to the client!");
        }
    }

    public unsafe class PointerReturnType
    {
        public int* MethodWithPointerReturnTypeAsync(int* input)  // Noncompliant
        {
            return input;
        }
    }

    // See https://github.com/SonarSource/sonar-dotnet/issues/4799
    public class AsyncCouldStay
    {
        public T GenericTaskTAsync<T>(T input) where T : Task
        {
            return input;
        }

        public T GenericTaskT2Async<T, V>(V input) where V : Task // Noncompliant
        {
            return default(T);
        }
    }

    public class GenericWithoutConstraint
    {
        public T GenericTaskTAsync<T>(T input) // Noncompliant
        {
            return input;
        }
    }

    public class GenericWithDifferentConstraint
    {
        public T GenericTaskTAsync<T>(T input) where T : new() // Noncompliant
        {
            return input;
        }
    }
}
