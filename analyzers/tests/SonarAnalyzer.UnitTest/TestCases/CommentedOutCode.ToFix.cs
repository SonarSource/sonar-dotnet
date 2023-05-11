// return "Some code at the start of a file".Trim();
// Noncompliant@-1
class Compliant
{
    int SingleLine() { return 42; } // Single line comment.
    int BlockComment() { return 42; } /* Block comment. */
}

class Noncompliant
{
    // Noncompliant@+1
    int SingleLine() { return 42; }// return 42;

    // Noncompliant@+1
    int SingleLineWithSpacing() { return 42; } // return 42;

    // Noncompliant@+2
    int SeperateLine() { return 42; }
    // return 42;

    // Noncompliant@+2
    int MultipleLines() { return 42; }
    // int OldImplementation()
    // {
    //     return 42;
    // }

    // Noncompliant@+1
    int SingleLineBlock() { return 42; }/* return 42;*/

    // Noncompliant@+2
    int SeperateLineBlock() { return 42; }
    /*
     * return 42;
     */
}
