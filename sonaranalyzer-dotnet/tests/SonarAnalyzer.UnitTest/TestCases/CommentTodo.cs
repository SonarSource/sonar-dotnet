using System;

/*
hello
todo -  // Noncompliant
TODO */ // Noncompliant

// Noncompliant: hello TODO world

// ok


// Noncompliant@+3
///
/// <summary>
/// TODO -
/// </summary>

// Noncompliant@+2
/**
toDo -
*/

// The following should be compliant:
// aaaTODO000

// Noncompliant@+1
// !TODO!

/*TODO*/ // Noncompliant
//^^^^

/**
*/
namespace Tests.Diagnostics
{
    class Todo
    {
        public Todo()
        {
        }
    }
}
