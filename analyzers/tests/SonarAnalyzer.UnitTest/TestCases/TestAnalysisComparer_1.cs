namespace Tests.Diagnostics
{
    public class FirstClass // Noncompliant
    {
        async void CorrectNoncompliant() { } // Noncompliant {{Return 'Task' instead.}}
        //    ^^^^

        async void Unexpected() { }

        async void WrongMessage() { } // Noncompliant {{Wrong message}}

        async void WrongPlace() { } // Noncompliant {{Return 'Task' instead.}}
        //         ^^^^^^^^^^

        async void WrongLength() { } // Noncompliant {{Return 'Task' instead.}}
        //    ^^^^^
    }
}
