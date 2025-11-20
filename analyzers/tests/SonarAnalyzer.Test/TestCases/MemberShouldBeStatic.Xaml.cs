using System;
using System.Windows;

namespace MemberShouldBeStatic
{
    // https://github.com/SonarSource/sonar-dotnet/issues/9695
    public partial class Rerpo_9695 : FrameworkElement
    {
        private void MethodWith1Argument(object sender) { Handle(); }                              // Noncompliant
        private void MethodWith3Argument(object sender, MyEventArgs e, string other) { Handle(); } // Noncompliant
        private void SenderArgumentNotObject(string input, MyEventArgs e) { Handle(); }            // Noncompliant
        private void EventArgs_Handler(object sender, EventArgs e) { Handle(); }                   // Compliant
        private void MyEventArgs_Handler(object sender, MyEventArgs e) { Handle(); }               // Compliant
        static void Handle() { }
    }

    public class NoFrameworkElement
    {
        private void EventArgs_Handler(object sender, EventArgs e) { Handle(); }     // Noncompliant
        private void MyEventArgs_Handler(object sender, MyEventArgs e) { Handle(); } // Noncompliant
        static void Handle() { }
    }

    public class MyEventArgs : EventArgs { }

    public class App : Application
    {
        private void MethodWith1Argument(object sender) { Handle(); }                               // Noncompliant
        private void MethodWith3Argument(object sender, MyEventArgs e, string other) { Handle(); }  // Noncompliant
        private void SenderArgumentNotObject(string input, MyEventArgs e) { Handle(); }             // Noncompliant
        private void EventArgs_Handler(object sender, EventArgs e) { Handle(); }                    // Noncompliant FP
        private void MyEventArgs_Handler(object sender, MyEventArgs e) { Handle(); }                // Noncompliant FP
        private void App_OnStartup(object sender, StartupEventArgs e)                               // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2677
        {
            Console.WriteLine("Hello World!");
        }
        static void Handle() { }
    }
}
