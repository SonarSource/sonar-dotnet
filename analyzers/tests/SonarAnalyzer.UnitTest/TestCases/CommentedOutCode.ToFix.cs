class Compliant
{
    int SingleLine() { return 42; } // Single line comment.
    int BlockComment() { return 42; } /* Single line comment. */
}

class Noncompliant
{
    // Noncompliant@+1
    int SingleLine() { return 42; }// return 17;
}
