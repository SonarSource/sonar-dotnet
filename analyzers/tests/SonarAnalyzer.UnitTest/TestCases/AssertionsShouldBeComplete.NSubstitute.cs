using NSubstitute;

public interface ICommand
{
    void Execute();
}

namespace NSubstituteTests
{
    internal class Tests
    {
        public void Received()
        {
            var command = Substitute.For<ICommand>();
            command.Received();                         // Noncompliant {{Complete the assertion}}
            //      ^^^^^^^^
            command.Received(requiredNumberOfCalls: 2); // Noncompliant
            command.Received<ICommand>();               // Noncompliant
            SubstituteExtensions.Received(command);     // Noncompliant

            command.Received().Execute();
        }

        public void DidNotReceive()
        {
            var command = Substitute.For<ICommand>();
            command.DidNotReceive();           // Noncompliant {{Complete the assertion}}
            //      ^^^^^^^^^^^^^
            command.DidNotReceive<ICommand>(); // Noncompliant

            command.DidNotReceive().Execute();
        }

        public void ReceivedWithAnyArgs()
        {
            var command = Substitute.For<ICommand>();
            command.ReceivedWithAnyArgs();                         // Noncompliant {{Complete the assertion}}
            //      ^^^^^^^^^^^^^^^^^^^
            command.ReceivedWithAnyArgs(requiredNumberOfCalls: 2); // Noncompliant
            command.ReceivedWithAnyArgs<ICommand>();               // Noncompliant

            command.ReceivedWithAnyArgs().Execute();
        }

        public void DidNotReceiveWithAnyArgs()
        {
            var command = Substitute.For<ICommand>();
            command.DidNotReceiveWithAnyArgs();           // Noncompliant {{Complete the assertion}}
            //      ^^^^^^^^^^^^^^^^^^^^^^^^
            command.DidNotReceiveWithAnyArgs<ICommand>(); // Noncompliant

            command.DidNotReceiveWithAnyArgs().Execute();
        }

        public void ReceivedCalls()
        {
            var command = Substitute.For<ICommand>();
            command.ReceivedCalls();           // Noncompliant {{Complete the assertion}}
            //      ^^^^^^^^^^^^^
            command.ReceivedCalls<ICommand>(); // Noncompliant

            var calls = command.ReceivedCalls();
        }
    }
}

namespace OtherReceived
{
    public static class OtherExtensions
    {
        public static void Received<T>(this T something) { }
    }

    public class Test
    {
        public void Received(ICommand command)
        {
            command.Received(); // Compliant. Other Received call
        }
    }
}
