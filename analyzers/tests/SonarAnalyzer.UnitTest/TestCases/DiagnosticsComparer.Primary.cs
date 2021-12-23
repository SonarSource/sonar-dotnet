public class FirstClass // Noncompliant
{
    async void CorrectNoncompliant() { } // Noncompliant {{Return 'Task' instead.}}
    //    ^^^^

    async void Unexpected() { }

    async void WrongMessage() { } // Noncompliant {{Wrong message}}

    async void WrongPlaceWrongMessage() { } // Noncompliant {{Wrong message, wrong place}}
    //         ^^^^^^^^^^

    async void WrongPlace() { } // Noncompliant {{Return 'Task' instead.}}
    //         ^^^^^^^^^^

    async void WrongPlaceNoMessage() { } // Noncompliant
    //         ^^^^^^^^^^

    async void WrongLength() { } // Noncompliant {{Return 'Task' instead.}}
    //    ^^^^^
}
