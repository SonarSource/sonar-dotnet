﻿using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class Foo : EventArgs { }

    public record EventHandlerCasesInRecord
    {
        async void MyMethod() // Noncompliant {{Return 'Task' instead.}}
//            ^^^^
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod(object sender, EventArgs args)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod1(object o, EventArgs e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod2(object o, Foo e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        public event EventHandler<bool> MyEvent;

        public EventHandlerCasesInRecord()
        {
            MyEvent += EventHandlerCases_MyEvent;
        }

        private async void EventHandlerCases_MyEvent(object sender, bool e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        private async void NotAHandler(object sender) // Noncompliant
//                    ^^^^
        {
            await Task.Run(() => Console.WriteLine("test"));
        }
    }

    public record EventHandlerCasesInPositionalRecord(string Param)
    {
        async void MyMethod() // Noncompliant
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod(object sender, EventArgs args)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod1(object o, EventArgs e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod2(object o, Foo e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        private async void NotAHandler(object sender) // Noncompliant
        {
            await Task.Run(() => Console.WriteLine("test"));
        }
    }

    public interface EventHandlerCasesInInterface
    {
        async void MyMethod() // Compliant FP , see https://github.com/SonarSource/sonar-dotnet/issues/4396
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod(object sender, EventArgs args)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod1(object o, EventArgs e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        async void MyMethod2(object o, Foo e)
        {
            await Task.Run(() => Console.WriteLine("test"));
        }

        private async void NotAHandler(object sender) // Noncompliant
        {
            await Task.Run(() => Console.WriteLine("test"));
        }
    }
}
