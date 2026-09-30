using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

#nullable enable

namespace MemberShouldBeStatic
{
    // https://sonarsource.atlassian.net/browse/NET-4640
    public partial class NewAppVersionAvailableInfoBanner
    {
        private async void TapGestureRecognizer_OnTapped(object? sender, TappedEventArgs e) // Compliant: wired from XAML
        {
            await Task.CompletedTask;
        }

        private void EventArgs_Handler(object sender, EventArgs e) => Console.WriteLine();
        private void TappedEventArgs_Handler(object sender, TappedEventArgs e) => Console.WriteLine();
        private partial void MauiElement_Event(object? sender, EventArgs e) => Console.WriteLine(); // Compliant: partial event handler

        private void MethodWithNoArguments() => Console.WriteLine();                                              // Noncompliant
        private void MethodWith1Argument(object sender) => Console.WriteLine();                                   // Noncompliant
        private void MethodWith3Arguments(object sender, TappedEventArgs e, string other) => Console.WriteLine(); // Noncompliant
        private void SenderArgumentNotObject(string sender, TappedEventArgs e) => Console.WriteLine();            // Noncompliant
        private void SecondArgumentNotEventArgs(object sender, object e) => Console.WriteLine();                  // Noncompliant
        private int OnButtonClickHelper(object sender, EventArgs e) => 0;                                         // Noncompliant
        private Task TaskReturningMethod(object sender, TappedEventArgs e) => Task.CompletedTask;                 // Noncompliant
        private int Value => 42;                                                                                  // Noncompliant
    }

    public class MauiElement : Microsoft.Maui.Controls.Element
    {
        private void EventArgs_Handler(object sender, EventArgs e) => Console.WriteLine(); // Compliant: derives from the MAUI Element
    }

    // https://sonarsource.atlassian.net/browse/NET-4640
    public class App : Application
    {
        private void EventArgs_Handler(object sender, EventArgs e) => Console.WriteLine(); // Compliant: Application derives from the MAUI Element
    }

    public class MauiBindableObject : BindableObject // Does not derive from the MAUI Element
    {
        private void EventArgs_Handler(object sender, EventArgs e) => Console.WriteLine();  // Noncompliant
    }

    public interface INotMauiElement
    {
        void Work(object sender, EventArgs e);
    }

    public partial class NotMauiElement : INotMauiElement
    {
        public void Work(object sender, EventArgs e) => Console.WriteLine();                            // Compliant: implements an interface member
        private partial void NotMauiElement_Event(object? sender, EventArgs e) => Console.WriteLine();  // Compliant: partial method

        private void EventArgs_Handler(object sender, EventArgs e) => Console.WriteLine();              // Noncompliant
        private void TappedEventArgs_Handler(object sender, TappedEventArgs e) => Console.WriteLine();  // Noncompliant
    }

    public class NonMauiElement : MyCustomNamespace.Element
    {
        private void EventArgs_Handler(object sender, EventArgs e) => Console.WriteLine(); // Noncompliant
    }
}

namespace MyCustomNamespace
{
    public class Element { }
}
