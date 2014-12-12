using System;

/* Noncompliant:
hello
TODO */

// Noncompliant: hello TODO world

// ok


/// Noncompliant:
/// <summary>
/// TODO
/// </summary>

/** Noncompliant:
toDo
*/

/**
*/
namespace Tests.Diagnostics
{
}
